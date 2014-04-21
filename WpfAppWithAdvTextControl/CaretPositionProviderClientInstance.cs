using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;

namespace WpfAppWithAdvTextControl
{
    public class CaretPositionProviderClientInstance : CustomClientInstanceBase, ICaretPositionPattern
    {
        public CaretPositionProviderClientInstance(IUIAutomationPatternInstance patternInstance)
            : base(patternInstance)
        {
        }

        public int CurrentSelectionStart
        {
            get { return (int)GetCurrentPropertyValue(CaretPositionSchema.Instance.SelectionStartProperty); }
        }

        public int CurrentSelectionLength
        {
            get { return (int)GetCurrentPropertyValue(CaretPositionSchema.Instance.SelectionLengthProperty); }
        }

        public int CachedSelectionStart
        {
            get { return (int)GetCachedPropertyValue(CaretPositionSchema.Instance.SelectionStartProperty); }
        }

        public int CachedSelectionLength
        {
            get { return (int)GetCachedPropertyValue(CaretPositionSchema.Instance.SelectionLengthProperty); }
        }

        public void SetSelectionStart(int value)
        {
            CallMethod(CaretPositionSchema.Instance.SetSelectionStartMethod, value);
        }

        public void SetSelectionLength(int value)
        {
            CallMethod(CaretPositionSchema.Instance.SetSelectionLengthMethod, value);
        }
    }
}
