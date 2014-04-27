uia-custom-pattern-managed
==========================

Managed implementation of custom UIA patterns (for WinForms and WPF) both for x64 and x86.

Initial version mostly taken from these blog posts:

http://blogs.msdn.com/b/winuiautomation/archive/2010/12/08/uia-custom-patterns-part-1.aspx

http://blogs.msdn.com/b/winuiautomation/archive/2010/12/10/uia-custom-patterns-part-2.aspx

http://blogs.msdn.com/b/winuiautomation/archive/2010/12/27/uia-custom-patterns-part-3.aspx

Additionally implementted most of the groundwork to make user declare his pattern interface, attach there several attributes with GUIDs and the like and implement it on the AutomationPeer (WPF) or on the custom control (WinForms). Without diving really deep into Marshalling, UIA internals etc.

P.S. To make it run you may need to edit WindowsSdkDir property in UIACoreInterop.vcxproj and UIAClientInterop.vcxproj files if you have it installed somewhere else.
