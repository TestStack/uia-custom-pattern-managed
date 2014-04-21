using System;
using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;
using UIAutomationClient;

namespace UIAControls
{
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
}