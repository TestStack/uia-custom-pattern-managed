using System.Runtime.InteropServices;
using Interop.UIAutomationCore;

namespace UIAControls
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("E7C4D124-E430-46B8-B9CC-1DED8BBDA0F2")]
    public interface ITestProvider
    {
        int IntValue { get; }
        string StringValue { get; }
        bool BoolValue { get; }
        double DoubleValue { get; }
        IRawElementProviderSimple ElementValue { get; }

        void PassIntParam(int value, out int retVal);
        void PassStringParam(string value, out string retVal);
        void PassBoolParam(bool value, out bool retVal);
        void PassDoubleParam(double value, out double retVal);
    }
}