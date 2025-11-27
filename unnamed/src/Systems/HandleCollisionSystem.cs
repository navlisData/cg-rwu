using System.Diagnostics;

using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Texture;

namespace unnamed.systems;

public class HandleCollisionSystem(World world)
    : EntitySetSystem<(float dt, ActionControlHandler<EnemyAction> actionHandler, IAssetStore assetStore)>(world,
        world.Query()
            .With<Collided>()
            .With<EntityStats>()
            .With<EnemyActionState>()
            .Build()
    )
{
    protected override void Update(
        (float dt, ActionControlHandler<EnemyAction> actionHandler, IAssetStore assetStore) args, in Entity e)
    {
        ref EntityStats stats = ref e.Get<EntityStats>();

        if (e.Has<Enemy>())
        {
            Debug.Assert(e.Has<NonDirectionalCharacter>());
            ref EnemyActionState enemyState = ref e.Get<EnemyActionState>();
            ref NonDirectionalCharacter nonDirectionalCharacter = ref e.Get<NonDirectionalCharacter>();

            if (stats.Hitpoints <= 0)
            {
                e.Add(new MarkedToDestroy());
            }
            else
            {
                var clip = args.assetStore.Get(GameAssets.Enemy.Slime1.Damage);
                EnemyAction currentState = args.actionHandler.TryUpdateAction(
                    ref enemyState.CurrentAction,
                    ref enemyState.RemainingTime,
                    desiredAction: EnemyAction.Damage,
                    clip.AnimationDuration(),
                    out bool _
                );
                nonDirectionalCharacter.ActionIndex = (byte)currentState;
            }
        }

        e.Remove<Collided>();
    }
}