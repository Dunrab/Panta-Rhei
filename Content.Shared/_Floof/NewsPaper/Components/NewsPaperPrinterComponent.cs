using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Floof.NewsPaper.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NewspaperPrinterComponent : Component
{
    /// <summary>
    /// This stores what the user is currently typing so others can see if they use the console
    /// </summary>
    [AutoNetworkedField]
    [DataField, ViewVariables]
    public string DraftContent = "";

    /// <summary>
    /// Maximum amount of text the printer will accept.
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
    public readonly string DraftContent;

    public NewspaperPrinterBoundUserInterfaceState(
        bool canPrint,
        int maxContentLength,
        string draftContent)
    {
        CanPrint = canPrint;
        MaxContentLength = maxContentLength;
        DraftContent = draftContent;
    }
}

[Serializable, NetSerializable]
public sealed class NewspaperPrinterRequestStateMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class NewspaperPrinterPrintMessage(string text) : BoundUserInterfaceMessage
{
    public readonly string Text = text;
}

[Serializable, NetSerializable]
public enum NewspaperPrinterUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class NewspaperPrinterDraftChangedMessage : BoundUserInterfaceMessage
{
    public readonly string Text;

    public NewspaperPrinterDraftChangedMessage(string text)
    {
        Text = text;
    }
}
