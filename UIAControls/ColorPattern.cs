using System;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;

// Color Pattern
// Schema and implementation for the custom pattern for setting/retrieving a controls
// value by RGB color.

namespace UIAControls
{
    // Declaration of the provider-side interface, which the control will implement.
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("49F2F4CD-FFB7-4b21-9C4F-58090CDD8BCE")]
    public interface IColorProvider
    {
        int ValueAsColor { get; }
        void SetValueAsColor(int value);
    }

    // Declaration of the client-side interface, for the client/test to use.
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("B98D615C-C7A2-4afd-AEC9-62FF4501AA30")]
    public interface IColorPattern
    {
        int CurrentValueAsColor { get; }
        int CachedValueAsColor { get; }
        void SetValueAsColor(int value);
    }

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
                UIAutomationType.UIAutomationType_Int);

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

        public override Guid PatternClientGuid
        {
            get { return typeof (IColorPattern).GUID; }
        }

        public override Guid PatternProviderGuid
        {
            get { return typeof (IColorProvider).GUID; }
        }

        public override string PatternName
        {
            get { return "ColorPattern"; }
        }

        public override IUIAutomationPatternHandler Handler
        {
            get { return new ColorProviderHandler(); }
        }
    }

    // Pattern instance class: wrap up a IUIAutomationPatternInstance and implement the
    // custom pattern interface on top of it.    
    public class ColorProviderClientInstance : CustomClientInstanceBase, IColorPattern
    {
        public ColorProviderClientInstance(IUIAutomationPatternInstance patternInstance) 
            : base(patternInstance)
        {
        }

        public int CurrentValueAsColor
        {
            get { return (int) GetCurrentPropertyValue(ColorSchema.GetInstance().ValueAsColorProperty); }
        }

        public int CachedValueAsColor
        {
            get { return (int) GetCachedPropertyValue(ColorSchema.GetInstance().ValueAsColorProperty); }
        }

        public void SetValueAsColor(int value)
        {
            CallMethod(ColorSchema.GetInstance().SetValueAsColorMethod, value);
        }
    }

    /// <summary>
    /// Pattern handler class: creates pattern instances on client side and dispatches
    /// calls on the provider side.
    /// </summary>
    public class ColorProviderHandler : IUIAutomationPatternHandler
    {
        public void CreateClientWrapper(IUIAutomationPatternInstance pPatternInstance, out object pClientWrapper)
        {
            pClientWrapper = new ColorProviderClientInstance(pPatternInstance);
        }

        public void Dispatch(object pTarget, uint index, UIAutomationParameter[] pParams, uint cParams)
        {
            // Parse the provider and parameter list
            var provider = (IColorProvider) pTarget;
            var paramList = new UiaParameterListHelper(pParams, cParams);

            // Dispatch the method/property calls
            if (index == ColorSchema.GetInstance().ValueAsColorProperty.Index)
            {
                paramList[0] = provider.ValueAsColor;
            }
            else if (index == ColorSchema.GetInstance().SetValueAsColorMethod.Index)
            {
                provider.SetValueAsColor((int) paramList[0]);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}