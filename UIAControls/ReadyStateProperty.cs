using System;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;

/// Ready State Property
/// Declaration of a simple custom property to represent the readiness of a control.

namespace UIAControls
{
    /// <summary>
    /// Declaration of the pattern schema, with all of the information UIA needs
    /// about this property.
    /// </summary>
    public class ReadyStateSchema
    {
        static readonly ReadyStateSchema instance = new ReadyStateSchema();
        static public ReadyStateSchema GetInstance()
        {
            return ReadyStateSchema.instance;
        }

        public readonly UiaPropertyInfoHelper ReadyStateProperty =
            new UiaPropertyInfoHelper(
                new Guid("6E3383FB-96CF-485E-A796-FB6DE483B3DA"), 
                "ReadyState", 
                UIAutomationType.UIAutomationType_String);

        public void Register()
        {
            if (!this.registered)
            {
                // Get our pointer to the registrar
                Interop.UIAutomationCore.IUIAutomationRegistrar registrar =
                    new Interop.UIAutomationCore.CUIAutomationRegistrarClass();

                // Set up the property struct
                UIAutomationPropertyInfo propertyInfo = ReadyStateProperty.Data;

                // Register it
                int propertyId;
                registrar.RegisterProperty(
                    ref propertyInfo,
                    out propertyId);

                // Write the property ID back
                ReadyStateProperty.PropertyId = propertyId;

                this.registered = true;
            }
        }

        bool registered;
    }
}