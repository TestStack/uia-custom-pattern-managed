using System.Runtime.InteropServices;
using ManagedUiaCustomizationCore;

namespace WpfAppWithAdvTextControl
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("0F268572-8746-4105-9188-080086FCC6E4")]
    [PatternGuid("B85FDDEA-D38F-44D6-AE42-0CA3CF0433F1")]
    public interface ICaretPositionProvider
    {
        [PatternProperty("6B55247F-6BAF-460C-9C3E-388E7161A7E9")]
        int SelectionStart { get; }

        [PatternProperty("F0CD6926-AA86-4EBF-BDCC-7345C5D98EC6")]
        int SelectionLength { get; }

        [PatternMethod]
        void SetSelectionStart(int value);

        [PatternMethod]
        void SetSelectionLength(int value);
    }
}