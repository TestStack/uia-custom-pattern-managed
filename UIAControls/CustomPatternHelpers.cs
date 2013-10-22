using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;

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
        public UiaParameterDescription(string name, UIAutomationType type)
        {
            this.name = name;
            this.uiaType = type;
        }

        public string Name
        {
            get { return this.name; }
        }

        public UIAutomationType UiaType
        {
            get { return this.uiaType; }
        }

        private string name;
        private UIAutomationType uiaType;
    }

    /// <summary>
    /// Helper class to assemble information about a custom method
    /// Corresponds to UIAutomationMethodInfo.
    /// </summary>
    public class UiaMethodInfoHelper
    {
        public UiaMethodInfoHelper(string programmaticName, bool doSetFocus)
        {
            this.programmaticName = programmaticName;
            this.doSetFocus = doSetFocus;
        }

        public UiaMethodInfoHelper(string programmaticName, bool doSetFocus, UiaParameterDescription [] uiaParams)
        {
            this.programmaticName = programmaticName;
            this.doSetFocus = doSetFocus;

            foreach (UiaParameterDescription param in uiaParams)
            {
                this.AddParameter(param.Name, param.UiaType);
            }
        }

        ~UiaMethodInfoHelper()
        {
            foreach (IntPtr marshalledName in this.inParamNames)
            {
                Marshal.FreeCoTaskMem(marshalledName);
            }
            foreach (IntPtr marshalledName in this.outParamNames)
            {
                Marshal.FreeCoTaskMem(marshalledName);
            }

            Marshal.FreeCoTaskMem(this.data.pParameterNames);
            Marshal.FreeCoTaskMem(this.data.pParameterTypes);
        }

        /// <summary>
        /// Get a marshalled UIAutomationMethodInfo struct for this Helper.
        /// </summary>
        public UIAutomationMethodInfo Data
        {
            get
            {
                if (!this.built)
                {
                    Build();
                }
                return this.data;
            }
        }

        /// <summary>
        /// The array of in-parameter types.
        /// </summary>
        public UIAutomationType [] InParamTypes
        {
            get { return this.inParamTypes.ToArray(); }
        }

        /// <summary>
        /// The array of out-parameter types.
        /// </summary>
        public UIAutomationType [] OutParamTypes
        {
            get { return this.outParamTypes.ToArray(); }
        }

        /// <summary>
        /// The index of this method.  
        /// In a UIA custom pattern, every method (and pattern property)
        /// has an assigned index.
        /// </summary>
        public uint Index
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;
            }
        }

        /// <summary>
        /// Add a parameter to the list of parameters for this method.
        /// </summary>
        public void AddParameter(string name, UIAutomationType type)
        {
            IntPtr marshalledName = Marshal.StringToCoTaskMemUni(name);
            if ((type & UIAutomationType.UIAutomationType_Out) == 0)
            {
                this.inParamNames.Add(marshalledName);
                this.inParamTypes.Add(type);
            }
            else
            {
                this.outParamNames.Add(marshalledName);
                this.outParamTypes.Add(type);
            }
        }

        /// <summary>
        /// Marshal our data to the UIAutomationMethodInfo struct.
        /// </summary>
        void Build()
        {
            // Copy basic data
            this.data = new UIAutomationMethodInfo();
            this.data.pProgrammaticName = this.programmaticName;
            this.data.doSetFocus = this.doSetFocus ? 1 : 0;
            this.data.cInParameters = (uint)this.inParamNames.Count;
            this.data.cOutParameters = (uint)this.outParamNames.Count;
            uint cTotalParameters = this.data.cInParameters + this.data.cOutParameters;

            // Allocate parameter lists and populate them
            if (cTotalParameters > 0)
            {
                this.data.pParameterNames = Marshal.AllocCoTaskMem((int)(cTotalParameters * Marshal.SizeOf(typeof(IntPtr))));
                this.data.pParameterTypes = Marshal.AllocCoTaskMem((int)(cTotalParameters * Marshal.SizeOf(typeof(Int32))));

                IntPtr namePointer = this.data.pParameterNames;
                IntPtr typePointer = this.data.pParameterTypes;
                for (int i = 0; i < this.data.cInParameters; ++i)
                {
                    Marshal.WriteIntPtr(namePointer, this.inParamNames[i]);
                    namePointer = (IntPtr)(namePointer.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
                    Marshal.WriteInt32(typePointer, (int)this.inParamTypes[i]);
                    typePointer = (IntPtr)(typePointer.ToInt64() + Marshal.SizeOf(typeof(Int32)));
                }

                for (int i = 0; i < this.data.cOutParameters; ++i)
                {
                    Marshal.WriteIntPtr(namePointer, this.outParamNames[i]);
                    namePointer = (IntPtr)(namePointer.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
                    Marshal.WriteInt32(typePointer, (int)this.outParamTypes[i]);
                    typePointer = (IntPtr)(typePointer.ToInt64() + Marshal.SizeOf(typeof(Int32)));
                }
            }
            else
            {
                this.data.pParameterNames = IntPtr.Zero;
                this.data.pParameterTypes = IntPtr.Zero;
            }

            this.built = true;
        }

        private string programmaticName;
        private bool doSetFocus;
        private List<IntPtr> inParamNames = new List<IntPtr>();
        private List<UIAutomationType> inParamTypes = new List<UIAutomationType>();
        private List<IntPtr> outParamNames = new List<IntPtr>();
        private List<UIAutomationType> outParamTypes = new List<UIAutomationType>();
        private uint index;
        private bool built;
        private UIAutomationMethodInfo data;
    }

    /// <summary>
    /// Helper class to gather data about a custom property
    /// Corresponds to UIAutomationPropertyInfo
    /// </summary>
    public class UiaPropertyInfoHelper
    {
        public UiaPropertyInfoHelper(Guid propertyGuid, string programmaticName, UIAutomationType propertyType)
        {
            this.programmaticName = programmaticName;
            this.propertyGuid = propertyGuid;
            this.propertyType = propertyType;
        }

        /// <summary>
        /// Get a marshalled UIAutomationPropertyInfo struct for this Helper.
        /// </summary>
        public UIAutomationPropertyInfo Data
        {
            get
            {
                if (!this.built)
                {
                    Build();
                }
                return this.data;
            }
        }

        /// <summary>
        /// The UIA type of this property
        /// </summary>
        public UIAutomationType UiaType
        {
            get
            {
                return this.propertyType;
            }
        }

        /// <summary>
        /// The unique identifier for this property
        /// </summary>
        public Guid Guid
        {
            get
            {
                return this.propertyGuid;
            }
        }

        /// <summary>
        /// The index of this property, when it is used as part of a pattern
        /// </summary>
        public uint Index
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;
            }
        }

        /// <summary>
        /// The property ID of this property, assigned after registration
        /// </summary>
        public int PropertyId
        {
            get 
            { 
                return this.propertyId; 
            }
            set 
            { 
                this.propertyId = value; 
            }
        }

        void Build()
        {
            this.data = new UIAutomationPropertyInfo();
            this.data.pProgrammaticName = this.programmaticName;
            this.data.guid = this.propertyGuid;
            this.data.type = this.propertyType;
            this.built = true;
        }

        private Guid propertyGuid;
        private string programmaticName;
        private UIAutomationType propertyType;
        private uint index;
        private int propertyId;
        private bool built;
        private UIAutomationPropertyInfo data;
    }

    /// <summary>
    /// Helper class to gather data about a custom event
    /// Corresponds to UIAutomationEventInfo.
    /// </summary>
    public class UiaEventInfoHelper
    {
        public UiaEventInfoHelper(Guid eventGuid, string programmaticName)
        {
            this.programmaticName = programmaticName;
            this.eventGuid = eventGuid;
        }

        /// <summary>
        /// Get a marshalled UIAutomationEventInfo struct for this Helper.
        /// </summary>
        public UIAutomationEventInfo Data
        {
            get
            {
                if (!this.built)
                {
                    Build();
                }
                return this.data;
            }
        }

        /// <summary>
        /// The unique identifier of this event
        /// </summary>
        public Guid Guid
        {
            get
            {
                return this.eventGuid;
            }
        }

        /// <summary>
        /// The event ID of this event, assigned after registration
        /// </summary>
        public int EventId
        {
            get
            {
                return this.eventId;
            }
            set
            {
                this.eventId = value;
            }
        }

        void Build()
        {
            this.data = new UIAutomationEventInfo();
            this.data.pProgrammaticName = this.programmaticName;
            this.data.guid = this.eventGuid;
            this.built = true;
        }

        private Guid eventGuid;
        private string programmaticName;
        private int eventId;
        private bool built;
        private UIAutomationEventInfo data;
    }

    /// <summary>
    /// Helper class to assemble information about a custom pattern.
    /// Corresponds to UIAutomationPatternInfo
    /// </summary>
    public class UiaPatternInfoHelper
    {
        public UiaPatternInfoHelper(
            Guid patternGuid, 
            string programmaticName,
            Guid clientInterfaceId,
            Guid providerInterfaceId,
            IUIAutomationPatternHandler patternHandler)
        {
            this.programmaticName = programmaticName;
            this.patternGuid = patternGuid;
            this.clientInterfaceId = clientInterfaceId;
            this.providerInterfaceId = providerInterfaceId;
            this.patternHandler = patternHandler;
        }

        ~UiaPatternInfoHelper()
        {
            Marshal.FreeCoTaskMem(this.data.pMethods);
            Marshal.FreeCoTaskMem(this.data.pEvents);
            Marshal.FreeCoTaskMem(this.data.pProperties);
        }

        /// <summary>
        /// Get a marshalled UIAutomationPatternInfo struct for this Helper.
        /// </summary>
        public UIAutomationPatternInfo Data
        {
            get
            {
                if (!this.built)
                {
                    Build();
                }
                return this.data;
            }
        }

        /// <summary>
        /// Add a property to this pattern
        /// </summary>
        /// <param name="property"></param>
        public void AddProperty(UiaPropertyInfoHelper property)
        {
            this.properties.Add(property);
        }

        /// <summary>
        /// Add a method to this pattern
        /// </summary>
        /// <param name="method"></param>
        public void AddMethod(UiaMethodInfoHelper method)
        {
            this.methods.Add(method);
        }

        /// <summary>
        /// Add an event to this pattern
        /// </summary>
        /// <param name="eventHelper"></param>
        public void AddEvent(UiaEventInfoHelper eventHelper)
        {
            this.events.Add(eventHelper);
        }

        private void Build()
        {
            // Basic data
            this.data = new UIAutomationPatternInfo();
            this.data.pProgrammaticName = this.programmaticName;
            this.data.guid = this.patternGuid;
            this.data.clientInterfaceId = this.clientInterfaceId;
            this.data.providerInterfaceId = this.providerInterfaceId;
            this.data.pPatternHandler = this.patternHandler;

            // Build the list of methods
            this.data.cMethods = (uint)this.methods.Count;
            if (this.data.cMethods > 0)
            {
                this.data.pMethods = Marshal.AllocCoTaskMem((int)(this.data.cMethods * Marshal.SizeOf(typeof(UIAutomationMethodInfo))));
                IntPtr methodPointer = this.data.pMethods;
                for (int i = 0; i < this.data.cMethods; ++i)
                {
                    Marshal.StructureToPtr(this.methods[i].Data, methodPointer, false);
                    methodPointer = (IntPtr)(methodPointer.ToInt64() + Marshal.SizeOf(typeof(UIAutomationMethodInfo)));
                }
            }
            else
            {
                this.data.pMethods = IntPtr.Zero;
            }

            // Build the list of properties
            this.data.cProperties = (uint)this.properties.Count;
            if (this.data.cProperties > 0)
            {
                this.data.pProperties = Marshal.AllocCoTaskMem((int)(this.data.cProperties * Marshal.SizeOf(typeof(UIAutomationPropertyInfo))));
                IntPtr propertyPointer = this.data.pProperties;
                for (int i = 0; i < this.data.cProperties; ++i)
                {
                    Marshal.StructureToPtr(this.properties[i].Data, propertyPointer, false);
                    propertyPointer = (IntPtr)(propertyPointer.ToInt64() + Marshal.SizeOf(typeof(UIAutomationPropertyInfo)));
                }
            }
            else
            {
                this.data.pProperties = IntPtr.Zero;
            }

            // Build the list of events
            this.data.cEvents = (uint)this.events.Count;
            if (this.data.cEvents > 0)
            {
                this.data.pEvents = Marshal.AllocCoTaskMem((int)(this.data.cEvents * Marshal.SizeOf(typeof(UIAutomationEventInfo))));
                IntPtr eventPointer = this.data.pEvents;
                for (int i = 0; i < this.data.cEvents; ++i)
                {
                    Marshal.StructureToPtr(this.events[i].Data, eventPointer, false);
                    eventPointer = (IntPtr)(eventPointer.ToInt64() + Marshal.SizeOf(typeof(UIAutomationEventInfo)));
                }
            }
            else
            {
                this.data.pEvents = IntPtr.Zero;
            }

            this.built = true;
        }

        // Helper for the Registrar's pattern registration method, which has tricky marshalling
        public static void RegisterPattern(IUIAutomationRegistrar registrar, 
                                          UiaPatternInfoHelper patternInfo,
                                          out int patternId,
                                          out int patternAvailableId,
                                          out int [] propertyIds,
                                          out int [] eventIds)
        {
            Interop.UIAutomationCore.UIAutomationPatternInfo patternData = patternInfo.Data;

            IntPtr pEventIds = IntPtr.Zero;
            IntPtr pPropertyIds = IntPtr.Zero;
            try
            {
                // Allocate out-param lists
                if (patternData.cProperties > 0)
                {
                    pPropertyIds = Marshal.AllocCoTaskMem((int)patternData.cProperties * Marshal.SizeOf(typeof(int)));
                }
                if (patternData.cEvents > 0)
                {
                    pEventIds = Marshal.AllocCoTaskMem((int)patternData.cEvents * Marshal.SizeOf(typeof(int)));
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
                IntPtr propertyPointer = pPropertyIds;
                for (int i = 0; i < patternData.cProperties; ++i)
                {
                    propertyIds[i] = (int)Marshal.PtrToStructure(propertyPointer, typeof(int));
                    propertyPointer = (IntPtr)(propertyPointer.ToInt64() + Marshal.SizeOf(typeof(int)));
                }

                eventIds = new int[patternData.cEvents];
                IntPtr eventPointer = pEventIds;
                for (int i = 0; i < patternData.cEvents; ++i)
                {
                    eventIds[i] = (int)Marshal.PtrToStructure(eventPointer, typeof(int));
                    eventPointer = (IntPtr)(eventPointer.ToInt64() + Marshal.SizeOf(typeof(int)));
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(pPropertyIds);
                Marshal.FreeCoTaskMem(pEventIds);
            }
           
        }

        private Guid patternGuid;
        private Guid clientInterfaceId;
        private Guid providerInterfaceId;
        private string programmaticName;
        private IUIAutomationPatternHandler patternHandler;
        private List<UiaMethodInfoHelper> methods = new List<UiaMethodInfoHelper>();
        private List<UiaPropertyInfoHelper> properties = new List<UiaPropertyInfoHelper>();
        private List<UiaEventInfoHelper> events = new List<UiaEventInfoHelper>();

        private bool built;
        private UIAutomationPatternInfo data;
    }

    /// <summary>
    /// Helper class to marshal parameters for UIA custom method calls
    /// Corresponds to UIAutomationParameter
    /// </summary>
    public class UiaParameterHelper
    {
        public UiaParameterHelper(UIAutomationType type)
        {
            this.uiaType = type;
            this.clrType = ClrTypeFromUiaType(type);
            this.marshalledData = Marshal.AllocCoTaskMem(GetSizeOfMarshalledData());
            this.ownsData = true;

            // It is a safe assumption that if we are initialized without incoming data,
            // we are on the client side.  If this changes, we can make this an explicit parameter.
            this.onClientSide = true;
        }

        public UiaParameterHelper(UIAutomationType type, IntPtr marshalledData)
        {
            this.uiaType = type;
            this.clrType = ClrTypeFromUiaType(type);
            this.marshalledData = marshalledData;
            this.ownsData = false;

            // It is a safe assumption that if we are initialized with incoming data,
            // we are on the provider side.  If this changes, we can make this an explicit parameter.
            this.onClientSide = false;
        }

        /// <summary>
        /// Clean up any marshalled data attached to this object
        /// </summary>
        ~UiaParameterHelper()
        {
            if (this.ownsData)
            {
                UIAutomationType basicType = this.uiaType & ~UIAutomationType.UIAutomationType_Out;
                if (basicType == UIAutomationType.UIAutomationType_String)
                {
                    IntPtr bstr = Marshal.ReadIntPtr(this.marshalledData);
                    Marshal.FreeBSTR(bstr);
                }
                else if (basicType == UIAutomationType.UIAutomationType_Element)
                {
                    IntPtr elementAsIntPtr = Marshal.ReadIntPtr(this.marshalledData);
                    Marshal.Release(elementAsIntPtr);
                }

                Marshal.FreeCoTaskMem(this.marshalledData);
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
                UIAutomationType basicType = this.uiaType & ~UIAutomationType.UIAutomationType_Out;

                if (basicType == UIAutomationType.UIAutomationType_String)
                {
                    // Strings are held as BSTRs
                    IntPtr bstr = Marshal.ReadIntPtr(this.marshalledData);
                    return Marshal.PtrToStringBSTR(bstr);
                }
                else if (basicType == UIAutomationType.UIAutomationType_Bool)
                {
                    // Bools are stored as integers in UIA custom parameters
                    return 0 != (int)Marshal.PtrToStructure(this.marshalledData, typeof(int));
                }
                else if (basicType == UIAutomationType.UIAutomationType_Element)
                {
                    // Elements need to be copied as COM pointers
                    IntPtr elementAsIntPtr = Marshal.ReadIntPtr(this.marshalledData);
                    return Marshal.GetObjectForIUnknown(elementAsIntPtr);
                }
                else
                {
                    return Marshal.PtrToStructure(this.marshalledData, GetClrType());
                }
            }
            set
            {
                // Get the type without the Out flag
                UIAutomationType basicType = this.uiaType & ~UIAutomationType.UIAutomationType_Out;

                // Sanity check
                if (value.GetType() != GetClrType() &&
                    basicType != UIAutomationType.UIAutomationType_Bool &&
                    basicType != UIAutomationType.UIAutomationType_Element)
                {
                    throw new ArgumentException("Value is the wrong type for this parameter");
                }

                if (basicType == UIAutomationType.UIAutomationType_String)
                {
                    // Strings are stored as BSTRs
                    IntPtr bstr = Marshal.StringToBSTR((string)value);
                    Marshal.WriteIntPtr(this.marshalledData, bstr);
                }
                else if (basicType == UIAutomationType.UIAutomationType_Bool)
                {
                    // Bools are stored as integers in UIA custom parameters
                    int boolAsInt = ((bool)value) ? 1 : 0;
                    Marshal.StructureToPtr(boolAsInt, this.marshalledData, true);
                }
                else if (basicType == UIAutomationType.UIAutomationType_Element)
                {
                    // Elements are stroed as COM pointers
                    Type interfaceType = (this.onClientSide) ?
                        typeof(Interop.UIAutomationClient.IUIAutomationElement) :
                        typeof(IRawElementProviderSimple);
                    IntPtr elementAsIntPtr = Marshal.GetComInterfaceForObject(value, interfaceType);
                    Marshal.WriteIntPtr(this.marshalledData, elementAsIntPtr);
                }
                else
                {
                    Marshal.StructureToPtr(value, this.marshalledData, true);
                }
            }
        }

        /// <summary>
        /// Get the marshalled data for this helper
        /// </summary>
        public IntPtr Data
        {
            get
            {
                return this.marshalledData;
            }
        }

        // Retrieve a UIAutomationParameter structure for this parameter
        public UIAutomationParameter ToUiaParam()
        {
            UIAutomationParameter uiaParam;
            uiaParam.type = this.uiaType;
            uiaParam.pData = this.marshalledData;
            return uiaParam;
        }

        // Get the UIA type for this parameter
        public UIAutomationType GetUiaType()
        {
            return this.uiaType;
        }

        // Get the CLR type for this parameter
        public Type GetClrType()
        {
            return this.clrType;
        }

        // Calculate how much marshalled data we'll need for this
        private int GetSizeOfMarshalledData()
        {
            UIAutomationType basicType = this.uiaType & ~UIAutomationType.UIAutomationType_Out;
            if (basicType == UIAutomationType.UIAutomationType_String ||
                basicType == UIAutomationType.UIAutomationType_Element)
            {
                return IntPtr.Size;
            }
            else
            {
                return Marshal.SizeOf(this.clrType);
            }
        }

        // Compute a CLR type from its UIA type
        private Type ClrTypeFromUiaType(UIAutomationType uiaType)
        {
            // Mask off the out flag, which we don't care about.
            uiaType = (UIAutomationType)((int)uiaType & (int)~UIAutomationType.UIAutomationType_Out);

            switch (uiaType)
            {
                case UIAutomationType.UIAutomationType_Int: return typeof(int);
                case UIAutomationType.UIAutomationType_Bool: return typeof(int); // These are BOOL, not bool
                case UIAutomationType.UIAutomationType_String: return typeof(string);
                case UIAutomationType.UIAutomationType_Double: return typeof(double);
                case UIAutomationType.UIAutomationType_Element: 
                    return (this.onClientSide) ? typeof(Interop.UIAutomationClient.IUIAutomationElement) : typeof(IRawElementProviderSimple);
                default: throw new ArgumentException("Type not supported by UIAutomationType");
            }
        }

        private UIAutomationType uiaType;
        private Type clrType;
        private IntPtr marshalledData;
        private bool ownsData;
        private bool onClientSide;
    }

    /// <summary>
    /// Helper class to assemble information about a parameter list
    /// </summary>
    public class UiaParameterListHelper
    {
        // Construct a parameter list from a method info structure
        public UiaParameterListHelper(UiaMethodInfoHelper methodInfo)
        {
            foreach (UIAutomationType inParamType in methodInfo.InParamTypes)
            {
                this.uiaParams.Add(new UiaParameterHelper(inParamType));
            }
            foreach (UIAutomationType outParamType in methodInfo.OutParamTypes)
            {
                this.uiaParams.Add(new UiaParameterHelper(outParamType));
            }
        }

        // Construct a parameter list from a given in-memory structure
        public UiaParameterListHelper(IntPtr marshalledData, uint paramCount)
        {
            this.marshalledData = marshalledData;
            this.ownsData = false;

            // Construct the parameter list from the marshalled data
            IntPtr paramPointer = this.marshalledData;
            for (uint i = 0; i < paramCount; ++i)
            {
                UIAutomationParameter uiaParam = (UIAutomationParameter)Marshal.PtrToStructure(
                    paramPointer, typeof(UIAutomationParameter));
                this.uiaParams.Add(new UiaParameterHelper(uiaParam.type, uiaParam.pData));

                paramPointer = (IntPtr)(paramPointer.ToInt64() + Marshal.SizeOf(typeof(UIAutomationParameter)));
            }
        }

        // Free any marshalled data associated with this structure
        ~UiaParameterListHelper()
        {
            if (this.ownsData)
            {
                Marshal.FreeCoTaskMem(this.marshalledData);
            }
        }

        // Get a pointer to the whole parameter list marshalled into a block of memory
        public IntPtr Data
        {
            get
            {
                if (this.marshalledData == IntPtr.Zero)
                {
                    Build();
                }
                return this.marshalledData;
            }
        }

        /// <summary>
        /// The count of parameters in this list
        /// </summary>
        public uint Count
        {
            get
            {
                return (uint)this.uiaParams.Count;
            }
        }

        // Helper method to initialize the incoming parameters list.
        public void Initialize(params object[] inParams)
        {
            for (int i = 0; i < inParams.Length; ++i)
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
            get { return this.uiaParams[i].Value; }
            set { this.uiaParams[i].Value = value; }
        }

        private void Build()
        {
            // Allocate the parameter block
            this.marshalledData = Marshal.AllocCoTaskMem((int)(this.uiaParams.Count * Marshal.SizeOf(typeof(UIAutomationParameter))));

            // Write the parameter data to the marshalled array
            IntPtr paramPointer = this.marshalledData;
            for (int i = 0; i < this.uiaParams.Count; ++i)
            {
                Marshal.StructureToPtr(this.uiaParams[i].ToUiaParam(), paramPointer, true);
                paramPointer = (IntPtr)(paramPointer.ToInt64() + Marshal.SizeOf(typeof(UIAutomationParameter)));
            }

            this.ownsData = true;
        }

        private List<UiaParameterHelper> uiaParams = new List<UiaParameterHelper>();

        private IntPtr marshalledData;
        private bool ownsData;
    }
}
