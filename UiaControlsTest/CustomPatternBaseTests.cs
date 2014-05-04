using System.Runtime.InteropServices;
using System.Windows.Automation;
using ManagedUiaCustomizationCore;
using NUnit.Framework;

namespace UiaControlsTest
{
    // here we'll test that registration info is placed correctly to properties and wpf augmentation is done
    [TestFixture]
    public class CustomPatternBaseTests
    {
        [Test]
        public void AfterInitializationRegistrationInfoFilledCorrectly()
        {
            CustomPatternBaseTestPattern.Initialize();

            Assert.IsNotNull(CustomPatternBaseTestPattern.Pattern);
            Assert.IsNotNull(CustomPatternBaseTestPattern.SomeIntProperty);
            Assert.IsNotNull(CustomPatternBaseTestPattern.SomeStringProperty);
            Assert.IsNotNull(CustomPatternBaseTestPattern.StandaloneIntProperty);
        }

        #region Pattern definition

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        [Guid("1800101C-5550-46F0-99FC-11C6868D4237")]
        [PatternGuid("8453E6B6-C1D7-4A0E-A4F9-85CFCE098C2F")]
        public interface ICustomPatternBaseTestProvider
        {
            [PatternProperty("2D86858D-737E-4BC1-A9BA-51978E86B6A4")]
            int SomeInt { get; }

            [PatternProperty("64FE6A20-CB27-402E-86A4-29876D0DF81A")]
            string SomeString { get; }
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        [Guid("09DB9892-BD12-4A55-B3CF-218154B8D77B")]
        public interface ICustomPatternBaseTestPattern
        {
            int CurrentSomeInt { get; }
            string CurrentSomeString { get; }
            int CachedSomeInt { get; }
            string CachedSomeString { get; }
        }

        public class CustomPatternBaseTestPattern : CustomPatternBase<ICustomPatternBaseTestProvider, ICustomPatternBaseTestPattern>
        {
            private CustomPatternBaseTestPattern() 
                : base(usedInWpf: true)
            {
            }

            public static void Initialize()
            {
                if (PatternSchema != null) return;
                PatternSchema = new CustomPatternBaseTestPattern();
            }

            public static CustomPatternBaseTestPattern PatternSchema;

            public static AutomationPattern Pattern;
            public static AutomationProperty SomeIntProperty;
            public static AutomationProperty SomeStringProperty;

            [StandaloneProperty("B26C3D40-215A-478B-9C4D-6D91DFDAC3FF", typeof(int))]
            public static AutomationProperty StandaloneIntProperty;
        }

        #endregion
    }
}