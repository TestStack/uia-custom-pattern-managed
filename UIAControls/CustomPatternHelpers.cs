using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.UIAutomationClient;
using Interop.UIAutomationCore;
using IRawElementProviderSimple = Interop.UIAutomationCore.IRawElementProviderSimple;

// CustomPatternHelpers: a set of classes to help implement UI Automation custom patterns
// in managed code.
//
// It is often difficult to implement the UI Automation structures needed for custom
// patterns from managed code; there is a class in this file for each UIA structure to
// help implement it.

namespace UIAControls
{
    /// <summary>
    /// A description of a single parameter
    /// 
    /// This does not match a UIA structure, but is used as a simple data class
    /// to help form those structures.
    /// </summary>
    public class UiaParameterDescription
    {
        private readonly string _name;
        private readonly UIAutomationType _uiaType;

        public UiaParameterDescription(string name, UIAutomationType type)
        {
            _name = name;
            _uiaType = type;
        }

        public string Name
        {
            get { return _name; }
        }

        public UIAutomationType UiaType
        {
            get { return _uiaType; }
        }
    }

    /// <summary>
    /// Helper class to assemble information about a custom method
    /// Corresponds to UIAutomationMethodInfo.
    /// </summary>
    public class UiaMethodInfoHelper
    {
        private readonly string _programmaticName;
        private readonly bool _doSetFocus;
        private readonly List<IntPtr> _inParamNames = new List<IntPtr>();
        private readonly List<UIAutomationType> _inParamTypes = new List<UIAutomationType>();
        private readonly List<IntPtr> _outParamNames = new List<IntPtr>();
        private readonly List<UIAutomationType> _outParamTypes = new List<UIAutomationType>();
        private bool _built;
        private UIAutomationMethodInfo _data;

        public UiaMethodInfoHelper(string programmaticName, bool doSetFocus)
        {
            _programmaticName = programmaticName;
            _doSetFocus = doSetFocus;
        }

        public UiaMethodInfoHelper(string programmaticName, bool doSetFocus, UiaParameterDescription[] uiaParams)
        {
            _programmaticName = programmaticName;
            _doSetFocus = doSetFocus;

            foreach (var param in uiaParams)
            {
                AddParameter(param.Name, param.UiaType);
            }
        }

        ~UiaMethodInfoHelper()
        {
            foreach (var marshalledName in _inParamNames)
            {
                Marshal.FreeCoTaskMem(marshalledName);
            }
            foreach (var marshalledName in _outParamNames)
            {
                Marshal.FreeCoTaskMem(marshalledName);
            }

            Marshal.FreeCoTaskMem(_data.pParameterNames);
            Marshal.FreeCoTaskMem(_data.pParameterTypes);
        }

        /// <summary>
        /// Get a marshalled UIAutomationMethodInfo struct for this Helper.
        /// </summary>
        public UIAutomationMethodInfo Data
        {
            get
            {
                if (!_built)
                {
                    Build();
                }
                return _data;
            }
        }

        /// <summary>
        /// The array of in-parameter types.
        /// </summary>
        public UIAutomationType[] InParamTypes
        {
            get { return _inParamTypes.ToArray(); }
        }

        /// <summary>
        /// The array of out-parameter types.
        /// </summary>
        public UIAutomationType[] OutParamTypes
        {
            get { return _outParamTypes.ToArray(); }
        }

        /// <summary>
        /// The index of this method.  
        /// In a UIA custom pattern, every method (and pattern property)
        /// has an assigned index.
        /// </summary>
        public uint Index { get; set; }

        /// <summary>
        /// Add a parameter to the list of parameters for this method.
        /// </summary>
        public void AddParameter(string name, UIAutomationType type)
        {
            var marshalledName = Marshal.StringToCoTaskMemUni(name);
            if ((type & UIAutomationType.UIAutomationType_Out) == 0)
            {
                _inParamNames.Add(marshalledName);
                _inParamTypes.Add(type);
            }
            else
            {
                _outParamNames.Add(marshalledName);
                _outParamTypes.Add(type);
            }
        }

