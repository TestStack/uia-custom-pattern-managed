using System;
using NUnit.Framework;
using UiaControlsTest;
using UIAutomationClient;
using WpfAppWithAdvTextControl;

namespace WpfControlTest
{
    [TestFixture]
    public class CaretPositionPatternTest
    {
        private TargetApp _app;
        private IUIAutomation _factory;
        private IUIAutomationElement _customElement;

        [SetUp]
        public void MyTestInitialize()
        {
            // Create the factory and register schemas
            _factory = new CUIAutomationClass();
            CaretPositionSchema.Instance.Register(makeAugmentationForWpfPeers: true);

            // Start the app
            var curDir = Environment.CurrentDirectory;
            _app = new TargetApp(curDir + "\\WpfAppWithAdvTextControl.exe");
            _app.Start();

            // Find the main control
            var appElement = _factory.ElementFromHandle(_app.MainWindow);
            var condition = _factory.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, "advTextBox1");
            _customElement = appElement.FindFirst(TreeScope.TreeScope_Children, condition);
            Assert.IsNotNull(_customElement);
        }

        [TearDown]
        public void MyTestCleanup()
        {
            _app.Dispose();
            _app = null;
        }

        [Test]
        public void Smoke()
        {
            var schema = CaretPositionSchema.Instance;
            var cps = (ICaretPositionPattern)_customElement.GetCurrentPattern(schema.PatternId);
            Assert.IsNotNull(cps);

            Assert.AreEqual(0, cps.CurrentSelectionStart);
            Assert.AreEqual(0, cps.CurrentSelectionLength);
        }
    }
}