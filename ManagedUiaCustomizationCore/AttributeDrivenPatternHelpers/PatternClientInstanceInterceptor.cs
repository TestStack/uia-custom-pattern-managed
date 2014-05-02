using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Automation;
using Castle.DynamicProxy;
using Interop.UIAutomationCore;
using UIAutomationClient;

namespace ManagedUiaCustomizationCore
{
    public class PatternClientInstanceInterceptor : IInterceptor
    {
        private readonly IUIAutomationPatternInstance _patternInstance;
        private readonly Dictionary<string, UiaPropertyInfoHelper> _currentPropGetterNameToHelper;
        private readonly Dictionary<string, UiaPropertyInfoHelper> _cachedPropGetterNameToHelper;
        private readonly Dictionary<MethodInfo, UiaMethodInfoHelper> _methodInfoToHelper;

        public PatternClientInstanceInterceptor(CustomPatternSchemaBase schema, IUIAutomationPatternInstance patternInstance)
        {
            _patternInstance = patternInstance;
            _currentPropGetterNameToHelper = schema.Properties.ToDictionary(helper => string.Format("get_Current{0}", helper.Data.pProgrammaticName));
            _cachedPropGetterNameToHelper = schema.Properties.ToDictionary(helper => string.Format("get_Cached{0}", helper.Data.pProgrammaticName));

            // helpers are aware about provider methods, we have to map them from client-side pattern methods
            _methodInfoToHelper = new Dictionary<MethodInfo, UiaMethodInfoHelper>();
            foreach (var methodInfoHelper in schema.Methods)
            {
                var providerInfo = methodInfoHelper.ProviderMethodInfo;
                if (providerInfo == null) continue;

                var patternMethod = GetMatchingPatternMethod(schema.PatternClientInterface, providerInfo);
                if (patternMethod != null)
                    _methodInfoToHelper[patternMethod] = methodInfoHelper;
            }
        }

        private MethodInfo GetMatchingPatternMethod(Type patternInterface, MethodInfo providerMethodInfo)
        {
            return patternInterface.GetMethods()
                                   .Where(m => m.Name == providerMethodInfo.Name)
                                   .FirstOrDefault(mi => MethodsMatch(providerMethodInfo, mi));
        }

        private bool MethodsMatch(MethodInfo providerMethod, MethodInfo patternMethod)
        {
            var providerParamTypes = providerMethod.GetParameters().Select(param => param.ParameterType).ToArray();
            var patternParamTypes = patternMethod.GetParameters().Select(param => param.ParameterType).ToArray();
            
            if (patternParamTypes.Length != providerParamTypes.Length) 
                return false;
            return Enumerable.Range(0, patternParamTypes.Length)
                             .All(idx => ParametersMatch(providerParamTypes[idx], patternParamTypes[idx]));
        }

        private bool ParametersMatch(Type serverParamType, Type clientParamType)
        {
            if (serverParamType.IsByRef != clientParamType.IsByRef)
                return false;

            if (UiaTypesHelper.IsElementOnServerSide(serverParamType))
            {
                if (clientParamType.IsByRef)
                    clientParamType = clientParamType.GetElementType();
                return UiaTypesHelper.IsElementOnClientSide(clientParamType)
                       || clientParamType == typeof(AutomationElement);
            }

            return serverParamType == clientParamType;
        }

        public void Intercept(IInvocation invocation)
        {
            UiaPropertyInfoHelper propHelper;
            UiaMethodInfoHelper methodHelper;
            if (_currentPropGetterNameToHelper.TryGetValue(invocation.Method.Name, out propHelper))
                CallProperty(invocation, propHelper, cached: false);
            else if (_cachedPropGetterNameToHelper.TryGetValue(invocation.Method.Name, out propHelper))
                CallProperty(invocation, propHelper, cached: true);
            else if (_methodInfoToHelper.TryGetValue(invocation.Method, out methodHelper))
                CallMethod(invocation, methodHelper);
            else
                throw new NotSupportedException(string.Format("Method {0} is not expected", invocation.Method.Name));
        }

        private void CallProperty(IInvocation invocation, UiaPropertyInfoHelper propHelper, bool cached)
        {
            // it is call for CurrentXxx property
            var param = new UiaParameterHelper(propHelper.UiaType);
            NativeMethods.WrapUiaComCall(() => _patternInstance.GetProperty(propHelper.Index, cached ? 1 : 0, propHelper.UiaType, param.Data));
            object value = param.Value;
            if (invocation.Method.ReturnType == typeof(AutomationElement))
                value = AutomationElement.Wrap((IUIAutomationElement)value);
            invocation.ReturnValue = value;
        }

        private void CallMethod(IInvocation invocation, UiaMethodInfoHelper methodHelper)
        {
            if (!methodHelper.SupportsDispatch)
                throw new InvalidOperationException("Called method {0} doesn't support automatic metadata-driven dispatch. You have to modify schema in order to use this feature so that corresponding UiaMethodInfoHelper supports dispatch.");
            var paramList = new UiaParameterListHelper(methodHelper);
            
            // 1. Fill In params to paramList from invocation arguments
            // we're using the fact that In params are always going before out params, so we may just go through 0..(cInParameters-1)
            for (int i = 0; i < methodHelper.Data.cInParameters; i++)
            {
                var desc = methodHelper.PatternMethodParamDescriptions[i];
                var idx = methodHelper.GetProviderMethodArgumentIndex(desc.Name);
                paramList[i] = invocation.Arguments[idx];
            }

            // 2. Call patternInstance method
            NativeMethods.WrapUiaComCall(() => _patternInstance.CallMethod(methodHelper.Index, paramList.Data, paramList.Count));
            
            // 3. Fill Out params back to invocation from paramList
            for (int i = (int)methodHelper.Data.cInParameters; i < methodHelper.Data.cInParameters + methodHelper.Data.cOutParameters; i++)
            {
                object value = paramList[i];
                var desc = methodHelper.PatternMethodParamDescriptions[i];
                if (desc.Name == UiaTypesHelper.RetParamUnspeakableName)
                {
                    if (invocation.Method.ReturnType == typeof(AutomationElement))
                        value = AutomationElement.Wrap((IUIAutomationElement)value);
                    invocation.ReturnValue = value;
                }
                else
                {
                    var idx = methodHelper.GetProviderMethodArgumentIndex(desc.Name);
                    if (invocation.Method.GetParameters()[idx].ParameterType.GetElementType() == typeof(AutomationElement))
                        value = AutomationElement.Wrap((IUIAutomationElement)value);
                    invocation.Arguments[idx] = value;
                }
            }
        }
    }
}
