using System;
using System.Windows.Automation.Providers;
using Interop.UIAutomationCore;
using UIAutomationClient;
using IRawElementProviderFragment = Interop.UIAutomationCore.IRawElementProviderFragment;
using IRawElementProviderFragmentRoot = Interop.UIAutomationCore.IRawElementProviderFragmentRoot;
using IRawElementProviderSimple = Interop.UIAutomationCore.IRawElementProviderSimple;
using ISelectionItemProvider = Interop.UIAutomationCore.ISelectionItemProvider;
using ProviderOptions = Interop.UIAutomationCore.ProviderOptions;

namespace UIAControls
{
    /// <summary>
    /// Provider for the color-bar fragments within the TriColor control.
    /// </summary>
    public class TriColorFragmentProvider : BaseFragmentProvider, ISelectionItemProvider
    {
        private readonly TriColorControl _control;
        private readonly TriColorValue _value;

        public TriColorFragmentProvider(TriColorControl control, IRawElementProviderFragmentRoot root, TriColorValue value)
            : base((IRawElementProviderFragment) root /* parent */, root /* fragmentRoot */)
        {
            _control = control;
            _value = value;

            // Populate static properties
            //
            // In a production app, Name should be localized
            AddStaticProperty(UIA_PropertyIds.UIA_NamePropertyId, _value.ToString());
            AddStaticProperty(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_CustomControlTypeId);
            // In a production app, LocalizedControlType should be localized
            AddStaticProperty(UIA_PropertyIds.UIA_LocalizedControlTypePropertyId, "tri-color item");
            AddStaticProperty(UIA_PropertyIds.UIA_ProviderDescriptionPropertyId, "UIASamples: Tri-Color Fragment Provider");
            AddStaticProperty(UIA_PropertyIds.UIA_AutomationIdPropertyId, _value.ToString());
            AddStaticProperty(UIA_PropertyIds.UIA_IsKeyboardFocusablePropertyId, false);
            AddStaticProperty(UIA_PropertyIds.UIA_IsControlElementPropertyId, true);
            AddStaticProperty(UIA_PropertyIds.UIA_IsContentElementPropertyId, false);
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

        public override object GetPatternProvider(int patternId)
        {
            if (patternId == UIA_PatternIds.UIA_SelectionItemPatternId)
            {
                return this;
            }

            return base.GetPatternProvider(patternId);
        }

        // Create a runtime ID.  Since there is only one fragment per value,
        // the value turned into an integer is a unique identifier that we
        // can use as the runtime ID.
        public override int[] GetRuntimeId()
        {
            var runtimeId = new int[2];
            runtimeId[0] = AutomationInteropProvider.AppendRuntimeId;
            runtimeId[1] = (int) _value;
            return runtimeId;
        }

        // Get the bounding rect by consulting the control.
        public override UiaRect get_BoundingRectangle()
        {
            // Bounding rects must be in screen coordinates
            var screenRect = _control.RectangleToScreen(
                _control.RectFromValue(_value));
            var result = new UiaRect
                         {
                             left = screenRect.Left,
                             top = screenRect.Top,
                             width = screenRect.Width,
                             height = screenRect.Height
                         };
            return result;
        }

        // Return the fragment for the next value
        protected override IRawElementProviderFragment GetNextSibling()
        {
            if (!TriColorValueHelper.IsLast(_value))
            {
                return new TriColorFragmentProvider(
                    _control,
                    fragmentRoot,
                    TriColorValueHelper.NextValue(_value));
            }
            return null;
        }

        // Return the fragment for the previous value
        protected override IRawElementProviderFragment GetPreviousSibling()
        {
            if (!TriColorValueHelper.IsFirst(_value))
            {
                return new TriColorFragmentProvider(
                    _control,
                    fragmentRoot,
                    TriColorValueHelper.PreviousValue(_value));
            }
            return null;
        }

        // Select this item
        public void Select()
        {
            // Set the control's value to be the value of this fragment
            _control.Value = _value;
        }

        // Is this item selected?
        public int IsSelected
        {
            get
            {
                // This item is selected iff the control's value is the fragment's value
                return (_control.Value == _value) ? 1 : 0;
            }
        }

        // Adding is not valid for a single-select control
        public void AddToSelection()
        {
            throw new InvalidOperationException();
        }

        // Removing is not valid for a single-select control
        public void RemoveFromSelection()
        {
            throw new InvalidOperationException();
        }

        // The selection container is simply our fragment root
        public IRawElementProviderSimple SelectionContainer
        {
            get { return (IRawElementProviderSimple) fragmentRoot; }
        }
    }
}