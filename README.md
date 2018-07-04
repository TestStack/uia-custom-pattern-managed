# Changes

## v0.1.2

- updated Castle.Core dependency to 4.3.1

## v0.1.1

- added ability to avoid dispatching UIA calls to UI thread on server side: `PatternMethodAttribute.DoNotDispatchToUIThread`

## v0.1.0

- created NuGet package for the library along with auto-deployment/tagging script
- added source indexing with GitLink. Makes issue debugging much easier as stepping into UIAComWrapper code in VS automatically downloads proper source code from GitHub.
- updated Castle.Core required version to 3.3.0 to be compatible with other TestStack packages

# Creating custom UIA patterns and properties with C# #

UIA (User Interface Automation) is a Microsoft technology for application automation (e.g. for automated testing). It is written in native code as COM library. There's managed wrapper of UIA v2 by Microsoft and open-source wrapper for UIA v3, but both these libs cover only consuming predefined patterns and properties, making it near-to-impossible to register new pattern without diving deep into COM interop and UIA documentation.   

This library makes it easy to create new UIA patterns and properties in managed code for WinForms and WPF both for x64 and x86.

## Possible issues if using from sources ##

You must have Windows SDK installed in order to run build of the solution. Also if it installed to non-default paths, you have to make change to `UiaCoreInterop\UIACoreInterop.vcxproj` file and replace this path:
`<WindowsSdkDir>c:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\</WindowsSdkDir>`
so that file `$(WindowsSdkDir)\include\UIAutomationCore.idl` could be found.

## Creating your own custom pattern ##

To demonstrate, we will create a pattern to control caret position in the WPF TextBox control - it is actually implemented in the sources, so you may see it working yourself.

### Terminology ###
 
**Server side and client side in UIA**. In the terminology of UIA there are two sides of interaction: server-side is where the control resides, and client-side is where automation happens (e.g. test driver which emulates user action on server-side control). It is important to understand that server and client side reside in different processes and cannot communicate except through UIA. 

There are three pieces required to implement and consume custom UIA pattern:
- **Provider interface** - it is the interface that server-side control needs to implement
- **Pattern interface** - it is the interface which exposes automation features to the client
- **Registration info container** - it is holder of information about the pattern. During pattern registration in the UIA, it receives ID, each of its properties receive ID as well and then participants of automation can refer to the concrete property by its ID. This container holds these IDs.

### Implementation ###

For simplicity we will define custom pattern in the same assembly where custom control placed and assume that automation code has access to it. It is not necessary in general, but convenient. 

First, we need to define **server-side provider interface**. We'll expose current caret position and length of selection as properties and a method to set these if need be ([here](https://github.com/ivan-danilov/uia-custom-pattern-managed/blob/master/WpfAppWithAdvTextControl/ICaretPositionProvider.cs)):
```csharp
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[ComImport]
[Guid("0F268572-8746-4105-9188-080086FCC6E4")]
[PatternGuid("B85FDDEA-D38F-44D6-AE42-0CA3CF0433F1")]
public interface ICaretPositionProvider
{
    [PatternProperty("6B55247F-6BAF-460C-9C3E-388E7161A7E9")]
    int SelectionStart { get; }

    [PatternProperty("F0CD6926-AA86-4EBF-BDCC-7345C5D98EC6")]
    int SelectionLength { get; }

    [PatternMethod]
    void SetSelectionStart(int value);

    [PatternMethod]
    void SetSelectionLength(int value);
}
```

`InterfaceType(ComInterfaceType.InterfaceIsIUnknown)` and `ComImport` are required by UIA. Also you have to generate Guid (in Visual Studio it could be done via Tools -> Create GUID). `Guid` attribute here is the GUID of the COM interface. Also UIA requires that each pattern, event and property has its own unique GUID, hence `PropertyGuid` attribute and `PatternProperty` for each property interface exposes. Note that methods do not require this because they are called only as part of the interface (while properties, for example, could also participate in caching).

UIA properties support only getters, so no setters here. Instead of setters we have methods.

