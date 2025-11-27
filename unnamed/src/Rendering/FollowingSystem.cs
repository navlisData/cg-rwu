using System.Diagnostics;

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
        ref Position selfPosition = ref e.Get<Position>();
        ref Position targetPosition = ref target.Get<Position>();
        ref Follows follows = ref e.Get<Follows>();

        Position positionDifference = targetPosition - selfPosition;
        float distance = positionDifference.LengthFast();

        if (distance > follows.FollowRadius) { return; }

        switch (follows.Type)
        {
            case FollowType.Linear:
                e.Add(new Velocity { Direction = positionDifference.NormalizeFast(), Speed = follows.Speed });
                break;
            case FollowType.Lerp:
                selfPosition = Position.Lerp(selfPosition, targetPosition, follows.Speed * dt);
                break;
            default:
                Debug.Fail($"Unknown follow type {follows.Type}");
                break;
        }
    }
}