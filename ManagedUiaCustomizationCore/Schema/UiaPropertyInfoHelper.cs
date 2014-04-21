using System;
using Interop.UIAutomationCore;

namespace ManagedUiaCustomizationCore
{
    /// <summary>
    /// Helper class to gather data about a custom property
    /// Corresponds to UIAutomationPropertyInfo
    /// </summary>
    public class UiaPropertyInfoHelper
    {
        private readonly Guid _propertyGuid;
        private readonly string _programmaticName;
        private readonly UIAutomationType _propertyType;
        private bool _built;
        private UIAutomationPropertyInfo _data;

        public UiaPropertyInfoHelper(Guid propertyGuid, string programmaticName, UIAutomationType propertyType)
        {
            _programmaticName = programmaticName;
            _propertyGuid = propertyGuid;
            _propertyType = propertyType;
        }

        /// <summary>
        /// Get a marshalled UIAutomationPropertyInfo struct for this Helper.
        /// </summary>
        public UIAutomationPropertyInfo Data
        {
            get
            {
                if (!_built)
                    Build();
                return _data;
            }
        }

        /// <summary>
        /// The UIA type of this property
        /// </summary>
        public UIAutomationType UiaType
        {
            get { return _propertyType; }
        }

        /// <summary>
        /// The unique identifier for this property
        /// </summary>
        public Guid Guid
        {
            get { return _propertyGuid; }
        }

        /// <summary>
        /// The index of this property, when it is used as part of a pattern
        /// </summary>
        public uint Index { get; set; }

        /// <summary>
        /// The property ID of this property, assigned after registration
        /// </summary>
        public int PropertyId { get; set; }

        private void Build()
        {
            _data = new UIAutomationPropertyInfo
                    {
                        pProgrammaticName = _programmaticName,
                        guid = _propertyGuid,
                        type = _propertyType
                    };
            _built = true;
        }
    }
}