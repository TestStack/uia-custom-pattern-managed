using System;
using Interop.UIAutomationCore;
using UIAControls;

namespace WpfAppWithAdvTextControl
{
    public class CaretPositionProviderHandler : IUIAutomationPatternHandler
    {
        public void CreateClientWrapper(IUIAutomationPatternInstance pPatternInstance, out object pClientWrapper)
        {
            pClientWrapper = new CaretPositionProviderClientInstance(pPatternInstance);
        }

        public void Dispatch(object pTarget, uint index, UIAutomationParameter[] pParams, uint cParams)
        {
            var provider = (ICaretPositionProvider)pTarget;
            var paramList = new UiaParameterListHelper(pParams, cParams);

            if (index == CaretPositionSchema.Instance.SelectionStartProperty.Index)
            {
                paramList[0] = provider.SelectionStart;
            }
            else if (index == CaretPositionSchema.Instance.SelectionLengthProperty.Index)
            {
                paramList[0] = provider.SelectionLength;
            }
            else if (index == CaretPositionSchema.Instance.SetSelectionStartMethod.Index)
            {
                provider.SetSelectionStart((int)paramList[0]);
            }
            else if (index == CaretPositionSchema.Instance.SetSelectionLengthMethod.Index)
            {
                provider.SetSelectionLength((int)paramList[0]);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
