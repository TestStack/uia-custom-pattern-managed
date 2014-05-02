using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ManagedUiaCustomizationCore;

namespace WpfAppWithAdvTextControl
{
    public class TestControlAutomationPeer : FrameworkElementAutomationPeer,
                                             ITestOfMoreThanTwoPatternPropertiesProvider,
                                             IStandalonePropertyProvider,
                                             IAutomationElementRetievingProvider
    {
        public TestControlAutomationPeer(TestControl testControl)
            : base(testControl)
        {
            TestOfMoreThanTwoPatternPropertiesPattern.Initialize();
            AutomationElementRetievingPattern.Initialize();
        }

        protected override string GetClassNameCore()
        {
            return "TestControl";
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            var patternId = (int)patternInterface;
            if (patternId == TestOfMoreThanTwoPatternPropertiesPattern.Pattern ||
                patternId == AutomationElementRetievingPattern.Pattern.Id)
                return this;
            return base.GetPattern(patternInterface);
        }

        public int Property1
        {
            get { return 421; }
        }

        public int Property2
        {
            get { return 422; }
        }

        public int Property3
        {
            get { return 423; }
        }

        public object GetPropertyValue(AutomationProperty property)
        {
            if (TestOfMoreThanTwoPatternPropertiesPattern.Standalone1Property.Equals(property))
                return 42;
            return null;
        }

        public IRawElementProviderSimple Element1
        {
            get { return ProviderFromPeer(this); }
        }

        public IRawElementProviderSimple Element2
        {
            get { return ProviderFromPeer(this); }
        }

        public IRawElementProviderSimple GetCurrentElement()
        {
            return ProviderFromPeer(this);
        }

        public void GetCurrentElementWithOutParam(out IRawElementProviderSimple value)
        {
            value = ProviderFromPeer(this);
        }
    }
}