        /// <summary>
        /// Marshal our data to the UIAutomationMethodInfo struct.
        /// </summary>
        private void Build()
        {
            // Copy basic data
            _data = new UIAutomationMethodInfo
                    {
                        pProgrammaticName = _programmaticName,
                        doSetFocus = _doSetFocus ? 1 : 0,
                        cInParameters = (uint) _inParamNames.Count,
                        cOutParameters = (uint) _outParamNames.Count
                    };
            var cTotalParameters = _data.cInParameters + _data.cOutParameters;

            // Allocate parameter lists and populate them
            if (cTotalParameters > 0)
            {
                _data.pParameterNames = Marshal.AllocCoTaskMem((int) (cTotalParameters*Marshal.SizeOf(typeof (IntPtr))));
                _data.pParameterTypes = Marshal.AllocCoTaskMem((int) (cTotalParameters*Marshal.SizeOf(typeof (Int32))));

                var namePointer = _data.pParameterNames;
                var typePointer = _data.pParameterTypes;
                for (var i = 0; i < _data.cInParameters; ++i)
                {
                    Marshal.WriteIntPtr(namePointer, _inParamNames[i]);
                    namePointer = (IntPtr) (namePointer.ToInt64() + Marshal.SizeOf(typeof (IntPtr)));
                    Marshal.WriteInt32(typePointer, (int) _inParamTypes[i]);
                    typePointer = (IntPtr) (typePointer.ToInt64() + Marshal.SizeOf(typeof (Int32)));
                }

                for (var i = 0; i < _data.cOutParameters; ++i)
                {
                    Marshal.WriteIntPtr(namePointer, _outParamNames[i]);
                    namePointer = (IntPtr) (namePointer.ToInt64() + Marshal.SizeOf(typeof (IntPtr)));
                    Marshal.WriteInt32(typePointer, (int) _outParamTypes[i]);
                    typePointer = (IntPtr) (typePointer.ToInt64() + Marshal.SizeOf(typeof (Int32)));
                }
            }
            else
            {
                _data.pParameterNames = IntPtr.Zero;
                _data.pParameterTypes = IntPtr.Zero;
            }

            _built = true;
        }
    }

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

    /// <summary>
    /// Helper class to gather data about a custom event
    /// Corresponds to UIAutomationEventInfo.
    /// </summary>
    public class UiaEventInfoHelper
    {
        private readonly Guid _eventGuid;
        private readonly string _programmaticName;
        private bool _built;
        private UIAutomationEventInfo _data;

        public UiaEventInfoHelper(Guid eventGuid, string programmaticName)
        {
            _programmaticName = programmaticName;
            _eventGuid = eventGuid;
        }

        /// <summary>
        /// Get a marshalled UIAutomationEventInfo struct for this Helper.
        /// </summary>
        public UIAutomationEventInfo Data
        {
            get
            {
                if (!_built)
                    Build();
                return _data;
            }
        }

        /// <summary>
        /// The unique identifier of this event
        /// </summary>
        public Guid Guid
        {
            get { return _eventGuid; }
        }

        /// <summary>
        /// The event ID of this event, assigned after registration
        /// </summary>
        public int EventId { get; set; }

        private void Build()
        {
            _data = new UIAutomationEventInfo {pProgrammaticName = _programmaticName, guid = _eventGuid};
            _built = true;
        }
    }

    /// <summary>
    /// Helper class to assemble information about a custom pattern.
    /// Corresponds to UIAutomationPatternInfo
    /// </summary>
    public class UiaPatternInfoHelper
    {
        private readonly Guid _patternGuid;
        private readonly Guid _clientInterfaceId;
        private readonly Guid _providerInterfaceId;
        private readonly string _programmaticName;
        private readonly IUIAutomationPatternHandler _patternHandler;
        private readonly List<UiaMethodInfoHelper> _methods = new List<UiaMethodInfoHelper>();
        private readonly List<UiaPropertyInfoHelper> _properties = new List<UiaPropertyInfoHelper>();
        private readonly List<UiaEventInfoHelper> _events = new List<UiaEventInfoHelper>();

