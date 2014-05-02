using System.Runtime.InteropServices;
using System.Windows.Automation;
using ManagedUiaCustomizationCore;
using UIAutomationClient;
using IRawElementProviderSimple = System.Windows.Automation.Provider.IRawElementProviderSimple;

namespace WpfAppWithAdvTextControl
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    [Guid("DB7AAA20-B3F3-4A71-A0D1-F08EA667FC05")]
    [PatternGuid("7C776C94-2F54-4054-A8EB-B23E713C3F5E")]
    public interface IAutomationElementRetievingProvider
    {
        [PatternProperty("2C87DC26-B842-4E0D-ADD8-2DEFB8F7F360")]
        IRawElementProviderSimple Element1 { get; }

        [PatternProperty("B5C46FFC-C638-4520-9CCF-6D73FA9A6DF7")]
        IRawElementProviderSimple Element2 { get; }

        [PatternMethod]
        IRawElementProviderSimple GetCurrentElement();

        [PatternMethod]
        void GetCurrentElementWithOutParam(out IRawElementProviderSimple value);
    }

    public interface IAutomationElementRetievingPattern
    {
        IUIAutomationElement CurrentElement1 { get; }
        IUIAutomationElement CachedElement1 { get; }

        AutomationElement CurrentElement2 { get; }
        AutomationElement CachedElement2 { get; }

        IUIAutomationElement GetCurrentElement();
        void GetCurrentElementWithOutParam(out IUIAutomationElement value);
    }

    public class AutomationElementRetievingPattern : CustomPatternBase<IAutomationElementRetievingProvider, IAutomationElementRetievingPattern>
    {
        private AutomationElementRetievingPattern() 
            : base(usedInWpf: true)
        {
        }

        public static void Initialize()
        {
            if (PatternSchema != null) return;
            PatternSchema = new AutomationElementRetievingPattern();
        }

        public static AutomationElementRetievingPattern PatternSchema;
        public static AutomationPattern Pattern;
        public static AutomationProperty Element1Property;
        public static AutomationProperty Element2Property;
    }
}