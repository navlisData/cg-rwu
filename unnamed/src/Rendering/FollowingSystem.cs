using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public sealed class FollowingSystem(World world) : EntitySetSystem<float>(world, world.Query()
    .With<Follows>()
    .With<Position>()
    .Without<Sleeping>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref Entity target = ref e.Get<Follows>().Target;
        ref float speed = ref e.Get<Follows>().LerpSpeed;
        ref Position selfPosition = ref e.Get<Position>();
        ref Position targetPosition = ref target.Get<Position>();

        selfPosition = Position.Lerp(selfPosition, targetPosition, speed * dt);
    }
}