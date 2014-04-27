using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Threading;

namespace ManagedUiaCustomizationCore
{
    public abstract class CustomPatternBase<TProviderInterface, TPatternClientInterface> : AttributeDrivenPatternSchema
    {
        protected CustomPatternBase() 
            : base(typeof(TProviderInterface), typeof(TPatternClientInterface))
        {
            // try to auto-detect WPF
            bool areWeOnWpf = SynchronizationContext.Current is DispatcherSynchronizationContext;
            Register(makeAugmentationForWpfPeers: areWeOnWpf);
            FillRegistrationInfo();
        }

        private void FillRegistrationInfo()
        {
            var t = GetType();
            if (t.Name != PatternName)
                throw new ArgumentException(string.Format("Type is named incorrectly. Should be {0}", PatternName));
            var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            var pri = fields.FirstOrDefault(f => f.Name == "Pattern");
            if (pri == null)
                throw new ArgumentException("Field Pattern not found on the type");
            
            if (pri.FieldType == typeof(int))
                pri.SetValue(null, PatternId);
            else if (pri.FieldType == typeof(AutomationPattern))
                pri.SetValue(null, AutomationPattern.LookupById(PatternId));
            else
                throw new ArgumentException("Field Pattern should be either of type int of AutomationPattern");
            
            foreach (var prop in Properties)
            {
                var propFieldName = prop.Data.pProgrammaticName + "Property";
                var field = fields.FirstOrDefault(f => f.Name == propFieldName);
                if (field == null)
                    throw new ArgumentException(string.Format("Field {0} not found on the type", propFieldName));
                if (field.FieldType == typeof(int))
                    field.SetValue(null, prop.PropertyId);
                else if (field.FieldType == typeof(AutomationProperty))
                    field.SetValue(null, AutomationProperty.LookupById(prop.PropertyId));
                else
                    throw new ArgumentException("Fields for properties should be either of type int of AutomationProperty");
            }
        }
    }
}