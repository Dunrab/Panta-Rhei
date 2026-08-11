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

    // start her up
    private void OnStartup(EntityUid uid, NosebleedComponent comp, ComponentStartup args)
    {
        ScheduleNextNosebleed(comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // whos got our comp
        var query = EntityQueryEnumerator<NosebleedComponent>();

        // schedule the nosebleed or wait
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp is not { NextNosebleed: { } nextNosebleed })
            {
                ScheduleNextNosebleed(comp);
                continue;
            }

            // keep going if its not time to schedule another nosebleed
            if (_timing.CurTime < nextNosebleed)
                continue;

            CauseNosebleed(uid, comp);
        }
    }

    private void ScheduleNextNosebleed(NosebleedComponent comp)
    {
        // our min, max times from our nosebleed comp
        var delay = _random.Next(TimeSpan.FromSeconds(comp.MinimumDelay), TimeSpan.FromSeconds(comp.MaximumDelay));

        // the current time + our delay
        comp.NextNosebleed = _timing.CurTime + delay;
    }

    private void CauseNosebleed(EntityUid uid, NosebleedComponent comp)
    {
        // we just had a nosebleed so we need to schedule the next one
        ScheduleNextNosebleed(comp);

        // get our mobstate for below
        if (!TryComp<MobStateComponent>(uid, out var mobState))
            return;

        // are they not alive? it would be funny if we let it happen on the dead...
        if (!_mobState.IsAlive(uid, mobState))
            return;

        // we want to send a different message to IPCs since they dont have noses
        if (TryComp<HumanoidAppearanceComponent>(uid, out var species))
        {
            if (species.Species == "IPC")
            {
                _popup.PopupEntity(Loc.GetString("nosebleed-message-ipc"), uid, uid, PopupType.MediumCaution);
            }
            else
                _popup.PopupEntity(Loc.GetString("nosebleed-message"), uid, uid, PopupType.MediumCaution);
        }

        // bleed on the floor time (the poor janitors im sorry)
        _bloodstream.TryModifyBleedAmount(uid, comp.BleedAmount);
    }
}
