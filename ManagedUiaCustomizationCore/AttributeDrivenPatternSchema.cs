using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Interop.UIAutomationCore;
using UIAutomationClient;

namespace UIAControls
{
    public class AttributeDrivenPatternSchema : CustomPatternSchemaBase
    {
        private static readonly Dictionary<Type, UIAutomationType> _typeMapping
            = new Dictionary<Type, UIAutomationType>
              {
                  {typeof (int), UIAutomationType.UIAutomationType_Int},
                  {typeof (int[]), UIAutomationType.UIAutomationType_IntArray},
                  {typeof (bool), UIAutomationType.UIAutomationType_Bool},
                  {typeof (bool[]), UIAutomationType.UIAutomationType_BoolArray},
                  {typeof (string), UIAutomationType.UIAutomationType_String},
                  {typeof (string[]), UIAutomationType.UIAutomationType_StringArray},
                  {typeof (double), UIAutomationType.UIAutomationType_Double},
                  {typeof (double[]), UIAutomationType.UIAutomationType_DoubleArray},
                  {typeof (IUIAutomationElement), UIAutomationType.UIAutomationType_Element},
                  {typeof (IUIAutomationElement[]), UIAutomationType.UIAutomationType_ElementArray},
              };

        private static readonly Dictionary<Type, UIAutomationType> _outTypeMapping
            = new Dictionary<Type, UIAutomationType>
              {
                  {typeof (int), UIAutomationType.UIAutomationType_OutInt},
                  {typeof (int[]), UIAutomationType.UIAutomationType_OutIntArray},
                  {typeof (bool), UIAutomationType.UIAutomationType_OutBool},
                  {typeof (bool[]), UIAutomationType.UIAutomationType_OutBoolArray},
                  {typeof (string), UIAutomationType.UIAutomationType_OutString},
                  {typeof (string[]), UIAutomationType.UIAutomationType_OutStringArray},
                  {typeof (double), UIAutomationType.UIAutomationType_OutDouble},
                  {typeof (double[]), UIAutomationType.UIAutomationType_OutDoubleArray},
                  {typeof (IUIAutomationElement), UIAutomationType.UIAutomationType_OutElement},
                  {typeof (IUIAutomationElement[]), UIAutomationType.UIAutomationType_OutElementArray},
              };

        private readonly Guid _patternGuid;
        private readonly Guid _patternClientGuid;
        private readonly Guid _patternProviderGuid;
        private readonly string _patternName;
        private readonly UiaMethodInfoHelper[] _methods;
        private readonly UiaPropertyInfoHelper[] _properties;

        public AttributeDrivenPatternSchema(Type patternProviderInterface, Type patternClientInterface)
        {
            if (!patternProviderInterface.IsInterface)
                throw new ArgumentException("Provided pattern provider type should be an interface", "patternProviderInterface");
            if (!patternClientInterface.IsInterface)
                throw new ArgumentException("Provided pattern client type should be an interface", "patternClientInterface");

            var patternClientName = patternClientInterface.Name;
            if (!patternClientName.EndsWith("Pattern") || !patternClientName.StartsWith("I"))
                throw new ArgumentException("Pattern client interface named incorrectly, should be IXxxPattern", "patternClientInterface");
            var baseName = patternClientName.Substring(1, patternClientName.Length - "I".Length - "Pattern".Length);
            if (patternProviderInterface.Name != string.Format("I{0}Provider", baseName))
                throw new ArgumentException(string.Format("Pattern provider interface named incorrectly, should be I{0}Provider", baseName));
            _patternName = string.Format("{0}Pattern", baseName);

            var patternGuidAttr = patternProviderInterface.GetAttribute<PatternGuidAttribute>();
            if (patternGuidAttr == null) throw new ArgumentException("Provided type should be marked with PatternGuid attribute");
            _patternGuid = patternGuidAttr.Value;
            _patternClientGuid = patternClientInterface.GUID;
            _patternProviderGuid = patternProviderInterface.GUID;

            _methods = patternProviderInterface.GetMethodsMarkedWith<PatternMethodAttribute>().Select(GetMethodHelper).ToArray();
            _properties = patternProviderInterface.GetPropertiesMarkedWith<PatternPropertyAttribute>().Select(GetPropertyHelper).ToArray();
        }

        private UiaPropertyInfoHelper GetPropertyHelper(PropertyInfo pInfo)
        {
            var propertyAttr = pInfo.GetAttribute<PatternPropertyAttribute>(); // can'be null as otherwise it wouldn't get into this method
            var guid = propertyAttr.Guid;
            var programmaticName = pInfo.Name;
            var uiaType = TypeToAutomationType(pInfo.PropertyType);
            return new UiaPropertyInfoHelper(guid, programmaticName, uiaType);
        }

        private UiaMethodInfoHelper GetMethodHelper(MethodInfo mInfo)
        {
            var methodAttr = mInfo.GetAttribute<PatternMethodAttribute>(); // can'be null as otherwise it wouldn't get into this method
            var programmaticName = mInfo.Name;
            var doSetFocus = methodAttr.DoSetFocus;
            var args = new List<UiaParameterDescription>();
            if (mInfo.ReturnType != typeof (void))
                args.Add(new UiaParameterDescription("_retValue", TypeToOutAutomationType(mInfo.ReturnType)));
            args.AddRange(from parameterInfo in mInfo.GetParameters()
                          let uiaType = parameterInfo.IsOut
                              ? TypeToOutAutomationType(parameterInfo.ParameterType.GetElementType())
                              : TypeToAutomationType(parameterInfo.ParameterType)
                          select new UiaParameterDescription(parameterInfo.Name, uiaType));
            return new UiaMethodInfoHelper(programmaticName, doSetFocus, args);
        }

        private UIAutomationType TypeToAutomationType(Type propertyType)
        {
            UIAutomationType res;
            if (_typeMapping.TryGetValue(propertyType, out res))
                return res;
            throw new NotSupportedException("Provided type is not supported");
        }

        private UIAutomationType TypeToOutAutomationType(Type propertyType)
        {
            UIAutomationType res;
            if (_outTypeMapping.TryGetValue(propertyType, out res))
                return res;
            throw new NotSupportedException("Provided type is not supported");
        }

        public override string PatternName
        {
            get { return _patternName; }
        }

        public override Guid PatternGuid
        {
            get { return _patternGuid; }
        }

        public override Guid PatternClientGuid
        {
            get { return _patternClientGuid; }
        }

        public override Guid PatternProviderGuid
        {
            get { return _patternProviderGuid; }
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
            get { throw new NotImplementedException(); }
        }
    }
}
