using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interop.UIAutomationCore;
using Interop.UIAutomationClient;

namespace UiaControlsTest
{
    /// <summary>
    /// A set of unit tests for the UIA Custom Patterns demo
    /// </summary>
    [TestClass]
    public class CustomPatternUnitTest
    {
        private TargetApp app;
        private IUIAutomation factory;
        private IUIAutomationElement customElement;

        public CustomPatternUnitTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            // Create the factory and register schemas
            this.factory = new CUIAutomationClass();
            UIAControls.ReadyStateSchema.GetInstance().Register();
            UIAControls.TestSchema.GetInstance().Register();
            UIAControls.ColorSchema.GetInstance().Register();

            // Start the app
            string curDir = Environment.CurrentDirectory;
            this.app = new TargetApp(curDir + "\\UiaControls.exe");
            this.app.Start();

            // Find the main control
            IUIAutomationElement appElement = this.factory.ElementFromHandle(app.MainWindow);
            IUIAutomationCondition condition = this.factory.CreatePropertyCondition(
                 UIA_PropertyIds.UIA_AutomationIdPropertyId,
                 "triColorControl1");
            this.customElement = appElement.FindFirst(TreeScope.TreeScope_Children,
                 condition);
            Assert.IsNotNull(this.customElement);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            app.Dispose();
            app = null;
        }

        [TestMethod]
        public void TestReadyStateProperty()
        {
            // Query our custom property
            object readyStateValue = this.customElement.GetCurrentPropertyValue(UIAControls.ReadyStateSchema.GetInstance().ReadyStateProperty.PropertyId);
            Assert.IsInstanceOfType(readyStateValue, typeof(string));

            // By default, UIAControls.exe launches in not-ready state
            Assert.AreEqual("Not Ready", (string)readyStateValue);
        }

        /// <summary>
        /// Validate that the Color pattern is working for the main control
        /// </summary>
        [TestMethod]
        public void TestColorPattern()
        {
            UIAControls.ColorSchema schema = UIAControls.ColorSchema.GetInstance();

            // Ask for the custom pattern
            UIAControls.IColorPattern colorPattern = (UIAControls.IColorPattern)
                this.customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(colorPattern);

            // Call through a pattern getter
            int colorAsValue = colorPattern.CurrentValueAsColor;
            Assert.AreEqual(0xFFFF0000, (uint)colorAsValue);

            // Call a pattern setter
            colorPattern.SetValueAsColor(System.Convert.ToInt32(0x008000));

            // Call for a custom property
            int colorAsValue2 = (int)this.customElement.GetCurrentPropertyValue(schema.ValueAsColorProperty.PropertyId);
            Assert.AreEqual(0xFF008000, (uint)colorAsValue2);
        }

        /// <summary>
        /// Test support for Int values using Test pattern
        /// </summary>
        [TestMethod]
        public void TestIntSupport()
        {
            UIAControls.TestSchema schema = UIAControls.TestSchema.GetInstance();

            // Ask for the custom pattern
            UIAControls.ITestPattern testPattern = (UIAControls.ITestPattern)
                this.customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            int testPropertyValue = testPattern.CurrentIntValue;
            Assert.AreEqual(42, testPropertyValue);

            // Call for the custom property directly
            int testPropertyValue2 = (int)this.customElement.GetCurrentPropertyValue(schema.IntValueProperty.PropertyId);
            Assert.AreEqual(42, testPropertyValue2);

            // Call a pattern passer
            int testPropertyValue3;
            testPattern.PassIntParam(82, out testPropertyValue3);
            Assert.AreEqual(82, testPropertyValue3);
        }

        // <summary>
        // Test support for String values using Test pattern
        // </summary>
        [TestMethod]
        public void TestStringSupport()
        {
            UIAControls.TestSchema schema = UIAControls.TestSchema.GetInstance();
            
            // Ask for the custom pattern
            UIAControls.ITestPattern testPattern = (UIAControls.ITestPattern)
                this.customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            string testPropertyValue = testPattern.CurrentStringValue;
            Assert.AreEqual("TestString", testPropertyValue);

            // Call for the custom property directly
            string testPropertyValue2 = (string)this.customElement.GetCurrentPropertyValue(schema.StringValueProperty.PropertyId);
            Assert.AreEqual("TestString", testPropertyValue2);

            // Call a pattern passer
            string testPropertyValue3;
            testPattern.PassStringParam("String2", out testPropertyValue3);
            Assert.AreEqual("String2", testPropertyValue3);
        }

        /// <summary>
        /// Test support for Bool values using Test pattern
        /// </summary>
        [TestMethod]
        public void TestBoolSupport()
        {
            UIAControls.TestSchema schema = UIAControls.TestSchema.GetInstance();

            // Ask for the custom pattern
            UIAControls.ITestPattern testPattern = (UIAControls.ITestPattern)
                this.customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            bool testPropertyValue = testPattern.CurrentBoolValue;
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
        [TestMethod]
        public void TestDoubleSupport()
        {
            UIAControls.TestSchema schema = UIAControls.TestSchema.GetInstance();

            // Ask for the custom pattern
            UIAControls.ITestPattern testPattern = (UIAControls.ITestPattern)
                this.customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            double testPropertyValue = testPattern.CurrentDoubleValue;
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
        [TestMethod]
        public void TestElementSupport()
        {
            UIAControls.TestSchema schema = UIAControls.TestSchema.GetInstance();

            // Ask for the custom pattern
            UIAControls.ITestPattern testPattern = (UIAControls.ITestPattern)
                this.customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(testPattern);

            // Call through a pattern getter
            // We expect to get the custom element
            Interop.UIAutomationClient.IUIAutomationElement testPropertyValue = testPattern.CurrentElementValue;
            int compareResult = this.factory.CompareElements(this.customElement, testPropertyValue);
            Assert.AreNotEqual(0, compareResult);

            // Call for the custom property directly
            // We cannot request the property directly,
            // since it is declared as a method, not a property.
            //Interop.UIAutomationClient.IUIAutomationElement testPropertyValue2 = (Interop.UIAutomationClient.IUIAutomationElement)this.customElement.GetCurrentPropertyValue(schema.ElementValueProperty.PropertyId);
        }
    }
}
