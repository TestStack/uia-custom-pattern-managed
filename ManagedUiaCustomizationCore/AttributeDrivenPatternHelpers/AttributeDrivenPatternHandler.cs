using System;
using Interop.UIAutomationCore;

namespace ManagedUiaCustomizationCore
{
    public class AttributeDrivenPatternHandler : IUIAutomationPatternHandler
    {
        private readonly CustomPatternSchemaBase _schema;

        public AttributeDrivenPatternHandler(CustomPatternSchemaBase schema)
        {
            _schema = schema;
        }

        public void CreateClientWrapper(IUIAutomationPatternInstance pPatternInstance, out object pClientWrapper)
        {
            // TODO: Create general-purpose intercepting proxy
            throw new NotImplementedException();
        }

        public void Dispatch(object pTarget, uint index, UIAutomationParameter[] pParams, uint cParams)
        {
            ISchemaMember dispatchingMember = _schema.GetMemberByIndex(index);
            if (dispatchingMember == null)
                throw new NotSupportedException("Dispatching of this method is not supported");

            dispatchingMember.DispatchCallToProvider(pTarget, new UiaParameterListHelper(pParams));
        }
    }
}