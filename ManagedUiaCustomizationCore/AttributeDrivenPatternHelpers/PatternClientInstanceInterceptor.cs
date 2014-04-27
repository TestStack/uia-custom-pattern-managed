using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Interop.UIAutomationCore;

namespace ManagedUiaCustomizationCore
{
    public class PatternClientInstanceInterceptor : IInterceptor
    {
        private readonly IUIAutomationPatternInstance _patternInstance;
        private readonly Dictionary<string, UiaPropertyInfoHelper> _currentPropGetterNameToHelper;
        private readonly Dictionary<string, UiaPropertyInfoHelper> _cachedPropGetterNameToHelper;
        private readonly Dictionary<string, UiaMethodInfoHelper> _methodNameToHelper;

        public PatternClientInstanceInterceptor(CustomPatternSchemaBase schema, IUIAutomationPatternInstance patternInstance)
        {
            _patternInstance = patternInstance;
            _currentPropGetterNameToHelper = schema.Properties.ToDictionary(helper => string.Format((string)"get_Current{0}", (object)helper.Data.pProgrammaticName));
            _cachedPropGetterNameToHelper = schema.Properties.ToDictionary(helper => string.Format("get_Cached{0}", helper.Data.pProgrammaticName));
            _methodNameToHelper = schema.Methods.ToDictionary(helper => helper.Data.pProgrammaticName);
        }

        public void Intercept(IInvocation invocation)
        {
            UiaPropertyInfoHelper propHelper;
            if (_currentPropGetterNameToHelper.TryGetValue(invocation.Method.Name, out propHelper))
                CallProperty(invocation, propHelper, cached: false);
            else if (_cachedPropGetterNameToHelper.TryGetValue(invocation.Method.Name, out propHelper))
                CallProperty(invocation, propHelper, cached: true);
            else
                throw new NotSupportedException(string.Format("Method {0} is not expected", invocation.Method.Name));
        }

        private void CallProperty(IInvocation invocation, UiaPropertyInfoHelper propHelper, bool cached)
        {
            // it is call for CurrentXxx property
            var param = new UiaParameterHelper(propHelper.UiaType);
            _patternInstance.GetProperty(propHelper.Index, cached ? 1 : 0, propHelper.UiaType, param.Data);
            invocation.ReturnValue = param.Value;
        }
    }
}
