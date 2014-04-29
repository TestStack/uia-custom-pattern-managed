using System;
using System.Windows.Automation;
using NUnit.Framework;
using UIAutomationClient;
using WpfAppWithAdvTextControl;
using NTreeScope = UIAutomationClient.TreeScope;
using WTreeScope = System.Windows.Automation.TreeScope;

namespace UiaControlsTest
{
    [TestFixture]
    public class WpfAppTests
    {
        private TargetApp _app;
        private IUIAutomation _nFactory;
        private IUIAutomationElement _nAdvancedTextBoxElement;
        private IUIAutomationElement _nTestControlElement;
        private AutomationElement _wAdvancedTextBoxElement;

        [SetUp]
        public void MyTestInitialize()
        {
            // Create the factory and register schemas
            _nFactory = new CUIAutomationClass();

            // Start the app
            var curDir = Environment.CurrentDirectory;
            _app = new TargetApp(curDir + "\\WpfAppWithAdvTextControl.exe");
            _app.Start();

            // Find the main control
            var appElement = _nFactory.ElementFromHandle(_app.MainWindow);

            var advTestBoxCondition = _nFactory.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, "advTextBox1");
            _nAdvancedTextBoxElement = appElement.FindFirst(NTreeScope.TreeScope_Children, advTestBoxCondition);
            Assert.IsNotNull(_nAdvancedTextBoxElement);

            var testControlCondition = _nFactory.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, "testControl");
            _nTestControlElement = appElement.FindFirst(NTreeScope.TreeScope_Children, testControlCondition);
            Assert.IsNotNull(_nTestControlElement);

            var window = AutomationElement.RootElement.FindFirst(WTreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "MainWindow"));
            _wAdvancedTextBoxElement = window.FindFirst(WTreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "advTextBox1"));
        }

        [TearDown]
        public void MyTestCleanup()
        {
            _app.Dispose();
            _app = null;
        }

        [Test]
        public void Native_CaretPositionPatternSmokeTest()
        {
            CaretPositionPattern.Initialize();
            var cps = (ICaretPositionPattern)_nAdvancedTextBoxElement.GetCurrentPattern(CaretPositionPattern.Pattern.Id);
            Assert.IsNotNull(cps);

            // sanity check: intial selection is none, there's no text after all
            Assert.AreEqual(0, cps.CurrentSelectionStart);
            Assert.AreEqual(0, cps.CurrentSelectionLength);

            // enter "abcd" and select central part of it - "bc"
            NSetTextAndSelection("abcd", 1, 2);
            Assert.AreEqual(1, cps.CurrentSelectionStart);
            Assert.AreEqual(2, cps.CurrentSelectionLength);

            // validate that selected text retrieved from TextPattern changed as expected
            var text = (IUIAutomationTextPattern)_nAdvancedTextBoxElement.GetCurrentPattern(TextPattern.Pattern.Id);
            var selectionArray = text.GetSelection();
            var selection = selectionArray.GetElement(0);
            var selectedString = selection.GetText(-1);
            Assert.AreEqual("bc", selectedString);
        }

        [Test]
        public void Wpf_CaretPositionPatternSmokeTest()
        {
            CaretPositionPattern.Initialize();
            var cps = (ICaretPositionPattern)_wAdvancedTextBoxElement.GetCurrentPattern(CaretPositionPattern.Pattern);
            Assert.IsNotNull(cps);

            // sanity check: intial selection is none, there's no text after all
            Assert.AreEqual(0, cps.CurrentSelectionStart);
            Assert.AreEqual(0, cps.CurrentSelectionLength);

            // enter "abcd" and select central part of it - "bc"
            WSetTextAndSelection("abcd", 1, 2);
            Assert.AreEqual(1, cps.CurrentSelectionStart);
            Assert.AreEqual(2, cps.CurrentSelectionLength);

            // validate that selected text retrieved from TextPattern changed as expected
            var text = (TextPattern)_wAdvancedTextBoxElement.GetCurrentPattern(TextPattern.Pattern);
            var selectionArray = text.GetSelection();
            var selection = selectionArray[0];
            var selectedString = selection.GetText(-1);
            Assert.AreEqual("bc", selectedString);
        }

        [Test]
        public void Native_GettingPropertyValueByIdWithoutPatternInterfaceWorks()
        {
            CaretPositionPattern.Initialize();
            NSetTextAndSelection("abcd", 1, 2);
            var nSelStart = _nAdvancedTextBoxElement.GetCurrentPropertyValue(CaretPositionPattern.SelectionStartProperty.Id);
            Assert.AreEqual(1, nSelStart);
        }

        private void NSetTextAndSelection(string text, int selectionStart, int selectionLength)
        {
            var cps = (ICaretPositionPattern)_nAdvancedTextBoxElement.GetCurrentPattern(CaretPositionPattern.Pattern.Id);
            var value = (IUIAutomationValuePattern)_nAdvancedTextBoxElement.GetCurrentPattern(ValuePattern.Pattern.Id);
            value.SetValue(text);
            cps.SetSelectionStart(selectionStart);
            cps.SetSelectionLength(selectionLength);
        }

        private void WSetTextAndSelection(string text, int selectionStart, int selectionLength)
        {
            var cps = (ICaretPositionPattern)_wAdvancedTextBoxElement.GetCurrentPattern(CaretPositionPattern.Pattern);
            var value = (ValuePattern)_wAdvancedTextBoxElement.GetCurrentPattern(ValuePattern.Pattern);
            value.SetValue(text);
            cps.SetSelectionStart(selectionStart);
            cps.SetSelectionLength(selectionLength);
        }

        [Test]
        [Ignore("Dut to bug in UIA implementation on Win7 it is not possible to have more than 2 properties on a pattern. You may use the test to detect if the bug is present in your system")]
        // On Win 8.1 issue was fixed it seems
        public void Native_TestOfMoreThanTwoPatternProperties()
        {
            TestOfMoreThanTwoPatternPropertiesPattern.Initialize();
            var pattern = (ITestOfMoreThanTwoPatternPropertiesPattern)_nTestControlElement.GetCurrentPattern(TestOfMoreThanTwoPatternPropertiesPattern.Pattern);

            Assert.AreEqual(421, pattern.CurrentProperty1);
            Assert.AreEqual(422, pattern.CurrentProperty2);
            Assert.AreEqual(423, pattern.CurrentProperty3);
        }
    }
}