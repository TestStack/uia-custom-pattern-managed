using System;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;
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

        [Guid(TestPatternProviderComGuid)]
        [PatternGuid(TestPatternGuid)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAttrDrivenTestProvider
        {
            [PatternProperty(TestPatternBoolPropertyGuid)]
            bool BoolProperty { get; }

            [PatternMethod]
            void VoidParameterlessMethod();

            [PatternMethod(DoSetFocus = true)]
            bool BoolParameterlessMethodWithDoSetFocus();

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

            void VoidParameterlessMethod();
            bool BoolParameterlessMethodWithDoSetFocus();
            bool BoolMethodWithInAndOutParams(int intIn, out string stringOut);
        }

        [Test]
        public void AssertGuidsAreReflectedCorrectly()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            Assert.AreEqual(new Guid(TestPatternProviderComGuid), schema.PatternProviderGuid);
            Assert.AreEqual(new Guid(TestPatternClientComGuid), schema.PatternClientGuid);
            Assert.AreEqual(new Guid(TestPatternGuid), schema.PatternGuid);
        }

        [Test]
        public void AssertRegistrationGoesSmoothly()
        {
            var schema = new AttributeDrivenPatternSchema(typeof(IAttrDrivenTestProvider), typeof(IAttrDrivenTestPattern));
            schema.Register();
        }

        [Test]
        public void PropertiesAndMethodsAreMappedCorrectly()
        {
            var schema = new AttributeDrivenPatternSchema(typeof (IAttrDrivenTestProvider), typeof (IAttrDrivenTestPattern));

            var props = schema.Properties;
            Assert.AreEqual(1, props.Length);
            AssertPropertyInfo("BoolProperty", UIAutomationType.UIAutomationType_Bool, TestPatternBoolPropertyGuid, props[0]);

            Assert.AreEqual(3, schema.Methods.Length);
            
            var voidParamlessMethod = schema.Methods[0];
            Assert.AreEqual(0, voidParamlessMethod.Data.cInParameters);
            Assert.AreEqual(0, voidParamlessMethod.Data.cOutParameters);

            var boolParamlessMethodWithSetFocus = schema.Methods[1];
            Assert.AreEqual(1, boolParamlessMethodWithSetFocus.Data.doSetFocus);
            Assert.AreEqual(0, boolParamlessMethodWithSetFocus.Data.cInParameters);
            Assert.AreEqual(1, boolParamlessMethodWithSetFocus.Data.cOutParameters);
            Assert.AreEqual(UIAutomationType.UIAutomationType_OutBool, boolParamlessMethodWithSetFocus.OutParamTypes[0]);

            var boolMethodWithInOutParams = schema.Methods[2];
            Assert.AreEqual(1, boolMethodWithInOutParams.Data.cInParameters);
            Assert.AreEqual(2, boolMethodWithInOutParams.Data.cOutParameters);
            Assert.AreEqual(UIAutomationType.UIAutomationType_Int, boolMethodWithInOutParams.InParamTypes[0]);
            Assert.AreEqual(UIAutomationType.UIAutomationType_OutBool, boolMethodWithInOutParams.OutParamTypes[0]);
            Assert.AreEqual(UIAutomationType.UIAutomationType_OutString, boolMethodWithInOutParams.OutParamTypes[1]);
        }

        private void AssertPropertyInfo(string programmaticName, UIAutomationType uiaType, string guid, UiaPropertyInfoHelper propInfo)
        {
            var data = propInfo.Data;
            Assert.AreEqual(programmaticName, data.pProgrammaticName);
            Assert.AreEqual(uiaType, data.type);
            Assert.AreEqual(new Guid(guid), data.guid);
        }
    }
}