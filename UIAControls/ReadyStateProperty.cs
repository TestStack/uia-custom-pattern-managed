using System;
using Interop.UIAutomationCore;
using ManagedUiaCustomizationCore;

// Ready State Property
// Declaration of a simple custom property to represent the readiness of a control.

namespace UIAControls
{
    /// <summary>
    /// Declaration of the pattern schema, with all of the information UIA needs
    /// about this property.
    /// </summary>
    public class ReadyStateSchema
    {
        private bool _registered;

        private static readonly ReadyStateSchema Instance = new ReadyStateSchema();

        public static ReadyStateSchema GetInstance()
        {
            return Instance;
        }

        public readonly UiaPropertyInfoHelper ReadyStateProperty =
            new UiaPropertyInfoHelper(
                new Guid("6E3383FB-96CF-485E-A796-FB6DE483B3DA"),
                "ReadyState",
                UIAutomationType.UIAutomationType_String);

        public void Register()
        {
            if (!_registered)
            {
                // Get our pointer to the registrar
                IUIAutomationRegistrar registrar =
                    new CUIAutomationRegistrarClass();

                // Set up the property struct
                var propertyInfo = ReadyStateProperty.Data;

                // Register it
                int propertyId;
                registrar.RegisterProperty(
                    ref propertyInfo,
                    out propertyId);

                // Write the property ID back
                ReadyStateProperty.PropertyId = propertyId;

                _registered = true;
            }
        }
    }
}