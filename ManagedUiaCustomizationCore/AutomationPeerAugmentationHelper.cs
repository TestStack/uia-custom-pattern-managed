using System;
using System.Collections;
using System.Reflection;
using System.Windows.Automation.Peers;
using System.Windows.Threading;

namespace ManagedUiaCustomizationCore
{
    public class AutomationPeerAugmentationHelper
    {
        public static void Register(CustomPatternSchemaBase schema)
        {
            var automationPeerType = typeof (AutomationPeer);
            var patternInfoHashtableField = automationPeerType.GetField("s_patternInfo", BindingFlags.NonPublic | BindingFlags.Static);

            // from AutomationPeer.cs: private delegate object WrapObject(AutomationPeer peer, object iface);
            var wrapObjectDelegateType = automationPeerType.GetNestedType("WrapObject", BindingFlags.NonPublic);
            var wrapObjectReplacerMethodInfo = typeof (AutomationPeerAugmentationHelper)
                .GetMethod(
                    name: "WrapObjectReplacer",
                    bindingAttr: BindingFlags.NonPublic | BindingFlags.Static,
                    binder: null,
                    types: new[] {typeof (AutomationPeer), typeof (object)},
                    modifiers: null);
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
