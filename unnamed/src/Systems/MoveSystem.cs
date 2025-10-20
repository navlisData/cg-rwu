using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.systems;

public sealed class MoveSystem(World world) : EntitySetSystem<float>(world, world.Query()
    .With<Position>()
    .With<Velocity>()
    .Without<Sleeping>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref var position = ref e.Get<Position>();
        ref var velocity = ref e.Get<Velocity>();
        position.Value += velocity.Value * dt;
    }
}