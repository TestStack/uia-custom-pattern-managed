using System.Windows.Automation;
using ManagedUiaCustomizationCore;

namespace WpfAppWithAdvTextControl
{
    public class CaretPositionPattern : CustomPatternBase<ICaretPositionProvider, ICaretPositionPattern>
    {
        private CaretPositionPattern() 
            : base(usedInWpf: true)
        {
        }

        public static void Initialize()
        {
            if (PatternSchema != null) return;
            PatternSchema = new CaretPositionPattern();
        }

        public static CaretPositionPattern PatternSchema;

        // these will be set via reflection on Initialize() call
        public static AutomationPattern Pattern;
        public static AutomationProperty SelectionStartProperty;
        public static AutomationProperty SelectionLengthProperty;
    }
}