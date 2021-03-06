﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ManagedUiaCustomizationCore;
using UIAutomationClient;
using IRawElementProviderSimple = Interop.UIAutomationCore.IRawElementProviderSimple;
using ProviderOptions = Interop.UIAutomationCore.ProviderOptions;

namespace UIAControls
{
    /// <summary>
    /// A basic implementation of IRawElementSimpleProvider.
    /// Many customizations can be done by overriding virtual methods of this class,
    /// rather than having to implement the whole interface.
    /// </summary>
    [ComVisible(true)]
    public abstract class BaseSimpleProvider : IRawElementProviderSimple
    {
        private readonly Dictionary<int, object> _staticProps = new Dictionary<int, object>();
        
        public virtual object GetPatternProvider(int patternId)
        {
            return null;
        }

        public virtual object GetPropertyValue(int propertyId)
        {
            // Check the static props list first
            if (_staticProps.ContainsKey(propertyId))
            {
                return _staticProps[propertyId];
            }

            // Switching construct to go get the right property from a virtual method.
            if (propertyId == UIA_PropertyIds.UIA_NamePropertyId)
            {
                return GetName();
            }

            // Add further cases here to support more properties.
            // Do note that it may be more efficient to handle static properties
            // by adding them to the static props list instead of using methods.
            
            return null;
        }

        public IRawElementProviderSimple HostRawElementProvider
        {
            get
            {
                var hwnd = GetWindowHandle();
                if (hwnd == IntPtr.Zero) return null;
                IRawElementProviderSimple hostProvider;
                NativeMethods.UiaHostProviderFromHwnd(GetWindowHandle(), out hostProvider);
                return hostProvider;
            }
        }

        public virtual ProviderOptions ProviderOptions
        {
            get { return ProviderOptions.ProviderOptions_ServerSideProvider; }
        }

        // Get the window handle for a provider that is a full HWND
        protected virtual IntPtr GetWindowHandle()
        {
            return IntPtr.Zero;
        }

        // Get the localized name for this control
        protected virtual string GetName()
        {
            return null;
        }

        protected void AddStaticProperty(int propertyId, object value)
        {
            _staticProps.Add(propertyId, value);
        }
    }
}