        private bool _built;
        private UIAutomationPatternInfo _data;

        public UiaPatternInfoHelper(Guid patternGuid,
                                    string programmaticName,
                                    Guid clientInterfaceId,
                                    Guid providerInterfaceId,
                                    IUIAutomationPatternHandler patternHandler)
        {
            _programmaticName = programmaticName;
            _patternGuid = patternGuid;
            _clientInterfaceId = clientInterfaceId;
            _providerInterfaceId = providerInterfaceId;
            _patternHandler = patternHandler;
        }

        ~UiaPatternInfoHelper()
        {
            Marshal.FreeCoTaskMem(_data.pMethods);
            Marshal.FreeCoTaskMem(_data.pEvents);
            Marshal.FreeCoTaskMem(_data.pProperties);
        }

        /// <summary>
        /// Get a marshalled UIAutomationPatternInfo struct for this Helper.
        /// </summary>
        public UIAutomationPatternInfo Data
        {
            get
            {
                if (!_built)
                {
                    Build();
                }
                return _data;
            }
        }

        /// <summary>
        /// Add a property to this pattern
        /// </summary>
        /// <param name="property"></param>
        public void AddProperty(UiaPropertyInfoHelper property)
        {
            _properties.Add(property);
        }

        /// <summary>
        /// Add a method to this pattern
        /// </summary>
        /// <param name="method"></param>
        public void AddMethod(UiaMethodInfoHelper method)
        {
            _methods.Add(method);
        }

        /// <summary>
        /// Add an event to this pattern
        /// </summary>
        /// <param name="eventHelper"></param>
        public void AddEvent(UiaEventInfoHelper eventHelper)
        {
            _events.Add(eventHelper);
        }

        private void Build()
        {
            // Basic data
            _data = new UIAutomationPatternInfo
                    {
                        pProgrammaticName = _programmaticName,
                        guid = _patternGuid,
                        clientInterfaceId = _clientInterfaceId,
                        providerInterfaceId = _providerInterfaceId,
                        pPatternHandler = _patternHandler,
                        cMethods = (uint) _methods.Count
                    };

            // Build the list of methods
            if (_data.cMethods > 0)
            {
                _data.pMethods = Marshal.AllocCoTaskMem((int) (_data.cMethods*Marshal.SizeOf(typeof (UIAutomationMethodInfo))));
                var methodPointer = _data.pMethods;
                for (var i = 0; i < _data.cMethods; ++i)
                {
                    Marshal.StructureToPtr(_methods[i].Data, methodPointer, false);
                    methodPointer = (IntPtr) (methodPointer.ToInt64() + Marshal.SizeOf(typeof (UIAutomationMethodInfo)));
                }
            }
            else
            {
                _data.pMethods = IntPtr.Zero;
            }

            // Build the list of properties
            _data.cProperties = (uint) _properties.Count;
            if (_data.cProperties > 0)
            {
                _data.pProperties = Marshal.AllocCoTaskMem((int) (_data.cProperties*Marshal.SizeOf(typeof (UIAutomationPropertyInfo))));
                var propertyPointer = _data.pProperties;
                for (var i = 0; i < _data.cProperties; ++i)
                {
                    Marshal.StructureToPtr(_properties[i].Data, propertyPointer, false);
                    propertyPointer = (IntPtr) (propertyPointer.ToInt64() + Marshal.SizeOf(typeof (UIAutomationPropertyInfo)));
                }
            }
            else
            {
                _data.pProperties = IntPtr.Zero;
            }

            // Build the list of events
            _data.cEvents = (uint) _events.Count;
            if (_data.cEvents > 0)
            {
                _data.pEvents = Marshal.AllocCoTaskMem((int) (_data.cEvents*Marshal.SizeOf(typeof (UIAutomationEventInfo))));
                var eventPointer = _data.pEvents;
                for (var i = 0; i < _data.cEvents; ++i)
                {
                    Marshal.StructureToPtr(_events[i].Data, eventPointer, false);
                    eventPointer = (IntPtr) (eventPointer.ToInt64() + Marshal.SizeOf(typeof (UIAutomationEventInfo)));
                }
            }
            else
            {
                _data.pEvents = IntPtr.Zero;
            }

            _built = true;
        }

