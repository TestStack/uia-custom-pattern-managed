using System;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;

namespace ManagedUiaCustomizationCore
{
    // P/Invoke declarations
    public class NativeMethods
    {
        [DllImport("UIAutomationCore.dll", EntryPoint = "UiaHostProviderFromHwnd", CharSet = CharSet.Unicode)]
        public static extern int UiaHostProviderFromHwnd(IntPtr hwnd, [MarshalAs(UnmanagedType.Interface)] out IRawElementProviderSimple provider);

        [DllImport("UIAutomationCore.dll", EntryPoint = "UiaReturnRawElementProvider", CharSet = CharSet.Unicode)]
        public static extern IntPtr UiaReturnRawElementProvider(IntPtr hwnd, IntPtr wParam, IntPtr lParam, IRawElementProviderSimple el);

        [DllImport("UIAutomationCore.dll", EntryPoint = "UiaRaiseAutomationEvent", CharSet = CharSet.Unicode)]
        public static extern int UiaRaiseAutomationEvent(IRawElementProviderSimple el, int eventId);

        [DllImport("UIAutomationCore.dll", EntryPoint = "UiaRaiseAutomationPropertyChangedEvent", CharSet = CharSet.Unicode)]
        public static extern int UiaRaiseAutomationPropertyChangedEvent(IRawElementProviderSimple el, int propertyId, object oldValue, object newValue);
    }
}