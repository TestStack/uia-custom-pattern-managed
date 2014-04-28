using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace WpfAppWithAdvTextControl
{
    public class TestControl : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TestControlAutomationPeer(this);
        }
    }
}
