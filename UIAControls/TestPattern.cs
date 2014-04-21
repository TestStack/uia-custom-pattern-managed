using System;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;
using UIAutomationClient;
using IRawElementProviderSimple = Interop.UIAutomationCore.IRawElementProviderSimple;

// Test Pattern
// Schema and implementation for the custom pattern that demonstrates several
// different supported parameter types

namespace UIAControls
{
    // Declaration of the provider-side interface, which the control will implement.
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("E7C4D124-E430-46B8-B9CC-1DED8BBDA0F2")]
    public interface ITestProvider
    {
        int IntValue { get; }
        string StringValue { get; }
        bool BoolValue { get; }
        double DoubleValue { get; }
        IRawElementProviderSimple ElementValue { get; }

        void PassIntParam(int value, out int retVal);
        void PassStringParam(string value, out string retVal);
        void PassBoolParam(bool value, out bool retVal);
        void PassDoubleParam(double value, out double retVal);
    }

    // Declaration of the client-side interface, for the client/test to use.
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("82A9C7E7-9C87-497C-ABD2-054A6A7BACA2")]
    public interface ITestPattern
    {
        int CurrentIntValue { get; }
        string CurrentStringValue { get; }
        bool CurrentBoolValue { get; }
        double CurrentDoubleValue { get; }
        IUIAutomationElement CurrentElementValue { get; }

        int CachedIntValue { get; }
        string CachedStringValue { get; }
        bool CachedBoolValue { get; }
        double CachedDoubleValue { get; }
        IUIAutomationElement CachedElementValue { get; }

        void PassIntParam(int value, out int retVal);
        void PassStringParam(string value, out string retVal);
        void PassBoolParam(bool value, out bool retVal);
        void PassDoubleParam(double value, out double retVal);
    }

    /// <summary>
    /// Declaration of the pattern schema, with all of the information UIA needs
    /// about this property.
    /// </summary>
    public class TestSchema : CustomPatternSchemaBase
    {
        private static readonly TestSchema Instance = new TestSchema();

        public static TestSchema GetInstance()
        {
            return Instance;
        }

        public readonly UiaPropertyInfoHelper IntValueProperty =
            new UiaPropertyInfoHelper(
                new Guid("1898A775-726D-44AF-8F7B-A52814E46AC8"),
                "IntValue",
                UIAutomationType.UIAutomationType_Int);

        public readonly UiaPropertyInfoHelper StringValueProperty =
            new UiaPropertyInfoHelper(
                new Guid("83454F57-97C3-4740-B2CD-A5AA4FA40EA2"),
                "StringValue",
                UIAutomationType.UIAutomationType_String);

        // These function like properties, but are declared as methods
        // to work around the two-property limitation in Win7 UIA.
        // Win7 UIA does not seem to be able to process more than
        // two properties attached to a pattern.  Standalone properties work fine.
        // And we can also work around this by creating methods that
        // have a single out-parameter, which is what we're doing here.

        public readonly UiaMethodInfoHelper GetBoolValueMethod =
            new UiaMethodInfoHelper(
                "get_BoolValue",
                false /* doSetFocus */,
                new[]
                {
                    new UiaParameterDescription("retVal", UIAutomationType.UIAutomationType_OutBool)
                });

        public readonly UiaMethodInfoHelper GetDoubleValueMethod =
            new UiaMethodInfoHelper(
                "get_DoubleValue",
                false /* doSetFocus */,
                new[]
                {
                    new UiaParameterDescription("retVal", UIAutomationType.UIAutomationType_OutDouble)
                });

        public readonly UiaMethodInfoHelper GetElementValueMethod =
            new UiaMethodInfoHelper(
                "get_ElementValue",
                false /* doSetFocus */,
                new[]
                {
                    new UiaParameterDescription("retVal", UIAutomationType.UIAutomationType_OutElement)
                });

        public readonly UiaMethodInfoHelper PassIntParamMethod =
            new UiaMethodInfoHelper(
                "PassIntParam",
                true /* doSetFocus */,
                new[]
                {
                    new UiaParameterDescription("value", UIAutomationType.UIAutomationType_Int),
                    new UiaParameterDescription("retVal", UIAutomationType.UIAutomationType_OutInt)
                });

        public readonly UiaMethodInfoHelper PassStringParamMethod =
            new UiaMethodInfoHelper(
                "PassStringParam",
                true /* doSetFocus */,
                new[]
                {
                    new UiaParameterDescription("value", UIAutomationType.UIAutomationType_String),
                    new UiaParameterDescription("retVal", UIAutomationType.UIAutomationType_OutString)
                });

        public readonly UiaMethodInfoHelper PassBoolParamMethod =
            new UiaMethodInfoHelper(
                "PassBoolParam",
                true /* doSetFocus */,
                new[]
                {
                    new UiaParameterDescription("value", UIAutomationType.UIAutomationType_Bool),
                    new UiaParameterDescription("retVal", UIAutomationType.UIAutomationType_OutBool)
                });

        public readonly UiaMethodInfoHelper PassDoubleParamMethod =
            new UiaMethodInfoHelper(
                "PassDoubleParam",
                true /* doSetFocus */,
                new[]
                {
                    new UiaParameterDescription("value", UIAutomationType.UIAutomationType_Double),
                    new UiaParameterDescription("retVal", UIAutomationType.UIAutomationType_OutDouble)
                });

        public readonly UiaEventInfoHelper Test1Event =
            new UiaEventInfoHelper(
                new Guid("FDACD325-D5AE-4D80-AE13-81FA7793645B"),
                "Test1");

        public readonly UiaEventInfoHelper Test2Event =
            new UiaEventInfoHelper(
                new Guid("B7827175-069C-43D0-8D3A-843F42B846E1"),
                "Test2");

        public override UiaPropertyInfoHelper[] Properties
        {
            get
            {
                return new[]
                       {
                           IntValueProperty,
                           StringValueProperty,
                       };
            }
        }

        public override UiaMethodInfoHelper[] Methods
        {
            get
            {
                return new[]
                       {
                           GetBoolValueMethod,
                           GetDoubleValueMethod,
                           GetElementValueMethod,
                           PassIntParamMethod,
                           PassStringParamMethod,
                           PassBoolParamMethod,
                           PassDoubleParamMethod,
                       };
            }
        }

        public override UiaEventInfoHelper[] Events
        {
            get
            {
                return new[]
                       {
                           Test1Event,
                           Test2Event
                       };
            }
        }

        public override Guid PatternGuid
        {
            get { return new Guid("AD93BC6E-8BEC-4C29-9F4D-E820138FF43F"); }
        }

        public override Guid PatternClientGuid
        {
            get { return typeof (ITestPattern).GUID; }
        }

        public override Guid PatternProviderGuid
        {
            get { return typeof (ITestProvider).GUID; }
        }

        public override string PatternName
        {
            get { return "TestPattern"; }
        }

        public override IUIAutomationPatternHandler Handler
        {
            get { return new TestProviderHandler(); }
        }
    };

    // Pattern instance class: wrap up a IUIAutomationPatternInstance and implement the
    // custom pattern interface on top of it.
    public class TestProviderClientInstance : CustomClientInstanceBase, ITestPattern
    {
        public TestProviderClientInstance(IUIAutomationPatternInstance patternInstance)
            : base(patternInstance)
        {
        }

        public int CurrentIntValue
        {
            get { return (int) GetCurrentPropertyValue(TestSchema.GetInstance().IntValueProperty); }
        }

        public string CurrentStringValue
        {
            get { return (string) GetCurrentPropertyValue(TestSchema.GetInstance().StringValueProperty); }
        }

        public bool CurrentBoolValue
        {
            get
            {
                // Get the current property value via method, to work around the 2-property
                // limitation in Win7 UIA
                return (bool) GetCurrentPropertyValueViaMethod(TestSchema.GetInstance().GetBoolValueMethod);
            }
        }

        public double CurrentDoubleValue
        {
            get
            {
                // Get the current property value via method, to work around the 2-property
                // limitation in Win7 UIA
                return (double) GetCurrentPropertyValueViaMethod(TestSchema.GetInstance().GetDoubleValueMethod);
            }
        }

        public IUIAutomationElement CurrentElementValue
        {
            get
            {
                // Get the current property value via method, to work around the 2-property
                // limitation in Win7 UIA
                return (IUIAutomationElement) GetCurrentPropertyValueViaMethod(TestSchema.GetInstance().GetElementValueMethod);
            }
        }

        public int CachedIntValue
        {
            get { return (int) GetCachedPropertyValue(TestSchema.GetInstance().IntValueProperty); }
        }

        public string CachedStringValue
        {
            get { return (string) GetCachedPropertyValue(TestSchema.GetInstance().StringValueProperty); }
        }

        public bool CachedBoolValue
        {
            get
            {
                // Not supported, since Win7 UIA will not support more than 2 pattern properties
                throw new NotImplementedException();
            }
        }

        public double CachedDoubleValue
        {
            get
            {
                // Not supported, since Win7 UIA will not support more than 2 pattern properties
                throw new NotImplementedException();
            }
        }

        public IUIAutomationElement CachedElementValue
        {
            get
            {
                // Not supported, since Win7 UIA will not support more than 2 pattern properties
                throw new NotImplementedException();
            }
        }

        public void PassIntParam(int value, out int retVal)
        {
            // Create and init a parameter list
            // We can't just use the CallMethod helper because we have out-parameters
            var paramList = new UiaParameterListHelper(TestSchema.GetInstance().PassIntParamMethod);
            paramList[0] = value;

            // Call through
            PatternInstance.CallMethod(TestSchema.GetInstance().PassIntParamMethod.Index, paramList.Data, paramList.Count);

            // Get the out-parameter
            retVal = (int) paramList[1];
        }

        public void PassStringParam(string value, out string retVal)
        {
            // Create and init a parameter list
            // We can't just use the CallMethod helper because we have out-parameters
            var paramList = new UiaParameterListHelper(TestSchema.GetInstance().PassStringParamMethod);
            paramList[0] = value;

            // Call through
            PatternInstance.CallMethod(TestSchema.GetInstance().PassStringParamMethod.Index, paramList.Data, paramList.Count);

            // Get the out-parameter
            retVal = (string) paramList[1];
        }

        public void PassBoolParam(bool value, out bool retVal)
        {
            // Create and init a parameter list
            // We can't just use the CallMethod helper because we have out-parameters
            var paramList = new UiaParameterListHelper(TestSchema.GetInstance().PassBoolParamMethod);
            paramList[0] = value;

            // Call through
            PatternInstance.CallMethod(TestSchema.GetInstance().PassBoolParamMethod.Index, paramList.Data, paramList.Count);

            // Get the out-parameter
            retVal = (bool) paramList[1];
        }

        public void PassDoubleParam(double value, out double retVal)
        {
            // Create and init a parameter list
            // We can't just use the CallMethod helper because we have out-parameters
            var paramList = new UiaParameterListHelper(TestSchema.GetInstance().PassDoubleParamMethod);
            paramList[0] = value;

            // Call through
            PatternInstance.CallMethod(TestSchema.GetInstance().PassDoubleParamMethod.Index, paramList.Data, paramList.Count);

            // Get the out-parameter
            retVal = (double) paramList[1];
        }
    }

    /// <summary>
    /// Pattern handler class: creates pattern instances on client side and dispatches
    /// calls on the provider side.
    /// </summary>
    public class TestProviderHandler : IUIAutomationPatternHandler
    {
        public void CreateClientWrapper(IUIAutomationPatternInstance pPatternInstance, out object pClientWrapper)
        {
            pClientWrapper = new TestProviderClientInstance(pPatternInstance);
        }

        public void Dispatch(object pTarget, uint index, UIAutomationParameter[] pParams, uint cParams)
        {
            // Parse the provider and parameter list
            var provider = (ITestProvider) pTarget;
            var paramList = new UiaParameterListHelper(pParams, cParams);

            // Dispatch the method/property calls
            if (index == TestSchema.GetInstance().IntValueProperty.Index)
            {
                paramList[0] = provider.IntValue;
            }
            else if (index == TestSchema.GetInstance().StringValueProperty.Index)
            {
                paramList[0] = provider.StringValue;
            }
            else if (index == TestSchema.GetInstance().GetBoolValueMethod.Index)
            {
                paramList[0] = provider.BoolValue;
            }
            else if (index == TestSchema.GetInstance().GetDoubleValueMethod.Index)
            {
                paramList[0] = provider.DoubleValue;
            }
            else if (index == TestSchema.GetInstance().GetElementValueMethod.Index)
            {
                paramList[0] = provider.ElementValue;
            }
            else if (index == TestSchema.GetInstance().PassIntParamMethod.Index)
            {
                int retVal;
                provider.PassIntParam((int) paramList[0], out retVal);
                paramList[1] = retVal;
            }
            else if (index == TestSchema.GetInstance().PassStringParamMethod.Index)
            {
                string retVal;
                provider.PassStringParam((string) paramList[0], out retVal);
                paramList[1] = retVal;
            }
            else if (index == TestSchema.GetInstance().PassBoolParamMethod.Index)
            {
                bool retVal;
                provider.PassBoolParam((bool) paramList[0], out retVal);
                paramList[1] = retVal;
            }
            else if (index == TestSchema.GetInstance().PassDoubleParamMethod.Index)
            {
                double retVal;
                provider.PassDoubleParam((double) paramList[0], out retVal);
                paramList[1] = retVal;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    /// <summary>
    /// A sample implementation of ITestProvider.  
    /// This could be on the element provider itself, but it can also be separate,
    /// and keeping it separate keeps the main code cleaner,
    /// since this is pretty much just for testing.
    /// </summary>
    public class TestPatternProvider : ITestProvider
    {
        /// <summary>
        /// The host element of this pattern, from which the pattern was taken.
        /// </summary>
        private readonly IRawElementProviderSimple _hostElement;

        public TestPatternProvider(IRawElementProviderSimple hostElement)
        {
            _hostElement = hostElement;
        }

        public int IntValue
        {
            get { return 42; }
        }

        public string StringValue
        {
            get { return "TestString"; }
        }

        public bool BoolValue
        {
            get { return true; }
        }

        public double DoubleValue
        {
            get { return 3.1415; }
        }

        public IRawElementProviderSimple ElementValue
        {
            get { return _hostElement; }
        }

        public void PassIntParam(int value, out int retVal)
        {
            retVal = value;
        }

        public void PassStringParam(string value, out string retVal)
        {
            retVal = value;
        }

        public void PassBoolParam(bool value, out bool retVal)
        {
            retVal = value;
        }

        public void PassDoubleParam(double value, out double retVal)
        {
            retVal = value;
        }
    }
}