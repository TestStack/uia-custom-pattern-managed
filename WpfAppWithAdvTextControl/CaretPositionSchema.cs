using System;
using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;

namespace WpfAppWithAdvTextControl
{
    public class CaretPositionSchema : CustomPatternSchemaBase
    {
        private static CaretPositionSchema _instance;

        public static CaretPositionSchema Instance
        {
            get { return _instance ?? (_instance = new CaretPositionSchema()); }
        }

        private CaretPositionSchema()
        {
        }

        public readonly UiaPropertyInfoHelper SelectionStartProperty
            = new UiaPropertyInfoHelper(new Guid("6B55247F-6BAF-460C-9C3E-388E7161A7E9"),
                "SelectionStart",
                UIAutomationType.UIAutomationType_Int,
                TypeMember<ICaretPositionProvider>.GetPropertyGetter(p => p.SelectionStart));

        public readonly UiaPropertyInfoHelper SelectionLengthProperty
            = new UiaPropertyInfoHelper(new Guid("F0CD6926-AA86-4EBF-BDCC-7345C5D98EC6"),
                "SelectionLength",
                UIAutomationType.UIAutomationType_Int,
                TypeMember<ICaretPositionProvider>.GetPropertyGetter(p => p.SelectionLength));

        public readonly UiaMethodInfoHelper SetSelectionStartMethod
            = new UiaMethodInfoHelper("SetSelectionStart",
                true /* doSetFocus */,
                new[] {new UiaParameterDescription("value", UIAutomationType.UIAutomationType_Int)});

        public readonly UiaMethodInfoHelper SetSelectionLengthMethod
            = new UiaMethodInfoHelper("SetSelectionLength",
                true /* doSetFocus */,
                new[] {new UiaParameterDescription("value", UIAutomationType.UIAutomationType_Int)});

        public override UiaPropertyInfoHelper[] Properties
        {
            get { return new[] {SelectionStartProperty, SelectionLengthProperty}; }
        }

        public override UiaMethodInfoHelper[] Methods
        {
            get { return new[] {SetSelectionStartMethod, SetSelectionLengthMethod}; }
        }

        public override UiaEventInfoHelper[] Events
        {
            get { return new UiaEventInfoHelper[0]; }
        }

        public override Guid PatternGuid
        {
            get { return new Guid("B85FDDEA-D38F-44D6-AE42-0CA3CF0433F1"); }
        }

        public override Guid PatternClientGuid
        {
            get { return typeof(ICaretPositionPattern).GUID; }
        }

        public override Guid PatternProviderGuid
        {
            get { return typeof(ICaretPositionProvider).GUID; }
        }

        public override string PatternName
        {
            get { return "CaretPositionPattern"; }
        }

        public override IUIAutomationPatternHandler Handler
        {
            get { return new CaretPositionProviderHandler(); }
        }
    }
}
