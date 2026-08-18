using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Floof.NewsPaper.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class NewspaperPrinterComponent : Component
{
    /// <summary>
    /// Maximum amount of text the printer will accept.
    /// The actual paper's ContentSize is also checked server-side.
    /// </summary>
    [DataField]
    public int MaxContentLength = 10000;

    /// <summary>
    /// Time between prints.
    /// </summary>
    [DataField]
    public TimeSpan PrintCooldown = TimeSpan.FromSeconds(2);

    [ViewVariables]
    public TimeSpan NextPrint;

    /// <summary>
    /// Sound to play when printing new newspaper
    /// </summary>
    [DataField]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");
}

[Serializable, NetSerializable]
public sealed class NewspaperPrinterBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly bool CanPrint;
    public readonly int MaxContentLength;

    public NewspaperPrinterBoundUserInterfaceState(bool canPrint, int maxContentLength)
    {
        CanPrint = canPrint;
        MaxContentLength = maxContentLength;
    }
}

[Serializable, NetSerializable]
public sealed class NewspaperPrinterRequestStateMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class NewspaperPrinterPrintMessage : BoundUserInterfaceMessage
{
    public readonly string Text;

    public NewspaperPrinterPrintMessage(string text)
    {
        Text = text;
    }
}

[Serializable, NetSerializable]
public enum NewspaperPrinterUiKey
{
    Key
}
