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
            .Build()
    )
{
    protected override void Update(
        (float dt, ActionControlHandler<EnemyAction> actionHandler, IAssetStore assetStore) args, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref EntityStats stats = ref handle.Get<EntityStats>();

        if (handle.Has<Enemy>())
        {
            Debug.Assert(handle.Has<EnemyActionState>());
            Debug.Assert(handle.Has<NonDirectionalCharacter>());

            ref EnemyActionState enemyState = ref handle.Get<EnemyActionState>();
            ref NonDirectionalCharacter nonDirectionalCharacter = ref handle.Get<NonDirectionalCharacter>();

            if (stats.Hitpoints <= 0)
            {
                handle.Add(new MarkedToDestroy());
            }
            else
            {
                AnimationClip clip = args.assetStore.Get(GameAssets.Enemy.Slime1.Damage);
                EnemyAction currentState = args.actionHandler.TryUpdateAction(
                    ref enemyState.CurrentAction,
                    ref enemyState.RemainingTime,
                    EnemyAction.Damage,
                    clip.AnimationDuration(),
                    out bool _
                );
                nonDirectionalCharacter.ActionIndex = (byte)currentState;
            }
        }

        if (handle.Has<Player>())
        {
            stats.Hitpoints -= 1;

            if (stats.Hitpoints <= 0)
            {
#if DEBUG
                Console.WriteLine("You died!");
#endif
                // TODO: End game?
            }
            else
            {
#if DEBUG
                Console.WriteLine($"Player HP remaining: {stats.Hitpoints}");
#endif
                handle.Add(new Invincible { RemainingTime = 1f });
            }
        }

        handle.Remove<Collided>();
    }
}