        // Helper for the Registrar's pattern registration method, which has tricky marshalling
        public static void RegisterPattern(IUIAutomationRegistrar registrar,
                                           UiaPatternInfoHelper patternInfo,
                                           out int patternId,
                                           out int patternAvailableId,
                                           out int[] propertyIds,
                                           out int[] eventIds)
        {
            var patternData = patternInfo.Data;

            var pEventIds = IntPtr.Zero;
            var pPropertyIds = IntPtr.Zero;
            try
            {
                // Allocate out-param lists
                if (patternData.cProperties > 0)
                {
                    pPropertyIds = Marshal.AllocCoTaskMem((int) patternData.cProperties*Marshal.SizeOf(typeof (int)));
                }
                if (patternData.cEvents > 0)
                {
                    pEventIds = Marshal.AllocCoTaskMem((int) patternData.cEvents*Marshal.SizeOf(typeof (int)));
                }

                // Call register pattern
                registrar.RegisterPattern(
                    ref patternData,
                    out patternId,
                    out patternAvailableId,
                    patternData.cProperties,
                    pPropertyIds,
                    patternData.cEvents,
                    pEventIds);

                // Convert the lists of property IDs and event IDs into managed arrays
                propertyIds = new int[patternData.cProperties];
                var propertyPointer = pPropertyIds;
                for (var i = 0; i < patternData.cProperties; ++i)
                {
                    propertyIds[i] = (int) Marshal.PtrToStructure(propertyPointer, typeof (int));
                    propertyPointer = (IntPtr) (propertyPointer.ToInt64() + Marshal.SizeOf(typeof (int)));
                }

                eventIds = new int[patternData.cEvents];
                var eventPointer = pEventIds;
                for (var i = 0; i < patternData.cEvents; ++i)
                {
                    eventIds[i] = (int) Marshal.PtrToStructure(eventPointer, typeof (int));
                    eventPointer = (IntPtr) (eventPointer.ToInt64() + Marshal.SizeOf(typeof (int)));
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(pPropertyIds);
                Marshal.FreeCoTaskMem(pEventIds);
            }
        }
    }

    /// <summary>
    /// Helper class to marshal parameters for UIA custom method calls
    /// Corresponds to UIAutomationParameter
    /// </summary>
    public class UiaParameterHelper
    {
        private readonly UIAutomationType _uiaType;
        private readonly Type _clrType;
        private readonly IntPtr _marshalledData;
        private readonly bool _ownsData;
        private readonly bool _onClientSide;

        public UiaParameterHelper(UIAutomationType type)
        {
            _uiaType = type;
            _clrType = ClrTypeFromUiaType(type);
            _marshalledData = Marshal.AllocCoTaskMem(GetSizeOfMarshalledData());
            _ownsData = true;

            // It is a safe assumption that if we are initialized without incoming data,
            // we are on the client side.  If this changes, we can make this an explicit parameter.
            _onClientSide = true;
        }

        public UiaParameterHelper(UIAutomationType type, IntPtr marshalledData)
        {
            _uiaType = type;
            _clrType = ClrTypeFromUiaType(type);
            _marshalledData = marshalledData;
            _ownsData = false;

            // It is a safe assumption that if we are initialized with incoming data,
            // we are on the provider side.  If this changes, we can make this an explicit parameter.
            _onClientSide = false;
        }

        /// <summary>
        /// Clean up any marshalled data attached to this object
        /// </summary>
        ~UiaParameterHelper()
        {
            if (_ownsData)
            {
                var basicType = _uiaType & ~UIAutomationType.UIAutomationType_Out;
                if (basicType == UIAutomationType.UIAutomationType_String)
                {
                    var bstr = Marshal.ReadIntPtr(_marshalledData);
                    Marshal.FreeBSTR(bstr);
                }
                else if (basicType == UIAutomationType.UIAutomationType_Element)
                {
                    var elementAsIntPtr = Marshal.ReadIntPtr(_marshalledData);
                    Marshal.Release(elementAsIntPtr);
                }

                Marshal.FreeCoTaskMem(_marshalledData);
            }
        }

