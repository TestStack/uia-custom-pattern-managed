using System;
using System.Linq;
using System.Reflection;
using System.Windows.Automation;

namespace ManagedUiaCustomizationCore
{
    public abstract class CustomPatternBase<TProviderInterface, TPatternClientInterface> : AttributeDrivenPatternSchema
    {
        private readonly bool _usedInWpf;

        protected CustomPatternBase(bool usedInWpf)
            : base(typeof(TProviderInterface), typeof(TPatternClientInterface))
        {
            _usedInWpf = usedInWpf;
            Register(makeAugmentationForWpfPeers: usedInWpf);
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
            {
                if (!_usedInWpf)
                    throw new ArgumentException("You can't use AutomationPattern registration info because you passed usedInWpf: false in constructor");
                pri.SetValue(null, AutomationPattern.LookupById(PatternId));
            }
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
                {
                    if (!_usedInWpf)
                        throw new ArgumentException("You can't use AutomationPattern registration info because you passed usedInWpf: false in constructor");
                    field.SetValue(null, AutomationProperty.LookupById(prop.PropertyId));
                }
                else
                    throw new ArgumentException("Fields for properties should be either of type int of AutomationProperty");
            }
        }
    }
}
