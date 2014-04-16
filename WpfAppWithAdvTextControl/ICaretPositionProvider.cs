using System.Runtime.InteropServices;

namespace WpfAppWithAdvTextControl
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("0F268572-8746-4105-9188-080086FCC6E4")]
    public interface ICaretPositionProvider
    {
        int SelectionStart { get; }
        int SelectionLength { get; }
        void SetSelectionStart(int value);
        void SetSelectionLength(int value);
    }
}