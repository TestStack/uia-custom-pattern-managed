using System;
using System.Windows.Automation;
using NUnit.Framework;
using UIAutomationClient;
using WpfAppWithAdvTextControl;
using TreeScope = UIAutomationClient.TreeScope;

namespace UiaControlsTest
{
    [TestFixture]
    public class WpfAppTests
    {
        private TargetApp _app;
        private IUIAutomation _factory;
        private IUIAutomationElement _advancedTextBoxElement;

        [SetUp]
        public void MyTestInitialize()
        {
            // Create the factory and register schemas
            _factory = new CUIAutomationClass();

            // Start the app
            var curDir = Environment.CurrentDirectory;
            _app = new TargetApp(curDir + "\\WpfAppWithAdvTextControl.exe");
            _app.Start();

            // Find the main control
            var appElement = _factory.ElementFromHandle(_app.MainWindow);
            var condition = _factory.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, "advTextBox1");
            _advancedTextBoxElement = appElement.FindFirst(TreeScope.TreeScope_Children, condition);
            Assert.IsNotNull(_advancedTextBoxElement);
        }

        [TearDown]
        public void MyTestCleanup()
        {
            _app.Dispose();
            _app = null;
        }

        [Test]
        public void CaretPositionPatternSmokeTest()
        {
            CaretPositionPattern.Initialize();
            var cps = (ICaretPositionPattern)_advancedTextBoxElement.GetCurrentPattern(CaretPositionPattern.Pattern);
            Assert.IsNotNull(cps);

            // sanity check: intial selection is none, there's no text after all
            Assert.AreEqual(0, cps.CurrentSelectionStart);
            Assert.AreEqual(0, cps.CurrentSelectionLength);
            
            // enter "abcd" and select central part of it - "bc"
            var value = (IUIAutomationValuePattern)_advancedTextBoxElement.GetCurrentPattern(ValuePattern.Pattern.Id);
            value.SetValue("abcd");
            cps.SetSelectionStart(1);
            cps.SetSelectionLength(2);
            Assert.AreEqual(1, cps.CurrentSelectionStart);
            Assert.AreEqual(2, cps.CurrentSelectionLength);

            // validate that selected text retrieved from TextPattern changed as expected
            var text = (IUIAutomationTextPattern)_advancedTextBoxElement.GetCurrentPattern(TextPattern.Pattern.Id);
            var selectionArray = text.GetSelection();
            var selection = selectionArray.GetElement(0);
            var selectedString = selection.GetText(-1);
            Assert.AreEqual("bc", selectedString);
        }
    }
}