        /// <summary>
        /// Marshal the parameter's value to/from unmanaged code structures
        /// </summary>
        public object Value
        {
            get
            {
                // Get the type without the Out flag
                var basicType = _uiaType & ~UIAutomationType.UIAutomationType_Out;

                switch (basicType)
                {
                    case UIAutomationType.UIAutomationType_String:
                        // Strings are held as BSTRs
                        var bstr = Marshal.ReadIntPtr(_marshalledData);
                        return Marshal.PtrToStringBSTR(bstr);
                    case UIAutomationType.UIAutomationType_Bool:
                        return 0 != (int) Marshal.PtrToStructure(_marshalledData, typeof (int));
                    case UIAutomationType.UIAutomationType_Element:
                        // Elements need to be copied as COM pointers
                        var elementAsIntPtr = Marshal.ReadIntPtr(_marshalledData);
                        return Marshal.GetObjectForIUnknown(elementAsIntPtr);
                    default:
                        return Marshal.PtrToStructure(_marshalledData, GetClrType());
                }
            }
            set
            {
                // Get the type without the Out flag
                var basicType = _uiaType & ~UIAutomationType.UIAutomationType_Out;

                // Sanity check
                if (value.GetType() != GetClrType() &&
                    basicType != UIAutomationType.UIAutomationType_Bool &&
                    basicType != UIAutomationType.UIAutomationType_Element)
                {
                    throw new ArgumentException("Value is the wrong type for this parameter");
                }

                switch (basicType)
                {
                    case UIAutomationType.UIAutomationType_String:
                        // Strings are stored as BSTRs
                        var bstr = Marshal.StringToBSTR((string) value);
                        Marshal.WriteIntPtr(_marshalledData, bstr);
                        break;
                    case UIAutomationType.UIAutomationType_Bool:
                        // Bools are stored as integers in UIA custom parameters
                        var boolAsInt = ((bool) value) ? 1 : 0;
                        Marshal.StructureToPtr(boolAsInt, _marshalledData, true);
                        break;
                    case UIAutomationType.UIAutomationType_Element:
                        // Elements are stroed as COM pointers
                        var interfaceType = (_onClientSide) ?
                            typeof (IUIAutomationElement) :
                            typeof (IRawElementProviderSimple);
                        var elementAsIntPtr = Marshal.GetComInterfaceForObject(value, interfaceType);
                        Marshal.WriteIntPtr(_marshalledData, elementAsIntPtr);
                        break;
                    default:
                        Marshal.StructureToPtr(value, _marshalledData, true);
                        break;
                }
            }
        }

        /// <summary>
        /// Get the marshalled data for this helper
        /// </summary>
        public IntPtr Data
        {
            get { return _marshalledData; }
        }

        // Retrieve a UIAutomationParameter structure for this parameter
        public UIAutomationParameter ToUiaParam()
        {
            UIAutomationParameter uiaParam;
            uiaParam.type = _uiaType;
            uiaParam.pData = _marshalledData;
            return uiaParam;
        }

        // Get the UIA type for this parameter
        public UIAutomationType GetUiaType()
        {
            return _uiaType;
        }

        // Get the CLR type for this parameter
        public Type GetClrType()
        {
            return _clrType;
        }

        // Calculate how much marshalled data we'll need for this
        private int GetSizeOfMarshalledData()
        {
            var basicType = _uiaType & ~UIAutomationType.UIAutomationType_Out;
            if (basicType == UIAutomationType.UIAutomationType_String ||
                basicType == UIAutomationType.UIAutomationType_Element)
                return IntPtr.Size;
            return Marshal.SizeOf(_clrType);
        }

