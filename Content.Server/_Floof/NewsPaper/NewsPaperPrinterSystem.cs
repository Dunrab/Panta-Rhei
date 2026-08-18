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
                subs.Event<NewspaperPrinterPrintMessage>(OnPrint);
            });

        SubscribeLocalEvent<NewspaperPrinterComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(Entity<NewspaperPrinterComponent> ent, ref ComponentInit args)
    {
        UpdateUi(ent);
    }

    private void OnPrint(Entity<NewspaperPrinterComponent> ent, ref NewspaperPrinterPrintMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg.Text) || msg.Text.Length > ent.Comp.MaxContentLength)
            return;

        ent.Comp.NextPrint = _timing.CurTime + ent.Comp.PrintCooldown;
        Dirty(ent);

        // we to use our paper prototype
        var paper = Spawn("Newspaper", Transform(ent).Coordinates);

        if (!TryComp<PaperComponent>(paper, out var paperComp))
        {
            Log.Error($"Newspaper printer {ToPrettyString(ent)} spawned Paper " + $"without a PaperComponent.");

            QueueDel(paper);
            return;
        }

        // Respect the actual paper's configured capacity.
        if (msg.Text.Length > paperComp.ContentSize)
        {
            QueueDel(paper);
            return;
        }

        // Put the player's text onto the physical paper.
        _paper.SetContent((paper, paperComp), msg.Text);

        _audio.PlayPvs(ent.Comp.PrintSound, ent);
        Dirty(paper, paperComp);

        UpdateUi(ent);
    }

    private void UpdateUi(Entity<NewspaperPrinterComponent> ent)
    {
        if (!_ui.HasUi(ent, NewspaperPrinterUiKey.Key))
            return;

        var canPrint = _timing.CurTime >= ent.Comp.NextPrint;

        _ui.SetUiState(ent.Owner, NewspaperPrinterUiKey.Key, new NewspaperPrinterBoundUserInterfaceState(canPrint, ent.Comp.MaxContentLength));
    }
}
