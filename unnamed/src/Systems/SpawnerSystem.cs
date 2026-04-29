using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.UI;
using unnamed.GameMap;
using unnamed.Resources;

namespace unnamed.systems;

public sealed class SpawnerSystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<Spawner>()
        .WithAny<Position, AbsolutePosition>()
        .Build();

    public override void Run(World world)
    {
        ref DeltaTime dt = ref world.GetResource<DeltaTime>();
        ref Map map = ref world.GetResource<Map>();

        foreach (Entity e in Query.AsEnumerator(world))
        {
            Update(world, ref dt, ref map, world.Handle(e));
        }
    }

    private static void Update(World world, ref DeltaTime dt, ref Map map, EntityHandle e)
    {
        ref Spawner spawner = ref e.Get<Spawner>();

        spawner.SpawnTime += dt;
        if (spawner.SpawnTime < spawner.SpawnTimeMax)
        {
            return;
        }

        Random rng = Random.Shared;

        if (e.Has<Position>())
        {
            Position position = e.Get<Position>();

            if (spawner.SpawnEntity == null)
            {
                return;
            }

            for (int i = 0; i < spawner.SpawnAmount; i++)
            {
                if (rng.NextSingle() >= spawner.SpawnOdds)
                {
                    continue;
                }

                Vector2 variance = new((rng.NextSingle() * 2 * spawner.SpawnRadius) - spawner.SpawnRadius,
                    (rng.NextSingle() * 2 * spawner.SpawnRadius) - spawner.SpawnRadius);
                position += variance;

                if (spawner.AllowedSpawnLocations.HasValue)
                {
                    Tile? tile = map.GetTileAt(world, position);
                    if (!tile.HasValue || !((spawner.AllowedSpawnLocations & tile.Value.Flags) > 0))
                    {
                        continue;
                    }
                }

                spawner.SpawnEntity(world, position);
            }
        }
        else
        {
            AbsolutePosition position = e.Get<AbsolutePosition>();

            if (spawner.SpawnEntityA == null)
            {
                return;
            }

            for (int i = 0; i < spawner.SpawnAmount; i++)
            {
                if (rng.NextSingle() >= spawner.SpawnOdds)
                {
                    continue;
                }

                Vector2 variance = new((rng.NextSingle() * 2 * spawner.SpawnRadius) - spawner.SpawnRadius,
                    (rng.NextSingle() * 2 * spawner.SpawnRadius) - spawner.SpawnRadius);
                position += variance;
                spawner.SpawnEntityA(world, new AbsolutePosition(position.X, position.Y, false));
            }
        }


        spawner.SpawnTime = 0f;
    }
}