using System;
using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;

namespace UIAControls
{
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