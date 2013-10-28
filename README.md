uia-custom-pattern-managed
==========================

Sample implementation of Windows UI Automation Custom Pattern in managed code.

Sample mostly taken from these blog posts:
http://blogs.msdn.com/b/winuiautomation/archive/2010/12/08/uia-custom-patterns-part-1.aspx
http://blogs.msdn.com/b/winuiautomation/archive/2010/12/10/uia-custom-patterns-part-2.aspx
http://blogs.msdn.com/b/winuiautomation/archive/2010/12/27/uia-custom-patterns-part-3.aspx

Plus some minor changes to make it work in x64 environment and converted to VS2012.

To make it run you may need to edit WindowsSdkDir property in UIACoreInterop.vcxproj and UIAClientInterop.vcxproj files if you have it installed somewhere else.
