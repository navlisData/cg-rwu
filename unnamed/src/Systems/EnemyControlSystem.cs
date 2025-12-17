using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;

namespace unnamed.systems;

public sealed class EnemyControlSystem(World world)
    : EntitySetSystem<(float dt, ActionControlHandler<EnemyAction> actionHandler)>(world,
        new QueryBuilder()
            .With<Enemy>()
            .With<NonDirectionalCharacter>()
            .With<EnemyActionState>()
            .Build()
    )
{
    protected override void Update((float dt, ActionControlHandler<EnemyAction> actionHandler) args, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref NonDirectionalCharacter nonDirectionalCharacter = ref handle.Get<NonDirectionalCharacter>();
        ref EnemyActionState enemyState = ref handle.Get<EnemyActionState>();

        EnemyAction action = handle.Has<Velocity>()
            ? EnemyAction.Move
            : EnemyAction.Idle;

        EnemyAction currentState = args.actionHandler.TryUpdateAction(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            action,
            out bool _
        );

        nonDirectionalCharacter.ActionIndex = (byte)currentState;
        args.actionHandler.Sync(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            args.dt);
    }
}