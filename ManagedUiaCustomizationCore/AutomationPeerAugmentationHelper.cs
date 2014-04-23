using System;
using System.Collections;
using System.Reflection;
using System.Windows.Automation.Peers;
using System.Windows.Threading;

namespace ManagedUiaCustomizationCore
{
    public static class AutomationPeerAugmentationHelper
    {
        public static void Register(CustomPatternSchemaBase schema)
        {
            // The only purpose of this method is to construct and correctly execute these lines of code:
            //
            //   var wrapObject = new AutomationPeer.WrapObject(WrapObjectReplacer);
            //   AutomationPeer.s_patternInfo[schema.PatternId] 
            //      = new AutomationPeer.PatternInfo(schema.PatternId, 
            //                                       wrapObject, 
            //                                       (PatternInterface)schema.PatternId);
            //
            //   The problem here is that AutomationPeer.WrapObject, AutomationPeer.s_patternInfo and 
            // AutomationPeer.PatternInfo are not public, so we need some hardcore reflection.
            //   Now, to be very clear, casting patternId to PatternInterface is not totally correct, 
            // but customly registered patterns get IDs near 50000 and max PatternInterface value is 
            // something about 20, so they won't intersect ever. On the other hand, after several hours
            // studying AutomationPeer sources it seems unfeasible to get what we need in other way 
            // because AutomationPeer wasn't written with extensibility in mind.
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

            var automationPeerType = typeof (AutomationPeer);
            var patternInfoHashtableField = automationPeerType.GetField("s_patternInfo", BindingFlags.NonPublic | BindingFlags.Static);

            // from AutomationPeer.cs: private delegate object WrapObject(AutomationPeer peer, object iface);
            var wrapObjectDelegateType = automationPeerType.GetNestedType("WrapObject", BindingFlags.NonPublic);
            var wrapObjectReplacerMethodInfo = ReflectionExtensions.GetMethodInfo(() => WrapObjectReplacer(null, null));
            var wrapObject = Delegate.CreateDelegate(wrapObjectDelegateType, wrapObjectReplacerMethodInfo);

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
                types: new[] {typeof (int), wrapObjectDelegateType, typeof (PatternInterface)},
                modifiers: null);
            var patternInfo = patternInfoTypeCtor.Invoke(new object[] {schema.PatternId, wrapObject, (PatternInterface)schema.PatternId});

            using (Dispatcher.CurrentDispatcher.DisableProcessing())
            {
                var patternInfoHashtable = (Hashtable)patternInfoHashtableField.GetValue(null);
                if (patternInfoHashtable.Contains(schema.PatternId)) return;
                patternInfoHashtable[schema.PatternId] = patternInfo;
            }
        }

        private static object WrapObjectReplacer(AutomationPeer peer, object iface)
        {
            // Real wrapper that this method should return is a decoration of iface and ensures three things:
            // 1) call to iface (which is actually an implementation of pattern interface, usually same object as peer)
            //    is made synchronously on the peer's dispatcher thread via peer.Dispatcher.Invoke(DispatcherPriority.Send, ...)
            // 2) any exceptions thrown on dispatcher thread are catched and re-thrown on calling thread to UIA
            // 3) if calls result object is not a primitivy type, but rather some other provider - it is wrapped before 
            //    return in another wrapper (possible of different type), because UIA requires that providers could be
            //    safely called from any thread
            // For now for simplicity we will assume our new pattern implementation will handle these things itself :)
            return iface;
        }
    }
}
