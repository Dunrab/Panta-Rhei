using Content.Shared._Coyote.SniffAndSmell;
using Content.Shared.EntityEffects;

namespace Content.Shared._Euphoria.EntityEffects.Effects;

public sealed partial class PerfumeEntityEffect : EntityEffectSystem<ScentComponent, ApplyScentEffect>
{
    [Dependency]
    private readonly ScentSystem _scentSystem = default!;

    protected override void Effect(Entity<ScentComponent> entity, ref EntityEffectEvent<ApplyScentEffect> args)
    {
        entity.Comp.Scents.Clear();

        _scentSystem.AddScentPrototype(entity, args.Effect.Scent);
    }
}

public sealed partial class ApplyScentEffect : EntityEffectBase<ApplyScentEffect>
{
    [DataField(required: true)]
    public string Scent = string.Empty;
}

