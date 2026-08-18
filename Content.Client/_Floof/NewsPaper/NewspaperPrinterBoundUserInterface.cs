using Content.Shared._Floof.NewsPaper.Components;
using Robust.Client.UserInterface;

namespace Content.Client._Floof.NewsPaper;

public sealed class NewspaperPrinterBoundUserInterface : BoundUserInterface
{
    private NewspaperPrinterWindow? _window;

    public NewspaperPrinterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<NewspaperPrinterWindow>();
        _window.OnPrint += OnPrint;
        _window.OnClose += OnClose;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is NewspaperPrinterBoundUserInterfaceState printerState)
            _window?.UpdateState(printerState);
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
            _window.OnPrint -= OnPrint;
            _window.OnClose -= OnClose;
        }

        base.Dispose(disposing);
    }
}
