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
        private AutomationElement _wTestControlElement;

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
            _wTestControlElement = window.FindFirst(WTreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "testControl"));

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

        [Test]
        public void Wpf_GettingPropertyValueByIdWithoutPatternInterfaceWorks()
        {
            CaretPositionPattern.Initialize();
            WSetTextAndSelection("abcd", 1, 2);
            var selStart = _wAdvancedTextBoxElement.GetCurrentPropertyValue(CaretPositionPattern.SelectionStartProperty);
            Assert.AreEqual(1, selStart);
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

        [Test]
        public void Native_EnumTypedProperty_RetrievedCorrectly()
        {
            TestOfMoreThanTwoPatternPropertiesPattern.Initialize();
            var pattern = (ITestOfMoreThanTwoPatternPropertiesPattern)_nTestControlElement.GetCurrentPattern(TestOfMoreThanTwoPatternPropertiesPattern.Pattern);
            
            Assert.AreEqual(TestEnum.EnumValue42, pattern.GetEnum());
        }

        [Test]
        public void Wpf_StandaloneProperty_RetrievedCorrectly()
        {
            TestOfMoreThanTwoPatternPropertiesPattern.Initialize();
            var val = _wTestControlElement.GetCurrentPropertyValue(TestOfMoreThanTwoPatternPropertiesPattern.Standalone1Property);
            Assert.AreEqual(42, val);
        }

        [Test]
        public void Wpf_NullStringStandaloneProperty_RetrievedAsEmptyString()
        {
            TestOfMoreThanTwoPatternPropertiesPattern.Initialize();
            var val = (string)_wTestControlElement.GetCurrentPropertyValue(TestOfMoreThanTwoPatternPropertiesPattern.NullStringStandaloneProperty);
            Assert.IsEmpty(val);
        }

        [Test]
        public void Native_GetElementThroughProperty_Works()
        {
            AutomationElementRetievingPattern.Initialize();
            var p = (IAutomationElementRetievingPattern)_nTestControlElement.GetCurrentPattern(AutomationElementRetievingPattern.Pattern.Id);
            
            var nElementFromProperty = p.CurrentNativeElement;
            Assert.IsTrue(_nFactory.CompareElements(_nTestControlElement, nElementFromProperty) != 0);
        }

        [Test]
        public void Native_GetElementThroughMethodReturnValue_Works()
        {
            AutomationElementRetievingPattern.Initialize();
            var p = (IAutomationElementRetievingPattern)_nTestControlElement.GetCurrentPattern(AutomationElementRetievingPattern.Pattern.Id);

            var nElementFromMethodRetVal = p.NativeGetCurrentElement();
            Assert.IsTrue(_nFactory.CompareElements(_nTestControlElement, nElementFromMethodRetVal) != 0);
        }

        [Test]
        public void Native_GetNullElementThroughMethodReturnValue_Works()
        {
            AutomationElementRetievingPattern.Initialize();
            var p = (IAutomationElementRetievingPattern)_nTestControlElement.GetCurrentPattern(AutomationElementRetievingPattern.Pattern.Id);

            var nNullElementFromMethodRetVal = p.NativeGetNullElement();
            Assert.IsNull(nNullElementFromMethodRetVal);
        }

        [Test]
        public void Native_GetElementViaOutParam_Works()
        {
            AutomationElementRetievingPattern.Initialize();
            var p = (IAutomationElementRetievingPattern)_nTestControlElement.GetCurrentPattern(AutomationElementRetievingPattern.Pattern.Id);

            IUIAutomationElement nElementFromMethodOutParam;
            p.NativeGetCurrentElementWithOutParam(out nElementFromMethodOutParam);
            Assert.IsTrue(_nFactory.CompareElements(_nTestControlElement, nElementFromMethodOutParam) != 0);
        }

        [Test]
        public void Wpf_GetElementThroughProperty_Works()
        {
            AutomationElementRetievingPattern.Initialize();
            var p = (IAutomationElementRetievingPattern)_wTestControlElement.GetCurrentPattern(AutomationElementRetievingPattern.Pattern);

            var wElementFromProperty = p.CurrentWrappedElement;
            Assert.IsTrue(_wTestControlElement.Equals(wElementFromProperty));
        }

        [Test]
        public void Wpf_GetElementThroughMethodReturnValue_Works()
        {
            AutomationElementRetievingPattern.Initialize();
            var p = (IAutomationElementRetievingPattern)_wTestControlElement.GetCurrentPattern(AutomationElementRetievingPattern.Pattern);

            var wElementFromProperty = p.WrappedGetCurrentElement();
            Assert.IsTrue(_wTestControlElement.Equals(wElementFromProperty));
        }

        [Test]
        public void Wpf_GetNullElementThroughMethodReturnValue_Works()
        {
            AutomationElementRetievingPattern.Initialize();
            var p = (IAutomationElementRetievingPattern)_wTestControlElement.GetCurrentPattern(AutomationElementRetievingPattern.Pattern);

            var wNullElementFromProperty = p.WrappedGetNullElement();
            Assert.IsNull(wNullElementFromProperty);
        }

        [Test]
        public void Wpf_GetElementThroughMethodOutParam_Works()
        {
            AutomationElementRetievingPattern.Initialize();
            var p = (IAutomationElementRetievingPattern)_wTestControlElement.GetCurrentPattern(AutomationElementRetievingPattern.Pattern);

            AutomationElement wElementFromProperty;
            p.WrappedGetCurrentElementWithOutParam(out wElementFromProperty);
            Assert.IsTrue(_wTestControlElement.Equals(wElementFromProperty));
        }
    }
}