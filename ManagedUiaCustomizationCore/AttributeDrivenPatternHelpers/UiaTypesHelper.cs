using System;
using System.Collections.Generic;
using Interop.UIAutomationCore;
using UIAutomationClient;

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
                  {typeof (IUIAutomationElement), UIAutomationType.UIAutomationType_Element},
              };

        private static readonly Dictionary<Type, UIAutomationType> _outTypeMapping
            = new Dictionary<Type, UIAutomationType>
              {
                  {typeof (int), UIAutomationType.UIAutomationType_OutInt},
                  {typeof (bool), UIAutomationType.UIAutomationType_OutBool},
                  {typeof (string), UIAutomationType.UIAutomationType_OutString},
                  {typeof (double), UIAutomationType.UIAutomationType_OutDouble},
                  {typeof (IUIAutomationElement), UIAutomationType.UIAutomationType_OutElement},
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