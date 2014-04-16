using System.Runtime.InteropServices;

namespace WpfAppWithAdvTextControl
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("0FC33FD3-3874-4A32-A530-0EBE937D4419")]
    public interface ICaretPositionPattern
    {
        int CurrentSelectionStart { get; }
        int CurrentSelectionLength { get; }

        int CachedSelectionStart { get; }
        int CachedSelectionLength { get; }

        void SetSelectionStart(int value);
        void SetSelectionLength(int value);
    }
}