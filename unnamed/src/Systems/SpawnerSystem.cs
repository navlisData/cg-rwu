using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.UI;

namespace unnamed.systems;

public sealed class SpawnerSystem(World world)
    : EntitySetSystem<float>(world,
        new QueryBuilder()
            .With<Spawner>()
            .WithAny<Position, AbsolutePosition>()
            .Build()
    )
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Spawner spawner = ref handle.Get<Spawner>();

        spawner.SpawnTime += dt;
        if (spawner.SpawnTime < spawner.SpawnTimeMax)
        {
            return;
        }

        Random rng = Random.Shared;

        if (handle.Has<Position>())
        {
            ref Position position = ref handle.Get<Position>();

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

                //TODO: implement behaviour so RestrictSpawnLocation works correctly, currently unused

                spawner.SpawnEntity(this.world, position);
            }
        }
        else
        {
            ref AbsolutePosition position = ref handle.Get<AbsolutePosition>();

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
                spawner.SpawnEntityA(this.world, position);
            }
        }


        spawner.SpawnTime = 0f;
    }
}