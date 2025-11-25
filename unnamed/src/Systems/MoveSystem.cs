using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.GameMap;

namespace unnamed.systems;

public sealed class MoveSystem(World world, Map map) : EntitySetSystem<float>(world, world.Query()
    .With<Position>()
    .With<Velocity>()
    .Without<Sleeping>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref Position position = ref e.Get<Position>();
        ref Velocity velocity = ref e.Get<Velocity>();
        Position newPosition = position + (velocity.Value * dt);

        if (map.IsWallAt(newPosition)) { return; }

        position = newPosition;
    }
}