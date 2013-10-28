using System.Runtime.InteropServices;
using Interop.UIAutomationCore;


namespace UIAControls
{
    /// <summary>
    /// A basic implementation of the Fragment provider.
    /// This adds the concept of a sub-window element to the Simple provider,
    /// and so includes work to report element aspects that the system
    /// would usually get for free from a window handle, like position and runtime ID.
    /// </summary>
    [ComVisible(true)]
    public abstract class BaseFragmentProvider : BaseSimpleProvider, IRawElementProviderFragment
    {
        protected IRawElementProviderFragment parent;
        protected IRawElementProviderFragmentRoot fragmentRoot;

        protected BaseFragmentProvider(IRawElementProviderFragment parent, IRawElementProviderFragmentRoot fragmentRoot)
        {
            this.parent = parent;
            this.fragmentRoot = fragmentRoot;
        }

        // Return a unique runtime ID to distinguish this from other elements.
        // This is required to implement.  It is usually best to return an array
        // starting with the AppendRuntimeId so that it will be joined to the
        // fragment root's runtime -- then you just need to add a unique suffix.
        public abstract int[] GetRuntimeId();

        // Return the bounding rectangle of the fragment.
        // This is required to implement.
        public abstract UiaRect get_BoundingRectangle();

        // Return any fragment roots embedded within this fragment - uncommon
        // unless this is a fragment hosting another full HWND.
        public virtual IRawElementProviderFragmentRoot[] GetEmbeddedFragmentRoots()
        {
            return null;
        }

        // Set focus to this fragment, if it is keyboard focusable.
        public virtual void SetFocus()
        {
        }

        // Return the fragment root: the fragment that is tied to the window handle itself.
        // Don't override, since the constructor requires the fragment root already.
        public IRawElementProviderFragmentRoot FragmentRoot
        {
            get { return fragmentRoot; }
        }

        // Routing function for going to neighboring elements.  We implemented
        // this to delegate to other virtual functions, so don't override it.
        public IRawElementProviderFragment Navigate(NavigateDirection direction)
        {
            switch (direction)
            {
                case NavigateDirection.NavigateDirection_Parent: return parent;
                case NavigateDirection.NavigateDirection_FirstChild: return GetFirstChild();
                case NavigateDirection.NavigateDirection_LastChild: return GetLastChild();
                case NavigateDirection.NavigateDirection_NextSibling: return GetNextSibling();
                case NavigateDirection.NavigateDirection_PreviousSibling: return GetPreviousSibling();
            }
            return null;
        }

        // Return the first child of this fragment.
        protected virtual IRawElementProviderFragment GetFirstChild()
        {
            return null;
        }

        // Return the last child of this fragment.
        protected virtual IRawElementProviderFragment GetLastChild()
        {
            return null;
        }

        // Return the next sibling of this fragment.
        protected virtual IRawElementProviderFragment GetNextSibling()
        {
            return null;
        }

        // Return the previous sibling of this fragment.
        protected virtual IRawElementProviderFragment GetPreviousSibling()
        {
            return null;
        }
    }
}