        // Compute a CLR type from its UIA type
        private Type ClrTypeFromUiaType(UIAutomationType uiaType)
        {
            // Mask off the out flag, which we don't care about.
            uiaType = (UIAutomationType) ((int) uiaType & (int) ~UIAutomationType.UIAutomationType_Out);

            switch (uiaType)
            {
                case UIAutomationType.UIAutomationType_Int:
                    return typeof (int);
                case UIAutomationType.UIAutomationType_Bool:
                    return typeof (int); // These are BOOL, not bool
                case UIAutomationType.UIAutomationType_String:
                    return typeof (string);
                case UIAutomationType.UIAutomationType_Double:
                    return typeof (double);
                case UIAutomationType.UIAutomationType_Element:
                    return (_onClientSide) ? typeof (IUIAutomationElement) : typeof (IRawElementProviderSimple);
                default:
                    throw new ArgumentException("Type not supported by UIAutomationType");
            }
        }
    }

    /// <summary>
    /// Helper class to assemble information about a parameter list
    /// </summary>
    public class UiaParameterListHelper
    {
        private readonly List<UiaParameterHelper> _uiaParams = new List<UiaParameterHelper>();

        private IntPtr _marshalledData;
        private bool _ownsData;

        // Construct a parameter list from a method info structure
        public UiaParameterListHelper(UiaMethodInfoHelper methodInfo)
        {
            foreach (var inParamType in methodInfo.InParamTypes)
            {
                _uiaParams.Add(new UiaParameterHelper(inParamType));
            }
            foreach (var outParamType in methodInfo.OutParamTypes)
            {
                _uiaParams.Add(new UiaParameterHelper(outParamType));
            }
        }

        // Construct a parameter list from a given in-memory structure
        public UiaParameterListHelper(IntPtr marshalledData, uint paramCount)
        {
            _marshalledData = marshalledData;
            _ownsData = false;

            // Construct the parameter list from the marshalled data
            var paramPointer = _marshalledData;
            for (uint i = 0; i < paramCount; ++i)
            {
                var uiaParam = (UIAutomationParameter) Marshal.PtrToStructure(
                    paramPointer, typeof (UIAutomationParameter));
                _uiaParams.Add(new UiaParameterHelper(uiaParam.type, uiaParam.pData));

                paramPointer = (IntPtr) (paramPointer.ToInt64() + Marshal.SizeOf(typeof (UIAutomationParameter)));
            }
        }

        // Free any marshalled data associated with this structure
        ~UiaParameterListHelper()
        {
            if (_ownsData)
            {
                Marshal.FreeCoTaskMem(_marshalledData);
            }
        }

        // Get a pointer to the whole parameter list marshalled into a block of memory
        public IntPtr Data
        {
            get
            {
                if (_marshalledData == IntPtr.Zero)
                {
                    Build();
                }
                return _marshalledData;
            }
        }

        /// <summary>
        /// The count of parameters in this list
        /// </summary>
        public uint Count
        {
            get { return (uint) _uiaParams.Count; }
        }

        // Helper method to initialize the incoming parameters list.
        public void Initialize(params object[] inParams)
        {
            for (var i = 0; i < inParams.Length; ++i)
            {
                this[i] = inParams[i];
            }
        }

        /// <summary>
        /// The value of the specified parameter in this list
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object this[int i]
        {
            get { return _uiaParams[i].Value; }
            set { _uiaParams[i].Value = value; }
        }

        private void Build()
        {
            // Allocate the parameter block
            _marshalledData = Marshal.AllocCoTaskMem(_uiaParams.Count*Marshal.SizeOf(typeof (UIAutomationParameter)));

            // Write the parameter data to the marshalled array
            var paramPointer = _marshalledData;
            foreach (UiaParameterHelper paramHelper in _uiaParams)
            {
                Marshal.StructureToPtr(paramHelper.ToUiaParam(), paramPointer, true);
                paramPointer = (IntPtr) (paramPointer.ToInt64() + Marshal.SizeOf(typeof (UIAutomationParameter)));
            }

            _ownsData = true;
        }
    }
}