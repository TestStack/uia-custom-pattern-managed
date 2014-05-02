using System;
using System.Collections.Generic;
using Interop.UIAutomationCore;

namespace ManagedUiaCustomizationCore
{
    public static class UiaTypesHelper
    {
        public const string RetParamUnspeakableName = "<>retValue";

        private static readonly Dictionary<Type, UIAutomationType> _typeMapping
            = new Dictionary<Type, UIAutomationType>
              {
                  {typeof (int), UIAutomationType.UIAutomationType_Int},
                  {typeof (bool), UIAutomationType.UIAutomationType_Bool},
                  {typeof (string), UIAutomationType.UIAutomationType_String},
                  {typeof (double), UIAutomationType.UIAutomationType_Double},

                  // We want to support at least UIAComWrapper which uses Interop.AutomationClient,
                  // this code which uses Interop.UIAutomationCore and WPF's UIAutomationProvider.
                  // Actually it'd be better to ask type through IUnknown if it implements interface
                  // with required IID, but for now such simplified mapping would be enough.
                  {typeof (IRawElementProviderSimple), UIAutomationType.UIAutomationType_Element},
                  {typeof (UIAutomationClient.IRawElementProviderSimple), UIAutomationType.UIAutomationType_Element},
                  {typeof (System.Windows.Automation.Provider.IRawElementProviderSimple), UIAutomationType.UIAutomationType_Element},
              };

        private static readonly Dictionary<Type, UIAutomationType> _outTypeMapping
            = new Dictionary<Type, UIAutomationType>
              {
                  {typeof (int), UIAutomationType.UIAutomationType_OutInt},
                  {typeof (bool), UIAutomationType.UIAutomationType_OutBool},
                  {typeof (string), UIAutomationType.UIAutomationType_OutString},
                  {typeof (double), UIAutomationType.UIAutomationType_OutDouble},
                  {typeof (IRawElementProviderSimple), UIAutomationType.UIAutomationType_OutElement},
                  {typeof (UIAutomationClient.IRawElementProviderSimple), UIAutomationType.UIAutomationType_OutElement},
                  {typeof (System.Windows.Automation.Provider.IRawElementProviderSimple), UIAutomationType.UIAutomationType_OutElement},
              };

        public static UIAutomationType TypeToAutomationType(Type propertyType)
        {
            UIAutomationType res;
            if (_typeMapping.TryGetValue(propertyType, out res))
                return res;
            throw new NotSupportedException("Provided type is not supported");
        }

        public static UIAutomationType TypeToOutAutomationType(Type propertyType)
        {
            UIAutomationType res;
            if (_outTypeMapping.TryGetValue(propertyType, out res))
                return res;
            throw new NotSupportedException("Provided type is not supported");
        }

        public static bool IsInType(UIAutomationType type)
        {
            return !IsOutType(type);
        }

        public static bool IsOutType(UIAutomationType type)
        {
            return (type & UIAutomationType.UIAutomationType_Out) != 0;
        }
    }
}