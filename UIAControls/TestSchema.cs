using System;
using Interop.UIAutomationCore;

// Test Pattern
// Schema and implementation for the custom pattern that demonstrates several
// different supported parameter types
using ManagedUiaCustomizationCore;

namespace UIAControls
{
    // Declaration of the provider-side interface, which the control will implement.

    // Declaration of the client-side interface, for the client/test to use.

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
                UIAutomationType.UIAutomationType_Int,
                TypeMember<ITestProvider>.GetPropertyGetter(p => p.IntValue));

        public readonly UiaPropertyInfoHelper StringValueProperty =
            new UiaPropertyInfoHelper(
                new Guid("83454F57-97C3-4740-B2CD-A5AA4FA40EA2"),
                "StringValue",
                UIAutomationType.UIAutomationType_String,
                TypeMember<ITestProvider>.GetPropertyGetter(p => p.StringValue));

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
}