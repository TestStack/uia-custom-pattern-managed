using System;
using Interop.UIAutomationClient;
using NUnit.Framework;
using UIAControls;

namespace UiaControlsTest
{
    /// <summary>
    /// A set of unit tests for the UIA Custom Patterns demo
    /// </summary>
    [TestFixture]
    public class CustomPatternUnitTest
    {
        private TargetApp _app;
        private IUIAutomation _factory;
        private IUIAutomationElement _customElement;

        [SetUp]
        public void MyTestInitialize()
        {
            // Create the factory and register schemas
            _factory = new CUIAutomationClass();
            ReadyStateSchema.GetInstance().Register();
            TestSchema.GetInstance().Register();
            ColorSchema.GetInstance().Register();

            // Start the app
            var curDir = Environment.CurrentDirectory;
            _app = new TargetApp(curDir + "\\UiaControls.exe");
            _app.Start();

            // Find the main control
            var appElement = _factory.ElementFromHandle(_app.MainWindow);
            var condition = _factory.CreatePropertyCondition(
                UIA_PropertyIds.UIA_AutomationIdPropertyId,
                "triColorControl1");
            _customElement = appElement.FindFirst(TreeScope.TreeScope_Children,
                                                  condition);
            Assert.IsNotNull(_customElement);
        }

        [TearDown]
        public void MyTestCleanup()
        {
            _app.Dispose();
            _app = null;
        }

        [Test]
        public void TestReadyStateProperty()
        {
            // Query our custom property
            var readyStateValue = _customElement.GetCurrentPropertyValue(ReadyStateSchema.GetInstance().ReadyStateProperty.PropertyId);
            Assert.IsInstanceOf<string>(readyStateValue);

            // By default, UIAControls.exe launches in not-ready state
            Assert.AreEqual("Not Ready", readyStateValue);
        }

        /// <summary>
        /// Validate that the Color pattern is working for the main control
        /// </summary>
        [Test]
        public void TestColorPattern()
        {
            var schema = ColorSchema.GetInstance();

            // Ask for the custom pattern
            var colorPattern = (IColorPattern) _customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(colorPattern);

            // Call through a pattern getter
            var colorAsValue = colorPattern.CurrentValueAsColor;
            Assert.AreEqual(0xFFFF0000, (uint) colorAsValue);

            // Call a pattern setter
            colorPattern.SetValueAsColor(Convert.ToInt32(0x008000));

            // Call for a custom property
            var colorAsValue2 = (int) _customElement.GetCurrentPropertyValue(schema.ValueAsColorProperty.PropertyId);
            Assert.AreEqual(0xFF008000, (uint) colorAsValue2);
        }

        /// <summary>
        /// Test support for Int values using Test pattern
        /// </summary>
        [Test]
        public void TestIntSupport()
        {
            var schema = TestSchema.GetInstance();

            // Ask for the custom pattern
            var testPattern = (ITestPattern) _customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            var testPropertyValue = testPattern.CurrentIntValue;
            Assert.AreEqual(42, testPropertyValue);

            // Call for the custom property directly
            var testPropertyValue2 = (int) _customElement.GetCurrentPropertyValue(schema.IntValueProperty.PropertyId);
            Assert.AreEqual(42, testPropertyValue2);

            // Call a pattern passer
            int testPropertyValue3;
            testPattern.PassIntParam(82, out testPropertyValue3);
            Assert.AreEqual(82, testPropertyValue3);
        }

        // <summary>
        // Test support for String values using Test pattern
        // </summary>
        [Test]
        public void TestStringSupport()
        {
            var schema = TestSchema.GetInstance();

            // Ask for the custom pattern
            var testPattern = (ITestPattern) _customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            var testPropertyValue = testPattern.CurrentStringValue;
            Assert.AreEqual("TestString", testPropertyValue);

            // Call for the custom property directly
            var testPropertyValue2 = (string) _customElement.GetCurrentPropertyValue(schema.StringValueProperty.PropertyId);
            Assert.AreEqual("TestString", testPropertyValue2);

            // Call a pattern passer
            string testPropertyValue3;
            testPattern.PassStringParam("String2", out testPropertyValue3);
            Assert.AreEqual("String2", testPropertyValue3);
        }

        /// <summary>
        /// Test support for Bool values using Test pattern
        /// </summary>
        [Test]
        public void TestBoolSupport()
        {
            var schema = TestSchema.GetInstance();

            // Ask for the custom pattern
            var testPattern = (ITestPattern)
                _customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            var testPropertyValue = testPattern.CurrentBoolValue;
            Assert.AreEqual(true, testPropertyValue);

            // We cannot request the property directly,
            // since it is declared as a method, not a property.
            //bool testPropertyValue2 = (bool)this.customElement.GetCurrentPropertyValue(schema.BoolValueProperty.PropertyId);
            //Assert.AreEqual(true, testPropertyValue2);

            // Call a pattern passer
            bool testPropertyValue3;
            testPattern.PassBoolParam(true, out testPropertyValue3);
            Assert.AreEqual(true, testPropertyValue3);
        }

        /// <summary>
        /// Test support for Double values using Test pattern
        /// </summary>
        [Test]
        public void TestDoubleSupport()
        {
            var schema = TestSchema.GetInstance();

            // Ask for the custom pattern
            var testPattern = (ITestPattern) _customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            var testPropertyValue = testPattern.CurrentDoubleValue;
            Assert.AreEqual(3.1415, testPropertyValue);

            // We cannot request the property directly,
            // since it is declared as a method, not a property.
            //double testPropertyValue2 = (double)this.customElement.GetCurrentPropertyValue(schema.DoubleValueProperty.PropertyId);
            //Assert.AreEqual(3.1415, testPropertyValue2);

            // Call a pattern passer
            double testPropertyValue3;
            testPattern.PassDoubleParam(1.772, out testPropertyValue3);
            Assert.AreEqual(1.772, testPropertyValue3);
        }

        /// <summary>
        /// Test support for Element values using Test pattern
        /// </summary>
        [Test]
        public void TestElementSupport()
        {
            var schema = TestSchema.GetInstance();

            // Ask for the custom pattern
            var testPattern = (ITestPattern) _customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            // We expect to get the custom element
            var testPropertyValue = testPattern.CurrentElementValue;
            var compareResult = _factory.CompareElements(_customElement, testPropertyValue);
            Assert.AreNotEqual(0, compareResult);

            // Call for the custom property directly
            // We cannot request the property directly,
            // since it is declared as a method, not a property.
            //Interop.UIAutomationClient.IUIAutomationElement testPropertyValue2 = (Interop.UIAutomationClient.IUIAutomationElement)this.customElement.GetCurrentPropertyValue(schema.ElementValueProperty.PropertyId);
        }
    }
}