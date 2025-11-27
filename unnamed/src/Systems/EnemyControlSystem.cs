using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;

namespace unnamed.systems;

public sealed class EnemyControlSystem(World world)
    : EntitySetSystem<(float dt, ActionControlHandler<EnemyAction> actionHandler)>(world,
        world.Query()
            .With<Enemy>()
            .With<NonDirectionalCharacter>()
            .With<EnemyActionState>()
            .Build()
    )
{
    protected override void Update((float dt, ActionControlHandler<EnemyAction> actionHandler) args, in Entity e)
    {
        ref NonDirectionalCharacter nonDirectionalCharacter = ref e.Get<NonDirectionalCharacter>();
        ref EnemyActionState enemyState = ref e.Get<EnemyActionState>();

        EnemyAction action = e.Has<Velocity>()
            ? EnemyAction.Move
            : EnemyAction.Idle;

        EnemyAction currentState = args.actionHandler.TryUpdateAction(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            desiredAction: action,
            out bool _
        );

        nonDirectionalCharacter.ActionIndex = (byte)currentState;
        args.actionHandler.Sync(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            args.dt);
    }
}