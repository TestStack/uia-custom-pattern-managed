using System;

namespace ManagedUiaCustomizationCore
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PatternMethodAttribute : Attribute
    {
        /// <summary>
        /// true if UI Automation should set the focus on the object before calling the method; otherwise false.
        /// </summary>
        public bool DoSetFocus { get; set; }

        /// <summary>
        /// true if pattern method should not be invoked on target application's UI thread. It can be useful e.g.
        /// if the call should not block UI thread, but wait for something.
        /// </summary>
        public bool DoNotDispatchToUIThread { get; set; }
    }
}