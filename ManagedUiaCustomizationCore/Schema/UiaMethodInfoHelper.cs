using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;

namespace ManagedUiaCustomizationCore
{
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

        public UiaMethodInfoHelper(string programmaticName, bool doSetFocus, IEnumerable<UiaParameterDescription> uiaParams)
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
}