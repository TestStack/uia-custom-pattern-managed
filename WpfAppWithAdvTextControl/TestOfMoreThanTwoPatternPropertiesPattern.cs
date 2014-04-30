using System.Runtime.InteropServices;
using System.Windows.Automation;
using ManagedUiaCustomizationCore;

namespace WpfAppWithAdvTextControl
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [Guid("D645A984-99C8-4918-8E16-9C16B67EE9AE")]
    [PatternGuid("37799443-A693-461D-A84F-F3D4ED00CEF3")]
    public interface ITestOfMoreThanTwoPatternPropertiesProvider
    {
        [PatternProperty("71DAA00F-5179-43C7-B5E2-6F3CF7147356")]
        int Property1 { get; }

        [PatternProperty("896296D6-EC78-4E2B-A1B0-F80B6DC633D7")]
        int Property2 { get; }

        [PatternProperty("A6DD9558-B635-4C41-AE2F-93D57F68F107")]
        int Property3 { get; }
    }


    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [Guid("20260BA6-6A89-4147-B00C-569915822332")]
    public interface ITestOfMoreThanTwoPatternPropertiesPattern
    {
        int CurrentProperty1 { get; }
        int CurrentProperty2 { get; }
        int CurrentProperty3 { get; }
        int CachedProperty1 { get; }
        int CachedProperty2 { get; }
        int CachedProperty3 { get; }
    }

    public class TestOfMoreThanTwoPatternPropertiesPattern : CustomPatternBase<ITestOfMoreThanTwoPatternPropertiesProvider, ITestOfMoreThanTwoPatternPropertiesPattern>
    {
        private TestOfMoreThanTwoPatternPropertiesPattern() 
            : base(usedInWpf: true)
        {
        }

        public static void Initialize()
        {
            if (PatternSchema != null) return;
            PatternSchema = new TestOfMoreThanTwoPatternPropertiesPattern();
        }

        public static TestOfMoreThanTwoPatternPropertiesPattern PatternSchema;
        public static int Pattern;
        public static int Property1Property;
        public static int Property2Property;
        public static int Property3Property;

        [StandaloneProperty("36683304-3B8A-4035-A88C-B7384C7F057F", typeof(int))]
        public static AutomationProperty Standalone1Property;
    }
}