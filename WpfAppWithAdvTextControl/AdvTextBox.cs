using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace WpfAppWithAdvTextControl
{
    public class AdvTextBox : TextBox
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AdvTextBoxAutomationPeer(this);
        }
    }
}