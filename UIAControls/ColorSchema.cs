using System;
using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;

// Color Pattern
// Schema and implementation for the custom pattern for setting/retrieving a controls
// value by RGB color.
namespace UIAControls
{
    // Declaration of the provider-side interface, which the control will implement.

    // Declaration of the client-side interface, for the client/test to use.

    /// <summary>
    /// Declaration of the pattern schema, with all of the information UIA needs
    /// about this property.
    /// </summary>
    public class ColorSchema : CustomPatternSchemaBase
    {
        private static readonly ColorSchema Instance = new ColorSchema();

        public static ColorSchema GetInstance()
        {
            return Instance;
        }

        public readonly UiaPropertyInfoHelper ValueAsColorProperty =
            new UiaPropertyInfoHelper(
                new Guid("48F45D48-37A1-4480-B5A7-198315D2F2A0"),
                "ValueAsColor",
                UIAutomationType.UIAutomationType_Int,
                provider => ((IColorProvider)provider).ValueAsColor);

        public readonly UiaMethodInfoHelper SetValueAsColorMethod =
            new UiaMethodInfoHelper(
                "SetValueAsColor",
                true /* doSetFocus */,
                new[] {new UiaParameterDescription("value", UIAutomationType.UIAutomationType_Int)});

        public override UiaPropertyInfoHelper[] Properties
        {
            get { return new[] {ValueAsColorProperty}; }
        }

        public override UiaMethodInfoHelper[] Methods
        {
            get { return new[] {SetValueAsColorMethod}; }
        }

        public override UiaEventInfoHelper[] Events
        {
            get { return new UiaEventInfoHelper[] {}; }
        }

        public override Guid PatternGuid
        {
            get { return new Guid("CDF2D932-6043-47ef-AB48-1CA756678B0C"); }
        }

        public override string PatternName
        {
            get { return "ColorPattern"; }
        }

        public override Type PatternProviderInterface
        {
            get { return typeof(IColorProvider); }
        }

        public override Type PatternClientInterface
        {
            get { return typeof(IColorPattern); }
        }

        public override IUIAutomationPatternHandler Handler
        {
            get { return new ColorProviderHandler(); }
        }
    }

    // Pattern instance class: wrap up a IUIAutomationPatternInstance and implement the
    // custom pattern interface on top of it.    
}