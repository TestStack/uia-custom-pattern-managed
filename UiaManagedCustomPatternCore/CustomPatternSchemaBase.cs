using System;
using System.Diagnostics;
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
    public abstract class CustomPatternSchemaBase
    {
        private int _patternId;
        private int _patternAvailablePropertyId;
        private bool _registered;

        // The abstract properties define the minimal data needed to express
        // a custom pattern.

        /// <summary>
        /// The list of properties for this pattern.
        /// </summary>
        public abstract UiaPropertyInfoHelper[] Properties { get; }

        /// <summary>
        /// The list of methods for this pattern.
        /// </summary>
        public abstract UiaMethodInfoHelper[] Methods { get; }

        /// <summary>
        /// The list of events for this pattern.
        /// </summary>
        public abstract UiaEventInfoHelper[] Events { get; }

        /// <summary>
        ///  The unique ID for this pattern.
        /// </summary>
        public abstract Guid PatternGuid { get; }

        /// <summary>
        /// The interface ID for the COM interface for this pattern on the client side.
        /// </summary>
        public abstract Guid PatternClientGuid { get; }

        /// <summary>
        /// The interface ID for the COM interface for this pattern on the provider side.
        /// </summary>
        public abstract Guid PatternProviderGuid { get; }

        /// <summary>
        /// The programmatic name for this pattern.
        /// </summary>
        public abstract string PatternName { get; }

        /// <summary>
        /// An object that implements IUIAutomationPatternHandler to handle
        /// dispatching and client-pattern creation for this pattern
        /// </summary>
        public abstract IUIAutomationPatternHandler Handler { get; }

        /// <summary>
        /// The assigned ID for this pattern.
        /// </summary>
        public int PatternId
        {
            get { return _patternId; }
        }

        /// <summary>
        /// The assigned ID for the IsXxxxPatternAvailable property.
        /// </summary>
        public int PatternAvailablePropertyId
        {
            get { return _patternAvailablePropertyId; }
        }

        /// <summary>
        /// Helper method to register this pattern.
        /// </summary>
        public void Register()
        {
            if (!_registered)
            {
                // Get our pointer to the registrar
                IUIAutomationRegistrar registrar =
                    new CUIAutomationRegistrarClass();

                // Set up the pattern struct
                var patternInfo = new UiaPatternInfoHelper(
                    PatternGuid,
                    PatternName,
                    PatternClientGuid,
                    PatternProviderGuid,
                    Handler
                    );

                // Populate it with properties and methods
                uint index = 0;
                foreach (var propertyInfo in Properties)
                {
                    patternInfo.AddProperty(propertyInfo);
                    propertyInfo.Index = index++;
                }
                foreach (var methodInfo in Methods)
                {
                    patternInfo.AddMethod(methodInfo);
                    methodInfo.Index = index++;
                }

                // Add the events, too, although they are not indexed
                foreach (var eventInfo in Events)
                {
                    patternInfo.AddEvent(eventInfo);
                }

                // Register the pattern
                int[] propertyIds;
                int[] eventIds;
                UiaPatternInfoHelper.RegisterPattern(registrar,
                                                     patternInfo,
                                                     out _patternId,
                                                     out _patternAvailablePropertyId,
                                                     out propertyIds,
                                                     out eventIds);

                // Write the property IDs back
                for (var i = 0; i < propertyIds.Length; ++i)
                {
                    Properties[i].PropertyId = propertyIds[i];
                }
                for (var i = 0; i < eventIds.Length; ++i)
                {
                    Events[i].EventId = eventIds[i];
                }

                _registered = true;
            }
        }
    }

    /// <summary>
    /// Base class for a custom pattern's client instance.
    /// Responsible for hiding some of the details of marshalling client-side custom calls;
    /// this is mostly syntactic sugar to keep the custom pattern instance neat.
    /// </summary>
    public class CustomClientInstanceBase
    {
        protected IUIAutomationPatternInstance PatternInstance;

        protected CustomClientInstanceBase(IUIAutomationPatternInstance patternInstance)
        {
            PatternInstance = patternInstance;
        }

        // Get a current property value for this custom property
        protected object GetCurrentPropertyValue(UiaPropertyInfoHelper propInfo)
        {
            var param = new UiaParameterHelper(propInfo.UiaType);
            PatternInstance.GetProperty(propInfo.Index, 0 /* fCached */, param.GetUiaType(), param.Data);
            return param.Value;
        }

        // Get a current property value by calling a method, rather than by using GetProperty
        protected object GetCurrentPropertyValueViaMethod(UiaMethodInfoHelper methodInfo)
        {
            // Create and init a parameter list
            var paramList = new UiaParameterListHelper(methodInfo);
            Debug.Assert(paramList.Count == 1);

            // Call through
            PatternInstance.CallMethod(methodInfo.Index, paramList.Data, paramList.Count);

            // Return the out-parameter
            return paramList[0];
        }

        // Get a cached property value for this custom property
        protected object GetCachedPropertyValue(UiaPropertyInfoHelper propInfo)
        {
            var param = new UiaParameterHelper(propInfo.UiaType);
            PatternInstance.GetProperty(propInfo.Index, 1 /* fCached */, param.GetUiaType(), param.Data);
            return param.Value;
        }

        // Call a pattern instance method with this parameter list
        protected void CallMethod(UiaMethodInfoHelper methodInfo, UiaParameterListHelper paramList)
        {
            PatternInstance.CallMethod(methodInfo.Index, paramList.Data, paramList.Count);
        }

        // Call a pattern instance method with only in-params
        protected void CallMethod(UiaMethodInfoHelper methodInfo, params object[] methodParams)
        {
            // Create and init a parameter list
            var paramList = new UiaParameterListHelper(methodInfo);
            paramList.Initialize(methodParams);

            // Call through
            PatternInstance.CallMethod(methodInfo.Index, paramList.Data, paramList.Count);
        }
    }
}