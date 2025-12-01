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
        EntityHandle handle = this.world.Handle(e);

        ref Entity target = ref handle.Get<Follows>().Target;
        ref Position selfPosition = ref handle.Get<Position>();
        ref Position targetPosition = ref this.world.Get<Position>(target);
        ref Follows follows = ref handle.Get<Follows>();

        Position positionDifference = targetPosition - selfPosition;
        float distance = positionDifference.LengthFast();

        if (distance > follows.FollowRadius)
        {
            switch (follows.Type)
            {
                case FollowType.Linear:
                    handle.Remove<Velocity>();
                    break;
                case FollowType.Lerp:
                    // Shouldn't happen, as the lerp following directly updates the Position and is only really useful globally 
                    Debug.WriteLine("Lerp following is out of range of target");
                    break;
                default:
                    Debug.Fail($"Unknown follow type {follows.Type}");
                    break;
            }

            return;
        }

        switch (follows.Type)
        {
            case FollowType.Linear:
                handle.Add(new Velocity { Direction = positionDifference.NormalizeFast(), Speed = follows.Speed });
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