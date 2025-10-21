using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

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
        ref Vector2 selfPosition = ref e.Get<Position>().Value;
        ref Vector2 targetPosition = ref target.Get<Position>().Value;

        selfPosition = Vector2.Lerp(selfPosition, targetPosition, speed * dt);
    }
}