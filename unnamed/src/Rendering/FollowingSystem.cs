using System.Diagnostics;

using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;

namespace unnamed.Rendering;

public sealed class FollowingSystem(World world)
    : EntitySetSystem<(float dt, ActionControlHandler<EnemyAction> actionHandler)>(world, world.Query()
        .With<Follows>()
        .With<Position>()
        .Without<Sleeping>()
        .Build()
    )
{
    protected override void Update((float dt, ActionControlHandler<EnemyAction> actionHandler) args, in Entity e)
    {
        ref Entity target = ref e.Get<Follows>().Target;
        ref Position selfPosition = ref e.Get<Position>();
        ref Position targetPosition = ref target.Get<Position>();
        ref Follows follows = ref e.Get<Follows>();

        Position positionDifference = targetPosition - selfPosition;
        float distance = positionDifference.LengthFast();

        bool outOfRange = distance > follows.FollowRadius;
        if (e.Has<Enemy>())
        {
            UpdateEnemyState(outOfRange, ref args.actionHandler, e);
        }

        if (outOfRange) return;

        switch (follows.Type)
        {
            case FollowType.Linear:
                e.Add(new Velocity { Direction = positionDifference.NormalizeFast(), Speed = follows.Speed });
                break;
            case FollowType.Lerp:
                selfPosition = Position.Lerp(selfPosition, targetPosition, follows.Speed * args.dt);
                break;
            default:
                Debug.Fail($"Unknown follow type {follows.Type}");
                break;
        }
    }

    private void UpdateEnemyState(bool outOfRange, ref ActionControlHandler<EnemyAction> actionHandler, Entity entity)
    {
        ref NonDirectionalCharacter nonDirectionalCharacter = ref entity.Get<NonDirectionalCharacter>();
        ref EnemyActionState enemyState = ref entity.Get<EnemyActionState>();

        EnemyAction currentState = actionHandler.TryUpdateAction(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            desiredAction: outOfRange ? EnemyAction.Idle : EnemyAction.Move,
            out bool _
        );
        nonDirectionalCharacter.ActionIndex = (byte)currentState;
    }
}