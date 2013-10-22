using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.UIAutomationCore;

namespace UIAControls
{
    /// <summary>
    /// Base class for defining a custom schema.
    /// Responsible for defining the minimum info for a custom schema and 
    /// registering it with UI Automation.
    /// 
    /// This class is not required by UIA and doesn't correspond to anything in UIA;
    /// it's a personal preference about the right way to represent what is similar
    /// between various schemas and what varies.  
    /// </summary>
    abstract public class CustomPatternSchemaBase
    {
        // The abstract properties define the minimal data needed to express
        // a custom pattern.

        /// <summary>
        /// The list of properties for this pattern.
        /// </summary>
        abstract public UiaPropertyInfoHelper[] Properties { get; }

        /// <summary>
        /// The list of methods for this pattern.
        /// </summary>
        abstract public UiaMethodInfoHelper[] Methods { get; }

        /// <summary>
        /// The list of events for this pattern.
        /// </summary>
        abstract public UiaEventInfoHelper[] Events { get; }

        /// <summary>
        ///  The unique ID for this pattern.
        /// </summary>
        abstract public Guid PatternGuid { get; }

        /// <summary>
        /// The interface ID for the COM interface for this pattern on the client side.
        /// </summary>
        abstract public Guid PatternClientGuid { get; }

        /// <summary>
        /// The interface ID for the COM interface for this pattern on the provider side.
        /// </summary>
        abstract public Guid PatternProviderGuid { get; }

        /// <summary>
        /// The programmatic name for this pattern.
        /// </summary>
        abstract public string PatternName { get; }

        /// <summary>
        /// An object that implements IUIAutomationPatternHandler to handle
        /// dispatching and client-pattern creation for this pattern
        /// </summary>
        abstract public IUIAutomationPatternHandler Handler { get; }

        /// <summary>
        /// The assigned ID for this pattern.
        /// </summary>
        public int PatternId
        {
            get { return patternId; }
        }

        /// <summary>
        /// The assigned ID for the IsXxxxPatternAvailable property.
        /// </summary>
        public int PatternAvailablePropertyId
        {
            get { return patternAvailablePropertyId; }
        }

        /// <summary>
        /// Helper method to register this pattern.
        /// </summary>
        public void Register()
        {
            if (!this.registered)
            {
                // Get our pointer to the registrar
                Interop.UIAutomationCore.IUIAutomationRegistrar registrar =
                    new Interop.UIAutomationCore.CUIAutomationRegistrarClass();

                // Set up the pattern struct
                UiaPatternInfoHelper patternInfo = new UiaPatternInfoHelper(
                    this.PatternGuid,
                    this.PatternName,
                    this.PatternClientGuid,
                    this.PatternProviderGuid,
                    this.Handler
                    );

                // Populate it with properties and methods
                uint index = 0;
                foreach (UiaPropertyInfoHelper propertyInfo in this.Properties)
                {
                    patternInfo.AddProperty(propertyInfo);
                    propertyInfo.Index = index++;
                }
                foreach (UiaMethodInfoHelper methodInfo in this.Methods)
                {
                    patternInfo.AddMethod(methodInfo);
                    methodInfo.Index = index++;
                }

                // Add the events, too, although they are not indexed
                foreach (UiaEventInfoHelper eventInfo in this.Events)
                {
                    patternInfo.AddEvent(eventInfo);
                }

                // Register the pattern
                int[] propertyIds;
                int[] eventIds;
                UiaPatternInfoHelper.RegisterPattern(registrar,
                    patternInfo,
                    out this.patternId,
                    out this.patternAvailablePropertyId,
                    out propertyIds,
                    out eventIds);

                // Write the property IDs back
                for (int i = 0; i < propertyIds.Length; ++i)
                {
                    this.Properties[i].PropertyId = propertyIds[i];
                }
                for (int i = 0; i < eventIds.Length; ++i)
                {
                    this.Events[i].EventId = eventIds[i];
                }

                this.registered = true;
            }
        }

        int patternId;
        int patternAvailablePropertyId;
        bool registered;
    }

    /// <summary>
    /// Base class for a custom pattern's client instance.
    /// Responsible for hiding some of the details of marshalling client-side custom calls;
    /// this is mostly syntactic sugar to keep the custom pattern instance neat.
    /// </summary>
    public class CustomClientInstanceBase
    {
        protected CustomClientInstanceBase(IUIAutomationPatternInstance patternInstance)
        {
            this.patternInstance = patternInstance;
        }

        // Get a current property value for this custom property
        protected object GetCurrentPropertyValue(UiaPropertyInfoHelper propInfo)
        {
            UiaParameterHelper param = new UiaParameterHelper(propInfo.UiaType);
            this.patternInstance.GetProperty(propInfo.Index, 0 /* fCached */, param.GetUiaType(), param.Data);
            return param.Value;
        }

        // Get a current property value by calling a method, rather than by using GetProperty
        protected object GetCurrentPropertyValueViaMethod(UiaMethodInfoHelper methodInfo)
        {
            // Create and init a parameter list
            UiaParameterListHelper paramList = new UiaParameterListHelper(methodInfo);
            System.Diagnostics.Debug.Assert(paramList.Count == 1);

            // Call through
            this.patternInstance.CallMethod(methodInfo.Index, paramList.Data, paramList.Count);

            // Return the out-parameter
            return paramList[0];
        }

        // Get a cached property value for this custom property
        protected object GetCachedPropertyValue(UiaPropertyInfoHelper propInfo)
        {
            UiaParameterHelper param = new UiaParameterHelper(propInfo.UiaType);
            this.patternInstance.GetProperty(propInfo.Index, 1 /* fCached */, param.GetUiaType(), param.Data);
            return param.Value;
        }

        // Call a pattern instance method with this parameter list
        protected void CallMethod(UiaMethodInfoHelper methodInfo, UiaParameterListHelper paramList)
        {
            this.patternInstance.CallMethod(methodInfo.Index, paramList.Data, paramList.Count);
        }

        // Call a pattern instance method with only in-params
        protected void CallMethod(UiaMethodInfoHelper methodInfo, params object[] methodParams)
        {
            // Create and init a parameter list
            UiaParameterListHelper paramList = new UiaParameterListHelper(methodInfo);
            paramList.Initialize(methodParams);

            // Call through
            this.patternInstance.CallMethod(methodInfo.Index, paramList.Data, paramList.Count);
        }

        protected Interop.UIAutomationCore.IUIAutomationPatternInstance patternInstance;

    }
}
