using System;
using System.Drawing;
using Interop.UIAutomationCore;

namespace UIAControls
{
    /// <summary>
    /// Provider for the TriColor control itself.
    /// </summary>
    public class TriColorProvider : BaseFragmentRootProvider, IValueProvider, ISelectionProvider, IColorProvider
    {
        private readonly TriColorControl _control;

        public TriColorProvider(TriColorControl control)
        {
            _control = control;

            // Populate static properties
            //
            AddStaticProperty(UiaConstants.UIA_ControlTypePropertyId, UiaConstants.UIA_CustomControlTypeId);
            AddStaticProperty(UiaConstants.UIA_LocalizedControlTypePropertyId, "tri-color picker");
            AddStaticProperty(UiaConstants.UIA_ProviderDescriptionPropertyId, "UIASamples: Tri-Color Provider");
            AddStaticProperty(UiaConstants.UIA_HelpTextPropertyId,
                              "This is a color picker for a choice of three colors.  Use Up and Down arrows to move the selection between the colors.");
            // The WinForm name for this control makes a good Automation ID.
            AddStaticProperty(UiaConstants.UIA_AutomationIdPropertyId, _control.Name);
            AddStaticProperty(UiaConstants.UIA_IsKeyboardFocusablePropertyId, true);
            AddStaticProperty(UiaConstants.UIA_IsControlElementPropertyId, true);
            AddStaticProperty(UiaConstants.UIA_IsContentElementPropertyId, true);

            // Some properties are provided for me already by HWND provider
            // NativeWindowHandle, ProcessId, FrameworkId, IsEnabled, HasKeyboardFocus

            // Register for custom property
            ReadyStateSchema.GetInstance().Register();

            // Register for custom pattern
            ColorSchema.GetInstance().Register();

            // TEST: Register for test schema
            TestSchema.GetInstance().Register();
        }

        // Raise appropriate events for the value changing
        public void RaiseEventsForNewValue(TriColorValue oldValue, TriColorValue newValue)
        {
            // Since we support Value pattern, raise a PropertyChanged(Value) event
            // Values are represented as strings.
            NativeMethods.UiaRaiseAutomationPropertyChangedEvent(this,
                                                                 UiaConstants.UIA_ValueValuePropertyId,
                                                                 oldValue.ToString(),
                                                                 newValue.ToString());

            // Since we support Selection pattern, raise a SelectionChanged event.
            // Since a top-level AutomationEvent exists for this event, we raise it,
            // rather than raising PropertyChanged(Selection)
            NativeMethods.UiaRaiseAutomationEvent(
                this,
                UiaConstants.UIA_SelectionItem_ElementSelectedEventId);
        }

        public override ProviderOptions ProviderOptions
        {
            // Request COM threading style - all calls on main thread
            get
            {
                return (ProviderOptions) ((int) (ProviderOptions.ProviderOptions_ServerSideProvider |
                                                 ProviderOptions.ProviderOptions_UseComThreading));
            }
        }

        public override object GetPropertyValue(int propertyId)
        {
            if (propertyId == ReadyStateSchema.GetInstance().ReadyStateProperty.PropertyId)
            {
                return (_control.Value == TriColorValue.Green) ? "Ready" : "Not Ready";
            }

            return base.GetPropertyValue(propertyId);
        }

        public override object GetPatternProvider(int patternId)
        {
            // We just respond with ourself for the patterns we support.
            if (patternId == UiaConstants.UIA_ValuePatternId ||
                patternId == UiaConstants.UIA_SelectionPatternId ||
                patternId == ColorSchema.GetInstance().PatternId)
                return this;

            // TEST: Respond with a test schema object on request
            if (patternId == TestSchema.GetInstance().PatternId)
                return new TestPatternProvider(this);

            return base.GetPatternProvider(patternId);
        }

        protected override IntPtr GetWindowHandle()
        {
            // Return our window handle, since we're a root provider
            return _control.Handle;
        }

        protected override string GetName()
        {
            // This could certainly be a static property, but I'm leaving it
            // as an illustration of a function-provided property.

            // This should be localized; it's hard-coded only for the sample.
            return "Ready state";
        }

        protected override IRawElementProviderFragment GetFirstChild()
        {
            // Return our first child, which is the fragment for Red
            return new TriColorFragmentProvider(_control, this, TriColorValue.Red);
        }

        protected override IRawElementProviderFragment GetLastChild()
        {
            // Return our last child, which is the fragment for Green
            return new TriColorFragmentProvider(_control, this, TriColorValue.Green);
        }

        // Check to see if the passed point is a hit on one of our children
        public override IRawElementProviderFragment ElementProviderFromPoint(double x, double y)
        {
            // Convert screen point to client point
            var clientPoint = _control.PointToClient(
                new Point((int) x, (int) y));

            // Have the control do a hit test and see what value this is
            TriColorValue value;
            if (_control.ValueFromPoint(clientPoint, out value))
            {
                // Return the appropriate fragment
                return new TriColorFragmentProvider(_control, this, value);
            }
            return null;
        }

        public int IsReadOnly
        {
            get { return 0; }
        }

        // Getting the value is easy - it's just the control's value turning into a string.
        public string Value
        {
            get { return _control.Value.ToString(); }
        }

        // Setting the value requires turning a string back into the value
        public void SetValue(string value)
        {
            // This will throw an ArgumentException if it doesn't work
            _control.Value = (TriColorValue) Enum.Parse(typeof (TriColorValue), value, true /* ignoreCase */);
        }

        // This provider does not support multiple selection
        public int CanSelectMultiple
        {
            get { return 0; }
        }

        // This provider does require that there always be a selection
        public int IsSelectionRequired
        {
            get { return 1; }
        }

        // Get the current selection as an array of providers
        public IRawElementProviderSimple[] GetSelection()
        {
            // Create the fragment for the current value
            var selectedFragment =
                new TriColorFragmentProvider(_control, this, _control.Value);

            // Return it as a single-element array
            return new IRawElementProviderSimple[1] {selectedFragment};
        }

        int IColorProvider.ValueAsColor
        {
            get
            {
                switch (_control.Value)
                {
                    case TriColorValue.Red:
                        return Color.Red.ToArgb();
                    case TriColorValue.Yellow:
                        return Color.Yellow.ToArgb();
                    case TriColorValue.Green:
                        return Color.Green.ToArgb();
                }
                return 0;
            }
        }

        void IColorProvider.SetValueAsColor(int value)
        {
            var found = false;

            var newValue = TriColorValue.Red;
            if (value == (Color.Red.ToArgb() & 0xFFFFFF))
            {
                newValue = TriColorValue.Red;
                found = true;
            }
            else if (value == (Color.Yellow.ToArgb() & 0xFFFFFF))
            {
                newValue = TriColorValue.Yellow;
                found = true;
            }
            else if (value == (Color.Green.ToArgb() & 0xFFFFFF))
            {
                newValue = TriColorValue.Green;
                found = true;
            }
            if (found)
            {
                _control.Value = newValue;
            }
        }
    }
}