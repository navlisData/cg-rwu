using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Resources;

namespace unnamed.systems;

public sealed class EnemyControlSystem()
    : EntitySetSystem<DeltaTime, ActionControlHandler<EnemyAction>>(
        new QueryBuilder()
            .With<Enemy>()
            .With<NonDirectionalCharacter>()
            .With<EnemyActionState>()
            .Build()
    )
{
    protected override void Update(ref DeltaTime dt, ref ActionControlHandler<EnemyAction> actionHandler,
        EntityHandle e)
    {
        ref NonDirectionalCharacter nonDirectionalCharacter = ref e.Get<NonDirectionalCharacter>();
        ref EnemyActionState enemyState = ref e.Get<EnemyActionState>();

        EnemyAction action = e.Has<Velocity>()
            ? EnemyAction.Move
            : EnemyAction.Idle;

        EnemyAction currentState = actionHandler.TryUpdateAction(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            action,
            out bool _
        );

        nonDirectionalCharacter.ActionIndex = (byte)currentState;
        actionHandler.Sync(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            dt);
    }
}