using Content.Shared._Floof.NewsPaper.Components;
using Robust.Client.UserInterface;

namespace Content.Client._Floof.NewsPaper;

public sealed class NewspaperPrinterBoundUserInterface : BoundUserInterface
{
    private NewspaperPrinterWindow? _window;
    private bool draftRestored;

    public NewspaperPrinterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        draftRestored = false;

        _window = this.CreateWindow<NewspaperPrinterWindow>();
        _window.OnDraftChanged += OnDraftChanged;
        _window.OnPrint += OnPrint;
        _window.OnClose += OnClose;

        SendMessage(new NewspaperPrinterRequestStateMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not NewspaperPrinterBoundUserInterfaceState printerState)
            return;

        if (_window == null)
            return;

        if (!draftRestored)
        {
            draftRestored = true;
            _window.RestoreDraft(printerState.DraftContent);
        }

        _window.UpdateState(printerState);
    }

    private void OnDraftChanged(string text)
    {
        SendMessage(new NewspaperPrinterDraftChangedMessage(text));
    }

    private void OnPrint(string text)
    {
        SendMessage(new NewspaperPrinterPrintMessage(text));
    }

    private void OnClose()
    {
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (_window != null)
        {
            _window.OnDraftChanged -= OnDraftChanged;
            _window.OnPrint -= OnPrint;
            _window.OnClose -= OnClose;
        }

        base.Dispose(disposing);
    }
}
