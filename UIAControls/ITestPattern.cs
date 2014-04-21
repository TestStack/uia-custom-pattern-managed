using System.Runtime.InteropServices;
using UIAutomationClient;

namespace UIAControls
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("82A9C7E7-9C87-497C-ABD2-054A6A7BACA2")]
    public interface ITestPattern
    {
        int CurrentIntValue { get; }
        string CurrentStringValue { get; }
        bool CurrentBoolValue { get; }
        double CurrentDoubleValue { get; }
        IUIAutomationElement CurrentElementValue { get; }

        int CachedIntValue { get; }
        string CachedStringValue { get; }
        bool CachedBoolValue { get; }
        double CachedDoubleValue { get; }
        IUIAutomationElement CachedElementValue { get; }

        void PassIntParam(int value, out int retVal);
        void PassStringParam(string value, out string retVal);
        void PassBoolParam(bool value, out bool retVal);
        void PassDoubleParam(double value, out double retVal);
    }
}