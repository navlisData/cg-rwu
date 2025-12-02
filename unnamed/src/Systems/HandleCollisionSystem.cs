using System.Diagnostics;

using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.Drops;
using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Texture;
using unnamed.Utils.Health;

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

        try
        {
            ref var stats = ref handle.Get<EntityStats>();
            ref var collided = ref handle.Get<Collided>();

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
                if (!this.world.IsAlive(collided.CollidedWith))
                {
                    handle.Remove<Collided>();
                    return;
                }

                EntityHandle collidedEntityHandle = this.world.Handle(collided.CollidedWith);
                if (collidedEntityHandle.Has<DoAttack>())
                {
                    this.HandleAttack(handle, ref stats);
                    return;
                }

                if (collidedEntityHandle.Has<DropType>())
                {
                    this.HandleDropPickup(handle, collidedEntityHandle);
                    return;
                }
            }
        }
        finally
        {
            handle.Remove<Collided>();
        }
    }

    private void HandleAttack(EntityHandle playerHandle, ref EntityStats stats)
    {
        playerHandle.AddDamage(1);

        if (stats.Hitpoints <= 0)
        {
            // TODO: End game?
        }
    }

    private void HandleDropPickup(EntityHandle playerHandle, EntityHandle collidedEntityHandle)
    {
        ref DropType type = ref collidedEntityHandle.Get<DropType>();

        switch (type)
        {
            case DropType.MaxHealthDrop:
                int deltaMaxUnits = collidedEntityHandle.Get<MaxHealthDrop>().MaxHealthDeltaUnits;
                playerHandle.AddMaxHealthUnits(deltaMaxUnits);
                break;
            case DropType.UpdateHealthDrop:
                int deltaUnits = collidedEntityHandle.Get<UpdateHealthDrop>().HealthDeltaUnits;
                playerHandle.AddHealthUnits(deltaUnits);
                break;
        }

        collidedEntityHandle.Add(new MarkedToDestroy());
    }
}