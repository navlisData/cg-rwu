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
        ref Position position = ref e.Get<Position>();
        ref Velocity velocity = ref e.Get<Velocity>();
        position += (velocity.Value * dt);
    }
}