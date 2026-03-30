using Robust.Server.GameObjects;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Ghost;
using Content.Server.Goobstation.Ghostbar.Components;
using Content.Server.Mind;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Prototypes;
using Content.Shared.Roles.Jobs;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Inventory;
using Content.Shared.Temperature.Components;
using Content.Server.Body.Components;
using Content.Server.Atmos.Components;
using Content.Shared.Mindshield.Components;
using Content.Server.Antag.Components;

namespace Content.Server.Goobstation.Ghostbar;

public sealed class GhostBarSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly StationSpawningSystem _spawningSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private static readonly List<String> _jobPrototypes = new()
    {
        "Passenger",
        "Bartender",
        "Botanist",
        "Chef",
        "Janitor"
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
        SubscribeNetworkEvent<GhostBarSpawnEvent>(SpawnPlayer);
    }

    private readonly ResPath _mapPath = new("Maps/Floof/Nonstation/Ghostbar/ghostbar.yml");

    private void OnRoundStart(RoundStartingEvent ev)
    {
        if (_mapLoader.TryLoadMap(_mapPath, out var map, out _, new DeserializationOptions { InitializeMaps = true }))
            _mapSystem.SetPaused(map.Value.Comp.MapId, false);
    }

    public void SpawnPlayer(GhostBarSpawnEvent msg, EntitySessionEventArgs args)
    {
        if (!_entityManager.HasComponent<GhostComponent>(args.SenderSession.AttachedEntity))
        {
            Log.Warning($"User {args.SenderSession.Name} tried to spawn at ghost bar without being a ghost.");
            return;
        }

        var spawnPoints = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<GhostBarSpawnComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            spawnPoints.Add(_entityManager.GetComponent<TransformComponent>(ent).Coordinates);
        }

        if (spawnPoints.Count == 0)
        {
            Log.Warning("No spawn points found for ghost bar.");
            return;
        }


        var randomSpawnPoint = _random.Pick(spawnPoints);
        var randomJob = _random.Pick(_jobPrototypes);
        var profile = _ticker.GetPlayerProfile(args.SenderSession);
        var mobUid = _spawningSystem.SpawnPlayerMob(randomSpawnPoint, randomJob, profile, null);

        RemComp<TemperatureComponent>(mobUid);
        RemComp<RespiratorComponent>(mobUid);
        RemComp<BarotraumaComponent>(mobUid);
        _entityManager.EnsureComponent<MindShieldComponent>(mobUid);
        _entityManager.EnsureComponent<AntagImmuneComponent>(mobUid);


        var targetMind = _mindSystem.GetMind(args.SenderSession.UserId);


        if (targetMind != null)
        {
            _mindSystem.TransferTo(targetMind.Value, mobUid, true);
        }
    }

}
