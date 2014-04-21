using System;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;
using UIAutomationClient;
using IRawElementProviderSimple = Interop.UIAutomationCore.IRawElementProviderSimple;

namespace ManagedUiaCustomizationCore
{
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
}