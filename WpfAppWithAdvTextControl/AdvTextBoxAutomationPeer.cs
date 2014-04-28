using System.Windows.Automation.Peers;

namespace WpfAppWithAdvTextControl
{
    public class AdvTextBoxAutomationPeer : TextBoxAutomationPeer, ICaretPositionProvider
    {
        public AdvTextBoxAutomationPeer(AdvTextBox owner)
            : base(owner)
        {
            CaretPositionPattern.Initialize();
        }

        private new AdvTextBox Owner
        {
            get { return (AdvTextBox)base.Owner; }
        }

        protected override string GetClassNameCore()
        {
            return "AdvTextBox";
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if ((int)patternInterface == CaretPositionPattern.Pattern.Id)
                return this;
            return base.GetPattern(patternInterface);
        }

        public int SelectionStart
        {
            get { return Owner.SelectionStart; }
        }

        public int SelectionLength
        {
            get { return Owner.SelectionLength; }
        }

        public void SetSelectionStart(int value)
        {
            Owner.SelectionStart = value;
        }

        public void SetSelectionLength(int value)
        {
            Owner.SelectionLength = value;
        }
    }
}
