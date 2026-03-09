using Engine.Ecs;

using unnamed.Components.Physics;
using unnamed.Components.UI;
using unnamed.Enums;

namespace unnamed.Components.General;

public struct Spawner(
    float spawnTimeMax,
    int spawnOdds,
    int spawnAmount,
    float spawnRadius,
    Func<World, Position, Entity>? spawnEntity,
    Func<World, AbsolutePosition, Entity>? spawnEntityA,
    TileFlags? restrictSpawnLocations)
{
    public readonly Func<World, Position, Entity>? SpawnEntity = spawnEntity;
    public readonly Func<World, AbsolutePosition, Entity>? SpawnEntityA = spawnEntityA;
    public readonly float SpawnTimeMax = spawnTimeMax;
    public readonly int SpawnOdds = spawnOdds;
    public readonly int SpawnAmount = spawnAmount;
    public readonly float SpawnRadius = spawnRadius;
    public readonly TileFlags? RestrictSpawnLocations = restrictSpawnLocations;

    public float SpawnTime = 0;
}