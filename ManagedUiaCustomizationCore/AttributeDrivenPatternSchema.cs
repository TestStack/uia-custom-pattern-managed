using System;
using System.Linq;
using System.Reflection;
using Interop.UIAutomationCore;

namespace ManagedUiaCustomizationCore
{
    public class AttributeDrivenPatternSchema : CustomPatternSchemaBase
    {
        private readonly Type _patternProviderInterface;
        private readonly Type _patternClientInterface;
        private readonly Guid _patternGuid;
        private readonly string _patternName;
        private readonly UiaMethodInfoHelper[] _methods;
        private readonly UiaPropertyInfoHelper[] _properties;
        private readonly AttributeDrivenPatternHandler _handler;

        public AttributeDrivenPatternSchema(Type patternProviderInterface, Type patternClientInterface)
        {
            if (!patternProviderInterface.IsInterface)
                throw new ArgumentException("Provided pattern provider type should be an interface", "patternProviderInterface");
            if (!patternClientInterface.IsInterface)
                throw new ArgumentException("Provided pattern client type should be an interface", "patternClientInterface");
            _patternProviderInterface = patternProviderInterface;
            _patternClientInterface = patternClientInterface;

            var patternClientName = _patternClientInterface.Name;
            if (!patternClientName.EndsWith("Pattern") || !patternClientName.StartsWith("I"))
                throw new ArgumentException("Pattern client interface named incorrectly, should be IXxxPattern", "patternClientInterface");
            var baseName = patternClientName.Substring(1, patternClientName.Length - "I".Length - "Pattern".Length);
            if (_patternProviderInterface.Name != string.Format("I{0}Provider", baseName))
                throw new ArgumentException(string.Format("Pattern provider interface named incorrectly, should be I{0}Provider", baseName));
            _patternName = string.Format("{0}Pattern", baseName);

            var patternGuidAttr = _patternProviderInterface.GetAttribute<PatternGuidAttribute>();
            if (patternGuidAttr == null) throw new ArgumentException("Provided type should be marked with PatternGuid attribute");
            _patternGuid = patternGuidAttr.Value;

            _methods = patternProviderInterface.GetMethodsMarkedWith<PatternMethodAttribute>().Select(GetMethodHelper).ToArray();
            _properties = patternProviderInterface.GetPropertiesMarkedWith<PatternPropertyAttribute>().Select(GetPropertyHelper).ToArray();
            _handler = new AttributeDrivenPatternHandler(this);
        }

        private UiaPropertyInfoHelper GetPropertyHelper(PropertyInfo pInfo)
        {
            var propertyAttr = pInfo.GetAttribute<PatternPropertyAttribute>(); // can'be null as otherwise it wouldn't get into this method
            var guid = propertyAttr.Guid;
            var programmaticName = pInfo.Name;
            var uiaType = UiaTypesHelper.TypeToAutomationType(pInfo.PropertyType);
            return new UiaPropertyInfoHelper(guid, programmaticName, uiaType, pInfo.GetPropertyGetter());
        }

        private UiaMethodInfoHelper GetMethodHelper(MethodInfo mInfo)
        {
            var methodAttr = mInfo.GetAttribute<PatternMethodAttribute>(); // can'be null as otherwise it wouldn't get into this method
            var doSetFocus = methodAttr.DoSetFocus;
            return new UiaMethodInfoHelper(mInfo, doSetFocus);
        }

        public override string PatternName
        {
            get { return _patternName; }
        }

        public override Type PatternProviderInterface
        {
            get { return _patternProviderInterface; }
        }

        public override Type PatternClientInterface
        {
            get { return _patternClientInterface; }
        }

        public override Guid PatternGuid
        {
            get { return _patternGuid; }
        }

        public override UiaPropertyInfoHelper[] Properties
        {
            get { return _properties; }
        }

        public override UiaMethodInfoHelper[] Methods
        {
            get { return _methods; }
        }

        public override UiaEventInfoHelper[] Events
        {
            // not supported for now
            get { return new UiaEventInfoHelper[0]; }
        }

        public override IUIAutomationPatternHandler Handler
        {
            get { return _handler; }
        }
    }
}
