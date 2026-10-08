using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Euphoria.Perfume;

[Serializable, NetSerializable]
public sealed partial class PerfumeDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
}
