using Interop.UIAutomationCore;

namespace UIAControls
{
    /// <summary>
    /// A sample implementation of ITestProvider.  
    /// This could be on the element provider itself, but it can also be separate,
    /// and keeping it separate keeps the main code cleaner,
    /// since this is pretty much just for testing.
    /// </summary>
    public class TestPatternProvider : ITestProvider
    {
        /// <summary>
        /// The host element of this pattern, from which the pattern was taken.
        /// </summary>
        private readonly IRawElementProviderSimple _hostElement;

        public TestPatternProvider(IRawElementProviderSimple hostElement)
        {
            _hostElement = hostElement;
        }

        public int IntValue
        {
            get { return 42; }
        }

        public string StringValue
        {
            get { return "TestString"; }
        }

        public bool BoolValue
        {
            get { return true; }
        }

        public double DoubleValue
        {
            get { return 3.1415; }
        }

        public IRawElementProviderSimple ElementValue
        {
            get { return _hostElement; }
        }

        public void PassIntParam(int value, out int retVal)
        {
            retVal = value;
        }

        public void PassStringParam(string value, out string retVal)
        {
            retVal = value;
        }

        public void PassBoolParam(bool value, out bool retVal)
        {
            retVal = value;
        }

        public void PassDoubleParam(double value, out double retVal)
        {
            retVal = value;
        }
    }
}