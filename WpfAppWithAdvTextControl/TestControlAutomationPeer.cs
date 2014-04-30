using System.Windows.Automation;
using System.Windows.Automation.Peers;
using ManagedUiaCustomizationCore;

namespace WpfAppWithAdvTextControl
{
    public class TestControlAutomationPeer : FrameworkElementAutomationPeer, ITestOfMoreThanTwoPatternPropertiesProvider, IStandalonePropertyProvider
    {
        public TestControlAutomationPeer(TestControl testControl)
            : base(testControl)
        {
            TestOfMoreThanTwoPatternPropertiesPattern.Initialize();
        }

        protected override string GetClassNameCore()
        {
            return "TestControl";
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if ((int)patternInterface == TestOfMoreThanTwoPatternPropertiesPattern.Pattern)
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
    }
}