using Content.Shared._Floof.NewsPaper.Components;
using Content.Shared.Paper;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Floof.NewsPaper;

public sealed class NewspaperPrinterSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<NewspaperPrinterComponent>(NewspaperPrinterUiKey.Key,
            subs =>
            {
                subs.Event<NewspaperPrinterDraftChangedMessage>(OnDraftChanged);
                subs.Event<NewspaperPrinterRequestStateMessage>(OnRequestState);
                subs.Event<NewspaperPrinterPrintMessage>(OnPrint);
            });

        SubscribeLocalEvent<NewspaperPrinterComponent, ComponentInit>(OnComponentInit);
    }

    private void OnDraftChanged(Entity<NewspaperPrinterComponent> ent, ref NewspaperPrinterDraftChangedMessage msg)
    {
        if (msg.Text.Length > ent.Comp.MaxContentLength)
            return;

        ent.Comp.DraftContent = msg.Text;

        Dirty(ent);

        UpdateUi(ent);
    }


    private void OnRequestState(Entity<NewspaperPrinterComponent> ent, ref NewspaperPrinterRequestStateMessage msg)
    {
        UpdateUi(ent);
    }

    private void OnComponentInit(Entity<NewspaperPrinterComponent> ent, ref ComponentInit args)
    {
        UpdateUi(ent);
    }

    private void OnPrint(Entity<NewspaperPrinterComponent> ent, ref NewspaperPrinterPrintMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg.Text) || msg.Text.Length > ent.Comp.MaxContentLength)
            return;

        // we to use our paper prototype
        var paper = Spawn("Newspaper", Transform(ent).Coordinates);

        if (!TryComp<PaperComponent>(paper, out var paperComp))
        {
            Log.Error($"Newspaper printer {ToPrettyString(ent)} spawned Paper " + $"without a PaperComponent.");

            QueueDel(paper);
            return;
        }

        if (msg.Text.Length > paperComp.ContentSize)
        {
            QueueDel(paper);
            return;
        }

        // store the text in the comp similar to the newswritercomp
        ent.Comp.DraftContent = msg.Text;

        ent.Comp.NextPrint = _timing.CurTime + ent.Comp.PrintCooldown;

        // put the text from the comp onto the printed paper
        _paper.SetContent((paper, paperComp), ent.Comp.DraftContent);

        _audio.PlayPvs(ent.Comp.PrintSound, ent);
        Dirty(paper, paperComp);

        UpdateUi(ent);
    }

    private void UpdateUi(Entity<NewspaperPrinterComponent> ent)
    {
        if (!_ui.HasUi(ent, NewspaperPrinterUiKey.Key))
            return;

        var canPrint = _timing.CurTime >= ent.Comp.NextPrint;

        _ui.SetUiState(ent.Owner, NewspaperPrinterUiKey.Key, new NewspaperPrinterBoundUserInterfaceState(canPrint, ent.Comp.MaxContentLength, ent.Comp.DraftContent));
    }
}
