using System;
using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;

namespace UIAControls
{
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
            var paramList = new UiaParameterListHelper(pParams);

            var member = TestSchema.GetInstance().GetMemberByIndex(index);
            if (member != null)
            {
                member.DispatchCallToProvider(provider, paramList);
                return;
            }

            // Dispatch the method/property calls
            if (index == TestSchema.GetInstance().GetBoolValueMethod.Index)
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
}