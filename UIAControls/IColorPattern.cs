using System.Runtime.InteropServices;

namespace UIAControls
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("B98D615C-C7A2-4afd-AEC9-62FF4501AA30")]
    public interface IColorPattern
    {
        int CurrentValueAsColor { get; }
        int CachedValueAsColor { get; }
        void SetValueAsColor(int value);
    }
}