Also, UIA supports only five types for custom patterns and properties: `bool`, `int`, `double`, `string` and AutomationElement (see [AutomationElements section](#AutomationElements)).

The second step is to write **client-side pattern interface**. It is a mechanical transformation of provider interface: named the same except `Pattern` instead of `Provider` in the end; methods are translated as they are, and for each property `Abc` in the provider interface we add two properties of the same type, named `CachedAbc` and `CurrentAbc`. Plus, generate new GUID, but only for pattern interface itself, other attributes removed. In case of our caret position pattern it becomes ([here](https://github.com/ivan-danilov/uia-custom-pattern-managed/blob/master/WpfAppWithAdvTextControl/ICaretPositionPattern.cs)):

```csharp
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[ComImport]
[Guid("0FC33FD3-3874-4A32-A530-0EBE937D4419")]
public interface ICaretPositionPattern
{
    int CurrentSelectionStart { get; }
    int CurrentSelectionLength { get; }

    int CachedSelectionStart { get; }
    int CachedSelectionLength { get; }

    void SetSelectionStart(int value);
    void SetSelectionLength(int value);
}
```

The final part is the **registration info container for the pattern** and registration itself. It is another mechanical transformation of the provider mostly ([here](https://github.com/ivan-danilov/uia-custom-pattern-managed/blob/master/WpfAppWithAdvTextControl/CaretPositionPattern.cs)): 

```csharp
public class CaretPositionPattern : CustomPatternBase<ICaretPositionProvider, ICaretPositionPattern>
{
    private CaretPositionPattern() 
        : base(usedInWpf: true)
    {
    }

    public static void Initialize()
    {
        if (PatternSchema != null) return;
        PatternSchema = new CaretPositionPattern();
    }

    public static CaretPositionPattern PatternSchema;

    // these will be set via reflection on Initialize() call
    public static AutomationPattern Pattern;
    public static AutomationProperty SelectionStartProperty;
    public static AutomationProperty SelectionLengthProperty;
}
```

Last block of public static fields is important. First should be named `Pattern` and also each property `Abc` should have corresponding field `AbcProperty`. Types may be either `AutomationPattern`/`AutomationProperty` for WPF usage as shown here, or plain `int` if you plan to use UIA directly. Also mention base class which takes two typeparams for provider and pattern interfaces, and its constructor which takes bool argument. If you don't pass true there - it won't work with WPF as WPF needs some additional calls in order to know about new patterns/properties.

Basically that is all we need to declare new pattern correctly.

### Providing and consuming new pattern ###

Let's describe the process of exposing new pattern from your custom control. Here we'll extend standard `TextBox` control, so we will inherit our new control from it ([here](https://github.com/ivan-danilov/uia-custom-pattern-managed/blob/master/WpfAppWithAdvTextControl/AdvTextBox.cs)):

```csharp
public class AdvTextBox : TextBox
{
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new AdvTextBoxAutomationPeer(this);
    }
}
``` 

It is important step: for work with UIA WPF uses notion of AutomationPeers, thus to expose some new functionality custom WPF control, we have to define our own `AutomationPeer` descendant type and add something there. Like this ([here](https://github.com/ivan-danilov/uia-custom-pattern-managed/blob/master/WpfAppWithAdvTextControl/AdvTextBoxAutomationPeer.cs)):

```csharp
public class AdvTextBoxAutomationPeer : TextBoxAutomationPeer, ICaretPositionProvider
{
    public AdvTextBoxAutomationPeer(AdvTextBox owner)
        : base(owner)
    {
        CaretPositionPattern.Initialize();
    }

    private new AdvTextBox Owner
    {
        get { return (AdvTextBox)base.Owner; }
    }

    protected override string GetClassNameCore()
    {
        return "AdvTextBox";
    }

    public override object GetPattern(PatternInterface patternInterface)
    {
        if ((int)patternInterface == CaretPositionPattern.Pattern.Id)
            return this;
        return base.GetPattern(patternInterface);
    }

    public int SelectionStart
    {
        get { return Owner.SelectionStart; }
    }

    public int SelectionLength
    {
        get { return Owner.SelectionLength; }
    }

    public void SetSelectionStart(int value)
    {
        Owner.SelectionStart = value;
    }

    public void SetSelectionLength(int value)
    {
        Owner.SelectionLength = value;
    }
}
```

Each call to the automation peer from UIA happens on the UI thread, so it is safe to call owner control directly. Important pieces here are:
- implementing `ICaretPositionProvider` interface
- calling `CaretPositionPattern.Initialize()` in the constructor
- overriding `GetPattern` so that if new pattern is requested - our implementation is returned

Piece of cake, isn't it?

Now, how could we consume that new pattern from client side? It is not hard at all as well. I'll suppose you know how to get `AutomationElement` instance for your custom control, so from there:

```csharp
CaretPositionPattern.Initialize();
var cps = (ICaretPositionPattern)automationElement.GetCurrentPattern(CaretPositionPattern.Pattern);
cps.SetSelectionStart(1);
cps.SetSelectionLength(2);
```

It is not required to call `CaretPositionPattern.Initialize()` before each usa, but you have to ensure that this call happened in the client process prior to using the pattern. And as you may see from above, it does something only on first call, so nothing bad happens if you call it several times.

## Known issue with more than 2 custom properties on Win 7 ##

UIA has a bug on Windows 7 (it was fixed in Windows 8): if you register custom UIA pattern with more than 2 properties - registration succeeds, but the property always returns default value and thus unusable. In order to workaround this issue, you may define standalone UIA property which doesn't have this limitation.

Now, standalone properties go though a bit different path, so registration and providing them differs a bit.

In order to **register** standalone property, you have to add to your custom pattern registration info container new public static field by with the same rules as for normal pattern properties, but with additional attribute, like so:
```csharp
[StandaloneProperty("36683304-3B8A-4035-A88C-B7384C7F057F", typeof(int))]
public static AutomationProperty Standalone1Property;
```

To provide value for such property in raw UIA, you go the usual route: just return required value when your `IRawElementProviderSimple.GetPropertyValue` method is called. In the WPF you have to implement `IStandalonePropertyProvider` interface on your `AutomationPeer`. It has single method `object GetPropertyValue(AutomationProperty property)` with obvious semantics. Note that you also have to call `Initialize()` method in order to obtain valid reference to the `AutomationProperty` instance.

Note that standalone properties support only for basic types (`bool`, `int`, `double` and `string`) and do not support returning automation elements. 

## <a name="AutomationElements"></a>Working with `IUIAutomationElement`, `AutomationElement` and `IRawElementProviderSimple` types ##

Now, if you need to return automation element from your code, there's a bit of complexity comparing to simple types. Automation elements are represented differently on server and client side. Moreover, UIAComWrapper which White uses, wraps client-side representation with its own class, effectively hiding part of native COM complexity.

So, which types are involved?

**On server side** it is `IRawElementProviderSimple`. Remember, server side is where your custom control resides. How do you get instance of this interface?

If you're writing for WinForms - you have probably implemented it yourself, so it is easy.

WPF is slightly more friendly here: you don't have to implement this interface yourself - `AutomationPeer` handles managing and creation of the one automatically. You may obtain instance for your descendant of `AutomationPeer` by calling `protected IRawElementProviderSimple AutomationPeer.ProviderFromPeer(AutomationPeer peer)` method. Pass your peer there or other one if you need, e.g. while implementing some kind of container control pattern. Note though, that you cannot provide some random peer there from another window, because WPF will validate that calling peer (`this` one) and peer, passed as an argument, are connected (from the point of view of UIA tree) - and return `null` if they're not.

So, to summarize: in order to return automation element type, you need to declare Provider interface so that either a) type of some pattern property (standalone properties does not support automation elements); b) return type of some method; or c) `out` parameter of some method - is `IRawElementProviderSimple`. Plus implement it on your custom control, automation peer, wherever.

**On client side** corresponding type may be either `IUIAutomationElement` - which will be most suitable if you're working with raw UIA in COM version, or `AutomationElement` from UIAComWrapper library. ManagedUiaCustomizationCore library will convert native COM `IUIAutomationElement` result to `AutomationElement` when necessary. You may even mix the two approaches (though I cannot imagine situation when this could be required).

You may find **example** of declaring automation element properties/methods [here](https://github.com/ivan-danilov/uia-custom-pattern-managed/blob/master/WpfAppWithAdvTextControl/AutomationElementRetievingPattern.cs), providing them [here](https://github.com/ivan-danilov/uia-custom-pattern-managed/blob/master/WpfAppWithAdvTextControl/TestControlAutomationPeer.cs) and consuming from test code [here](https://github.com/ivan-danilov/uia-custom-pattern-managed/blob/master/UiaControlsTest/WpfAppTests.cs).


## Acknowledgments ##

Initial version mostly taken from these blog posts:

http://blogs.msdn.com/b/winuiautomation/archive/2010/12/08/uia-custom-patterns-part-1.aspx

http://blogs.msdn.com/b/winuiautomation/archive/2010/12/10/uia-custom-patterns-part-2.aspx

http://blogs.msdn.com/b/winuiautomation/archive/2010/12/27/uia-custom-patterns-part-3.aspx
