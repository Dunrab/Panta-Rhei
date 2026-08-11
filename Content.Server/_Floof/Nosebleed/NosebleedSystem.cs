using Content.Server.Body.Systems;
using Content.Shared._Floof.Nosebleed;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Floof.Nosebleed;

public sealed class NosebleedSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NosebleedComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, NosebleedComponent comp, ComponentStartup args)
    {
        ScheduleNextNosebleed(new Entity<NosebleedComponent>(uid, comp));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NosebleedComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            var ent = new Entity<NosebleedComponent>(uid, comp);

            if (ent.Comp is not { NextNosebleed: { } nextNosebleed })
            {
                ScheduleNextNosebleed(ent);
                continue;
            }

            if (_timing.CurTime < nextNosebleed)
                continue;

            CauseNosebleed(ent);
        }
    }

    private void ScheduleNextNosebleed(Entity<NosebleedComponent> ent)
    {
        // our min, max times from our nosebleed comp
        var delay = _random.Next(TimeSpan.FromSeconds(ent.Comp.MinimumDelay), TimeSpan.FromSeconds(ent.Comp.MaximumDelay));

        // the current time + our delay
        ent.Comp.NextNosebleed = _timing.CurTime + delay;
    }

    private void CauseNosebleed(Entity<NosebleedComponent> ent)
    {
        ScheduleNextNosebleed(ent);

        if (!TryComp<MobStateComponent>(ent.Owner, out var mobState))
            return;

        // are they not alive? it would be funny if we let it happen on the dead...
        if (!_mobState.IsAlive(ent.Owner, mobState))
            return;

        // we want to send a different message to IPCs since they dont have noses
        if (TryComp<HumanoidAppearanceComponent>(ent.Owner, out var species))
        {
            if (species.Species == "IPC")
            {
                _popup.PopupEntity(Loc.GetString("nosebleed-message-ipc"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            }
            else
                _popup.PopupEntity(Loc.GetString("nosebleed-message"), ent.Owner, ent.Owner, PopupType.MediumCaution);
        }

        // bleed on the floor time (the poor janitors im sorry)
        _bloodstream.TryModifyBleedAmount(ent.Owner, ent.Comp.BleedAmount);
    }
}
