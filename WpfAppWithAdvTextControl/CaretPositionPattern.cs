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
        public static int Pattern;
        public static int SelectionStartProperty;
        public static int SelectionLengthProperty;
    }
}