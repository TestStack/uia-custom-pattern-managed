using System;
using System.Collections;
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
using Castle.DynamicProxy;

namespace ManagedUiaCustomizationCore
{
    public static class AutomationPeerAugmentationHelper
    {
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public static void Register(CustomPatternSchemaBase schema)
        {
            //
            // TODO: If we want to support standalone properties - we have to make very similar trick for AutomationPeer.s_propertyInfo.
            //
            //   For events AutomationPeers goes other way and list of events it can raise through 
            // AutomationPeer.RaiseAutomationEvent() is very strictly hardcoded with switch()
            // operator in the EventMap internal class. But it is possible to get IRawElementProviderSimple
            // from the protected AutomationPeer.ProviderFromPeer method and use it directly via
            // NativeMethods.UiaRaiseAutomationEvent. Basically it is the same AutomationPeer does, 
            // so the only inconvenience would be non-standard method of raising.
            //   Another piece required here is to replicate for AutomationPeer.ListenersExist(). Seems to
            // fully support custom events we would have to rewrite EventsMap class. Fortunately it is small :)
            //
            // TODO: Add support for raising custom UIA events

            RegisterPattern(schema);
            foreach (var property in schema.Properties)
            {
                RegisterProperty(property);
            }
        }

        private static void RegisterPattern(CustomPatternSchemaBase schema)
        {
            var automationPeerType = typeof(AutomationPeer);
            // The only purpose of this method is to construct and correctly execute these lines of code:
            //
            //   var wrapper = new Wrapper(schema.PatternProviderInterface);
            //   var wrapObject = new AutomationPeer.WrapObject(wrapper.WrapObjectReplacer);
            //   AutomationPeer.s_patternInfo[schema.PatternId] 
            //      = new AutomationPeer.PatternInfo(schema.PatternId, 
            //                                       wrapObject, 
            //                                       (PatternInterface)schema.PatternId);
            //   AutomationPattern.Register(schema.PatternGuid, schema.PatternName);
            //
            //   The problem here is that AutomationPeer.WrapObject, AutomationPeer.s_patternInfo, AutomationPattern.Register
            // and AutomationPeer.PatternInfo are not public, so we need some hardcore reflection.
            //   Now, to be very clear, casting patternId to PatternInterface is not totally correct, 
            // but customly registered patterns get IDs near 50000 and max PatternInterface value is 
            // something about 20, so they won't intersect ever. On the other hand, after several hours
            // studying AutomationPeer sources it seems unfeasible to get what we need in other way 
            // because AutomationPeer wasn't written with extensibility in mind.
            var patternInfoHashtableField = automationPeerType.GetField("s_patternInfo", BindingFlags.NonPublic | BindingFlags.Static);

            // from AutomationPeer.cs: private delegate object WrapObject(AutomationPeer peer, object iface);
            var wrapObjectDelegateType = automationPeerType.GetNestedType("WrapObject", BindingFlags.NonPublic);
            var wrapper = new Wrapper(schema.PatternProviderInterface);
            var wrapObjectReplacerMethodInfo = ReflectionUtils.GetMethodInfo(() => wrapper.WrapObjectReplacer(null, null));
            var wrapObject = Delegate.CreateDelegate(wrapObjectDelegateType, wrapper, wrapObjectReplacerMethodInfo);
            
            // from AutomationPeer.cs:  
            //private class PatternInfo
            //{
            //  internal int Id;
            //  internal AutomationPeer.WrapObject WrapObject;
            //  internal PatternInterface PatternInterface;

            //  internal PatternInfo(int id, AutomationPeer.WrapObject wrapObject, PatternInterface patternInterface)
            //  {
            //    this.Id = id;
            //    this.WrapObject = wrapObject;
            //    this.PatternInterface = patternInterface;
            //  }
            //}
            var patternInfoType = automationPeerType.GetNestedType("PatternInfo", BindingFlags.NonPublic);
            var patternInfoTypeCtor = patternInfoType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                types: new[] {typeof(int), wrapObjectDelegateType, typeof(PatternInterface)},
                modifiers: null);
            var patternInfo = patternInfoTypeCtor.Invoke(new object[] {schema.PatternId, wrapObject, (PatternInterface)schema.PatternId});

            var automationPatternType = typeof(AutomationPattern);
            var registerMethod = automationPatternType.GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Static);

            using (Dispatcher.CurrentDispatcher.DisableProcessing())
            {
                var patternInfoHashtable = (Hashtable)patternInfoHashtableField.GetValue(null);
                if (patternInfoHashtable.Contains(schema.PatternId)) return;
                patternInfoHashtable[schema.PatternId] = patternInfo;
                registerMethod.Invoke(null, new object[] {schema.PatternGuid, schema.PatternName});
            }
        }

        private static void RegisterProperty(UiaPropertyInfoHelper property)
        {
            var automationPropertyType = typeof(AutomationProperty);
            var registerMethod = automationPropertyType.GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Static);

            using (Dispatcher.CurrentDispatcher.DisableProcessing())
                registerMethod.Invoke(null, new object[] { property.Guid, property.Data.pProgrammaticName});
        }

        // we have to capture providerInterfaceType; as lambda captures are ony compiler syntactic sugar - we
        // have to recreate its magic here by hand
        private class Wrapper
        {
            private readonly Type _providerInterfaceType;

            public Wrapper(Type providerInterfaceType)
            {
                _providerInterfaceType = providerInterfaceType;
            }

            public object WrapObjectReplacer(AutomationPeer peer, object iface)
            {
                var interceptor = new SendingToUIThreadInterceptor(peer);
                return _proxyGenerator.CreateInterfaceProxyWithTarget(_providerInterfaceType, iface, interceptor);
            }
        }


        private class SendingToUIThreadInterceptor : IInterceptor
        {
            private readonly Dispatcher _dispatcher;

            public SendingToUIThreadInterceptor(AutomationPeer peer)
            {
                _dispatcher = peer.Dispatcher;
            }

            public void Intercept(IInvocation invocation)
            {
                if (_dispatcher.CheckAccess())
                {
                    invocation.Proceed();
                }
                else
                {
                    Action a = invocation.Proceed;
                    _dispatcher.Invoke(a);
                }
            }
        }
    }
}
