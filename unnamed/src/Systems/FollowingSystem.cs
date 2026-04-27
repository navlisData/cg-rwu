using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.Systems;

public sealed class FollowingSystem(World world) : EntitySetSystem<float>(world, new QueryBuilder()
    .With<Follows>()
    .With<Position>()
    .Without<Sleeping>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Entity target = ref handle.Get<Follows>().Target;
        ref Position selfPosition = ref handle.Get<Position>();
        ref Position targetPosition = ref this.world.Get<Position>(target);
        ref Follows follows = ref handle.Get<Follows>();

        Position positionDifference = targetPosition - selfPosition;
        float distance = positionDifference.LengthFast();

        if (distance > follows.FollowRadius)
        {
            handle.Remove<Velocity>();
            return;
        }

        handle.Add(new Velocity { Direction = positionDifference.NormalizeFast(), Speed = follows.Speed });
    }
}