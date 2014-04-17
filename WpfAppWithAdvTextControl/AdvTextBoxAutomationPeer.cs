using System;
using System.Windows.Automation.Peers;
using System.Windows.Threading;

namespace WpfAppWithAdvTextControl
{
    public class AdvTextBoxAutomationPeer : TextBoxAutomationPeer, ICaretPositionProvider
    {
        public AdvTextBoxAutomationPeer(AdvTextBox owner)
            : base(owner)
        {
            CaretPositionSchema.Instance.Register(makeAugmentationForWpfPeers: true);
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
            if ((int)patternInterface == CaretPositionSchema.Instance.PatternId)
                return this;
            return base.GetPattern(patternInterface);
        }

        public int SelectionStart
        {
            get { return (int)Dispatcher.Invoke(DispatcherPriority.Send, (Func<int>)(() => Owner.SelectionStart)); }
        }

        public int SelectionLength
        {
            get { return (int)Dispatcher.Invoke(DispatcherPriority.Send, (Func<int>)(() => Owner.SelectionLength)); }
        }

        public void SetSelectionStart(int value)
        {
            Dispatcher.Invoke(DispatcherPriority.Send, (Action)(() => Owner.SelectionStart = value));
        }

        public void SetSelectionLength(int value)
        {
            Dispatcher.Invoke(DispatcherPriority.Send, (Action)(() => Owner.SelectionLength = value));
        }
    }
}
