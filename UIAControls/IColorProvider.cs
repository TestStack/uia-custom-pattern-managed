using System.Runtime.InteropServices;

namespace UIAControls
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("49F2F4CD-FFB7-4b21-9C4F-58090CDD8BCE")]
    public interface IColorProvider
    {
        int ValueAsColor { get; }
        void SetValueAsColor(int value);
    }
}