using System;
using System.Collections.Generic;
using Interop.UIAutomationCore;

namespace UIAControls
{
    /// <summary>
    /// Provider for the color-bar fragments within the TriColor control.
    /// </summary>
    public class TriColorFragmentProvider : BaseFragmentProvider, ISelectionItemProvider
    {
        public TriColorFragmentProvider(TriColorControl control, IRawElementProviderFragmentRoot root, TriColorValue value)
            : base((IRawElementProviderFragment)root /* parent */, root /* fragmentRoot */)
        {
            this.control = control;
            this.value = value;

            // Populate static properties
            //
            // In a production app, Name should be localized
            AddStaticProperty(UiaConstants.UIA_NamePropertyId, this.value.ToString());
            AddStaticProperty(UiaConstants.UIA_ControlTypePropertyId, UiaConstants.UIA_CustomControlTypeId);
            // In a production app, LocalizedControlType should be localized
            AddStaticProperty(UiaConstants.UIA_LocalizedControlTypePropertyId, "tri-color item");
            AddStaticProperty(UiaConstants.UIA_ProviderDescriptionPropertyId, "UIASamples: Tri-Color Fragment Provider");
            AddStaticProperty(UiaConstants.UIA_AutomationIdPropertyId, this.value.ToString());
            AddStaticProperty(UiaConstants.UIA_IsKeyboardFocusablePropertyId, false);
            AddStaticProperty(UiaConstants.UIA_IsControlElementPropertyId, true);
            AddStaticProperty(UiaConstants.UIA_IsContentElementPropertyId, false);
        }

        public override ProviderOptions ProviderOptions
        {
            // Request COM threading style - all calls on main thread
            get
            {
                return (ProviderOptions)((int)(ProviderOptions.ProviderOptions_ServerSideProvider |
                    ProviderOptions.ProviderOptions_UseComThreading));
            }
        }

        public override object GetPatternProvider(int patternId)
        {
            if (patternId == UiaConstants.UIA_SelectionItemPatternId)
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
            int[] runtimeId = new int[2];
            runtimeId[0] = UiaConstants.AppendRuntimeId;
            runtimeId[1] = (int)this.value;
            return runtimeId;
        }

        // Get the bounding rect by consulting the control.
        public override UiaRect get_BoundingRectangle()
        {
            // Bounding rects must be in screen coordinates
            System.Drawing.Rectangle screenRect = this.control.RectangleToScreen(
                this.control.RectFromValue(this.value));
            UiaRect result = new UiaRect();
            result.left = screenRect.Left;
            result.top = screenRect.Top;
            result.width = screenRect.Width;
            result.height = screenRect.Height;
            return result;
        }

        // Return the fragment for the next value
        protected override IRawElementProviderFragment GetNextSibling()
        {
            if (!TriColorValueHelper.IsLast(this.value))
            {
                return new TriColorFragmentProvider(
                   this.control,
                   this.fragmentRoot,
                   TriColorValueHelper.NextValue(this.value));
            }
            return null;
        }

        // Return the fragment for the previous value
        protected override IRawElementProviderFragment GetPreviousSibling()
        {
            if (!TriColorValueHelper.IsFirst(this.value))
            {
                return new TriColorFragmentProvider(
                   this.control,
                   this.fragmentRoot,
                   TriColorValueHelper.PreviousValue(this.value));
            }
            return null;
        }

        #region ISelectionItemProvider Members

        // Select this item
        public void Select()
        {
            // Set the control's value to be the value of this fragment
            this.control.Value = this.value;
        }

        // Is this item selected?
        public int IsSelected
        {
            get
            {
                // This item is selected iff the control's value is the fragment's value
                return (this.control.Value == this.value) ? 1 : 0;
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
            get
            {
                return (IRawElementProviderSimple)this.fragmentRoot;
            }
        }

        #endregion

        private TriColorControl control;
        private TriColorValue value;
    }
}
