using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.Systems;

public sealed class FollowingSystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<Follows>()
        .With<Position>()
        .Without<Sleeping>()
        .Build();

    public override void Run(World world)
    {
        foreach (Entity e in new QueryBuilder()
                     .With<Follows>()
                     .With<Position>()
                     .Without<Sleeping>()
                     .Build().AsEnumerator(world))
        {
            EntityHandle handle = world.Handle(e);

            ref Follows follows = ref handle.Get<Follows>();
            Entity target = follows.Target;

            ref Position selfPosition = ref handle.Get<Position>();
            ref Position targetPosition = ref world.Get<Position>(target);

            Position positionDifference = targetPosition - selfPosition;
            float distance = positionDifference.LengthFast();

            if (distance > follows.FollowRadius)
            {
                handle.Remove<Velocity>();
                continue;
            }

            handle.Add(new Velocity { Direction = positionDifference.NormalizeFast(), Speed = follows.Speed });
        }
    }
}