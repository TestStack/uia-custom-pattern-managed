using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;

namespace UIAControls
{
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
}