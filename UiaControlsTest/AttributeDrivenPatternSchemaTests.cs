using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;
using NSubstitute;
using NUnit.Framework;

namespace UiaControlsTest
{
    [TestFixture]
    public class AttributeDrivenPatternSchemaTests
    {
        private const string TestPatternProviderComGuid = "965D7E12-F5F5-42F9-9D72-75AAA7AE8FFD";
        private const string TestPatternClientComGuid = "267D23B7-6B12-4679-ACF0-E8FA0FB3BDD7";
        private const string TestPatternGuid = "E69F099B-7519-4CE7-9D61-77146DCB1B4A";
        private const string TestPatternBoolPropertyGuid = "DD339FFB-E244-41A2-A8A2-787F722C582B";
        private const string TestPatternIntPropertyGuid = "C6981328-E9B5-4EA1-AB21-A483D50D95BF";

        private static class Provider
        {
            private static string _dummyString;
            private static readonly IAttrDrivenTestProvider _dummyProvider = null;
            public static readonly PropertyInfo BoolPropertyProperty = TypeMember<IAttrDrivenTestProvider>.PropertyInfo(p => p.BoolProperty);
            public static readonly PropertyInfo IntPropertyProperty = TypeMember<IAttrDrivenTestProvider>.PropertyInfo(p => p.IntProperty);
            public static readonly MethodInfo VoidParameterlessMethod = ReflectionUtils.GetMethodInfo(() => _dummyProvider.VoidParameterlessMethod());
            public static readonly MethodInfo BoolParameterlessMethodWithDoSetFocus = ReflectionUtils.GetMethodInfo(() => _dummyProvider.BoolParameterlessMethodWithDoSetFocus());
            public static readonly MethodInfo IntMethodWithDoubleParam = ReflectionUtils.GetMethodInfo(() => _dummyProvider.IntMethodWithDoubleParam(0));
            public static readonly MethodInfo BoolMethodWithInAndOutParams = ReflectionUtils.GetMethodInfo(() => _dummyProvider.BoolMethodWithInAndOutParams(0, out _dummyString));
        }

        [Guid(TestPatternProviderComGuid)]
        [PatternGuid(TestPatternGuid)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAttrDrivenTestProvider
        {
            [PatternProperty(TestPatternBoolPropertyGuid)]
            bool BoolProperty { get; }

            [PatternProperty(TestPatternIntPropertyGuid)]
            int IntProperty { get; }

            [PatternMethod]
            void VoidParameterlessMethod();

            [PatternMethod(DoSetFocus = true)]
            bool BoolParameterlessMethodWithDoSetFocus();

            [PatternMethod]
            int IntMethodWithDoubleParam(double doubleIn);

            [PatternMethod]
            bool BoolMethodWithInAndOutParams(int intIn, out string stringOut);
        }

        [Guid(TestPatternClientComGuid)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAttrDrivenTestPattern
        {
            bool CurrentBoolProperty { get; }
            bool CachedBoolProperty { get; }
            int CurrentIntProperty { get; }
            int CachedIntProperty { get; }

            void VoidParameterlessMethod();
            bool BoolParameterlessMethodWithDoSetFocus();
            int IntMethodWithDoubleParam(double doubleIn);
            bool BoolMethodWithInAndOutParams(int intIn, out string stringOut);
        }

        [Test]
        public void AttributeDrivenPatternSchema_AssertGuidsAreReflectedCorrectly()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            Assert.AreEqual(new Guid(TestPatternProviderComGuid), schema.PatternProviderGuid);
            Assert.AreEqual(new Guid(TestPatternClientComGuid), schema.PatternClientGuid);
            Assert.AreEqual(new Guid(TestPatternGuid), schema.PatternGuid);
        }

        [Test]
        public void AttributeDrivenPatternSchema_AssertRegistrationGoesSmoothly()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            schema.Register();
        }

        [Test]
        public void AttributeDrivenPatternSchema_PropertiesAndMethodsAreMappedCorrectly()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));

            var props = schema.Properties;
            Assert.AreEqual(2, props.Length);
            AssertPropertyInfo("BoolProperty", UIAutomationType.UIAutomationType_Bool, TestPatternBoolPropertyGuid, props[0]);
            AssertPropertyInfo("IntProperty", UIAutomationType.UIAutomationType_Int, TestPatternIntPropertyGuid, props[1]);

            Assert.AreEqual(4, schema.Methods.Length);

            var voidParamlessMethod = schema.Methods.Single(m => m.Data.pProgrammaticName == Provider.VoidParameterlessMethod.Name);
            Assert.AreEqual(0, voidParamlessMethod.Data.cInParameters);
            Assert.AreEqual(0, voidParamlessMethod.Data.cOutParameters);

            var boolParamlessMethodWithSetFocus = schema.Methods.Single(m => m.Data.pProgrammaticName == Provider.BoolParameterlessMethodWithDoSetFocus.Name);
            Assert.AreEqual(1, boolParamlessMethodWithSetFocus.Data.doSetFocus);
            Assert.AreEqual(0, boolParamlessMethodWithSetFocus.Data.cInParameters);
            Assert.AreEqual(1, boolParamlessMethodWithSetFocus.Data.cOutParameters);
            Assert.AreEqual(UIAutomationType.UIAutomationType_OutBool, boolParamlessMethodWithSetFocus.OutParamTypes[0]);

            var boolMethodWithInOutParams = schema.Methods.Single(m => m.Data.pProgrammaticName == Provider.BoolMethodWithInAndOutParams.Name);
            Assert.AreEqual(1, boolMethodWithInOutParams.Data.cInParameters);
            Assert.AreEqual(2, boolMethodWithInOutParams.Data.cOutParameters);
            Assert.AreEqual(UIAutomationType.UIAutomationType_Int, boolMethodWithInOutParams.InParamTypes[0]);
            Assert.AreEqual(UIAutomationType.UIAutomationType_OutString, boolMethodWithInOutParams.OutParamTypes[0]);
            Assert.AreEqual(UIAutomationType.UIAutomationType_OutBool, boolMethodWithInOutParams.OutParamTypes[1]);

            var intMethodWithDoubleParam = schema.Methods.Single(m => m.Data.pProgrammaticName == Provider.IntMethodWithDoubleParam.Name);
            Assert.AreEqual(1, intMethodWithDoubleParam.Data.cInParameters);
            Assert.AreEqual(1, intMethodWithDoubleParam.Data.cOutParameters);
            Assert.AreEqual(UIAutomationType.UIAutomationType_Double, intMethodWithDoubleParam.InParamTypes[0]);
            Assert.AreEqual(UIAutomationType.UIAutomationType_OutInt, intMethodWithDoubleParam.OutParamTypes[0]);
        }

        private void AssertPropertyInfo(string programmaticName, UIAutomationType uiaType, string guid, UiaPropertyInfoHelper propInfo)
        {
            var data = propInfo.Data;
            Assert.AreEqual(programmaticName, data.pProgrammaticName);
            Assert.AreEqual(uiaType, data.type);
            Assert.AreEqual(new Guid(guid), data.guid);
        }

        [Test]
        public void PatternHandler_DispatchesPropertiesCorrectly()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            schema.Register();

            var paramHelper = new UiaParameterHelper(UIAutomationType.UIAutomationType_OutBool);
            var pParams = new[] {paramHelper.ToUiaParam()};

            var p = Substitute.For<IAttrDrivenTestProvider>();
            p.BoolProperty.Returns(true);
            schema.Handler.Dispatch(p, schema.Properties[0].Index, pParams, 1);
            Assert.AreEqual(true, paramHelper.Value);

            p.BoolProperty.Returns(false);
            schema.Handler.Dispatch(p, schema.Properties[0].Index, pParams, 1);
            Assert.AreEqual(false, paramHelper.Value);
        }

        [Test]
        public void UiaMethodInfoHelper_AddingInputParamAfterOutputOne_ThrowsAnException()
        {
            var inParam = new UiaParameterDescription("inInt", UIAutomationType.UIAutomationType_Int);
            var outParam = new UiaParameterDescription("outString", UIAutomationType.UIAutomationType_OutString);
            Assert.Throws<ArgumentException>(() => new UiaMethodInfoHelper("name", false, new[] {outParam, inParam}));
        }

        [Test]
        public void PatternHandler_VoidParameterlessMethodCalled_DispatchedCorrectly()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            schema.Register();

            var pParams = new UIAutomationParameter[0];

            var p = Substitute.For<IAttrDrivenTestProvider>();
            schema.Handler.Dispatch(p, schema.Methods[0].Index, pParams, 0);
            p.Received().VoidParameterlessMethod();
        }

        [Test]
        public void ClientWrapper_CurrentPropertyCalled_MakesCorrectRequestToPatternInstance()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            schema.Register();

            var patternInstance = Substitute.For<IUIAutomationPatternInstance>();
            var wrapper = GetClientWrapper(schema, patternInstance);

            var propHelper = schema.Properties.First(p => p.Data.pProgrammaticName == Provider.BoolPropertyProperty.Name);
            var verifier = ExpectCurrentPropertyCall(patternInstance, propHelper, returnValue: true);

            bool val = wrapper.CurrentBoolProperty;
            verifier();
            Assert.IsTrue(val);
        }

        [Test]
        public void ClientWrapper_CachedPropertyCalled_MakesCorrectRequestToPatternInstance()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            schema.Register();

            var patternInstance = Substitute.For<IUIAutomationPatternInstance>();
            var wrapper = GetClientWrapper(schema, patternInstance);

            var propHelper = schema.Properties.First(p => p.Data.pProgrammaticName == Provider.IntPropertyProperty.Name);
            var verifier = ExpectCachedPropertyCall(patternInstance, propHelper, returnValue: 42);

            var val = wrapper.CachedIntProperty;
            verifier();
            Assert.AreEqual(42, val);
        }

        [Test]
        public void ClientWrapper_BoolMethodWithIntAndOutStringParamsCalled_MakesCorrectRequestToPatternInstance()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            schema.Register();

            var patternInstance = Substitute.For<IUIAutomationPatternInstance>();
            var wrapper = GetClientWrapper(schema, patternInstance);

            var methodHelper = schema.Methods.First(m => m.Data.pProgrammaticName == Provider.BoolMethodWithInAndOutParams.Name);
            var inArgs = new object[] {42, null, null};
            var outArgs = new object[] {null, "abc", true};
            var verifier = ExpectMethodCall(patternInstance, methodHelper, inArgs, outArgs);

            string strResult;
            var boolResult = wrapper.BoolMethodWithInAndOutParams(42, out strResult);
            verifier();
            Assert.AreEqual("abc", strResult);
            Assert.AreEqual(true, boolResult);
        }

        private static Action ExpectCurrentPropertyCall<T>(IUIAutomationPatternInstance patternInstance, UiaPropertyInfoHelper propHelper, T returnValue)
        {
            return ExpectPropertyCall(patternInstance, propHelper, cached: false, returnValue: returnValue);
        }

        private static Action ExpectCachedPropertyCall<T>(IUIAutomationPatternInstance patternInstance, UiaPropertyInfoHelper propHelper, T returnValue)
        {
            return ExpectPropertyCall(patternInstance, propHelper, cached: true, returnValue: returnValue);
        }

        private static Action ExpectPropertyCall(IUIAutomationPatternInstance patternInstance, UiaPropertyInfoHelper propHelper, bool cached, object returnValue)
        {
            Action<IUIAutomationPatternInstance> substituteCall
                = instance => instance.GetProperty(propHelper.Index,
                                                   cached ? 1 : 0,
                                                   propHelper.UiaType,
                                                   Arg.Any<IntPtr>());
            patternInstance.When(substituteCall)
                           .Do(ci =>
                               {
                                   // imitate what the native UIA part does after server side returns result
                                   var marshalled = (IntPtr)ci.Args()[3];
                                   var paramHelper = new UiaParameterHelper(propHelper.UiaType, marshalled);
                                   paramHelper.Value = returnValue;
                               });

            return () =>
                   {
                       substituteCall(patternInstance.Received());
                       patternInstance.ClearReceivedCalls();
                   };
        }

        private static Action ExpectMethodCall(IUIAutomationPatternInstance patternInstance, UiaMethodInfoHelper methodHelper, object[] inArgs, object[] outArgs)
        {
            if (inArgs.Length != outArgs.Length)
                throw new ArgumentException();


            Action<IUIAutomationPatternInstance> substituteCall
                = instance => instance.CallMethod(methodHelper.Index,
                                                  Arg.Is<UIAutomationParameter[]>(pParams => DoParamsMatch(pParams, inArgs)),
                                                  (uint)inArgs.Length);
            patternInstance.When(substituteCall)
                           .Do(ci =>
                               {
                                   // imitate what the native UIA part does after server side finishes the call
                                   var marshalled = (UIAutomationParameter[])ci.Args()[1];
                                   var paramList = new UiaParameterListHelper(marshalled);
                                   for (int i = 0; i < marshalled.Length; i++)
                                   {
                                       if (UiaTypesHelper.IsOutType(marshalled[i].type))
                                           paramList[i] = outArgs[i];
                                   }
                               });

            return () =>
                   {
                       substituteCall(patternInstance.Received());
                       patternInstance.ClearReceivedCalls();
                   };
        }

        private static bool DoParamsMatch(UIAutomationParameter[] pParams, object[] inArgs)
        {
            if (pParams.Length != inArgs.Length) return false;
            var paramList = new UiaParameterListHelper(pParams);
            for (int i = 0; i < paramList.Count; i++)
            {
                if (UiaTypesHelper.IsInType(pParams[i].type))
                    if (!inArgs[i].Equals(paramList[i]))
                        return false;
            }
            return true;
        }

        private IAttrDrivenTestPattern GetClientWrapper(AttributeDrivenPatternSchema schema, IUIAutomationPatternInstance patternInstance)
        {
            object wrapperObj;
            schema.Handler.CreateClientWrapper(patternInstance, out wrapperObj);
            return (IAttrDrivenTestPattern)wrapperObj;
        }
    }
}
