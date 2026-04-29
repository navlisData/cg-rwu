using System.Diagnostics;

using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Querying;

using engine.Ecs.State;

using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.Drops;
using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Resources;
using unnamed.Texture;
using unnamed.Utils;
using unnamed.Utils.Health;
using unnamed.Utils.Loot;

namespace unnamed.systems;

public class HandleCollisionSystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<Collided>()
        .Build();

    private static readonly Query PlayerQuery = new QueryBuilder()
        .With<Player>()
        .Build();

    public override void Run(World world)
    {
        ref DeltaTime dt = ref world.GetResource<DeltaTime>();
        ref ActionControlHandler<EnemyAction> ach = ref world.GetResource<ActionControlHandler<EnemyAction>>();
        ref AssetStore assetStore = ref world.GetResource<AssetStore>();
        ref State<GameState> gameState = ref world.GetState<GameState>();

        foreach (Entity e in Query.AsEnumerator(world))
        {
            Update(world, ref dt, ref ach, ref assetStore, ref gameState, world.Handle(e));
        }
    }

    private static void Update(World world, ref DeltaTime dt, ref ActionControlHandler<EnemyAction> actionHandler,
        ref AssetStore assetStore, ref State<GameState> gameState, EntityHandle e)
    {
        try
        {
            ref Collided collided = ref e.Get<Collided>();

            if (e.Has<Enemy>())
            {
                Debug.Assert(e.Has<EnemyActionState>());
                Debug.Assert(e.Has<NonDirectionalCharacter>());
                Debug.Assert(e.Has<EntityStats>());

                ref EntityStats stats = ref e.Get<EntityStats>();
                ref EnemyActionState enemyState = ref e.Get<EnemyActionState>();
                ref NonDirectionalCharacter nonDirectionalCharacter = ref e.Get<NonDirectionalCharacter>();

                if (stats.Hitpoints <= 0)
                {
                    e.Add(new MarkedToDestroy());
                    e.Add(LootTableProvider.SlimeLootTable);

                    if (PlayerQuery.TrySingle(world, out Entity player))
                    {
                        world.IncreasePlayerScore(player, stats.ScoreReward);
                    }
                }
                else
                {
                    AnimationClip clip = assetStore.Get(GameAssets.Enemy.Slime1.Damage);
                    EnemyAction currentState = actionHandler.TryUpdateAction(
                        ref enemyState.CurrentAction,
                        ref enemyState.RemainingTime,
                        EnemyAction.Damage,
                        clip.AnimationDuration(),
                        out bool _
                    );
                    nonDirectionalCharacter.ActionIndex = (byte)currentState;

                    if (e.Has<Follows>() && !e.Has<Velocity>())
                    {
                        ref Follows follows = ref e.Get<Follows>();

                        if (PlayerQuery.TrySingle(world, out Entity player))
                        {
                            ref Position playerPos = ref world.Get<Position>(player);
                            follows.FollowRadius = (playerPos - e.Get<Position>()).LengthFast();
                        }
                    }
                }
            }

            if (e.Has<Player>())
            {
                Debug.Assert(e.Has<EntityStats>());
                ref EntityStats stats = ref e.Get<EntityStats>();

                if (!world.IsAlive(collided.CollidedWith))
                {
                    e.Remove<Collided>();
                    return;
                }

                EntityHandle collidedEntityHandle = world.Handle(collided.CollidedWith);
                if (collidedEntityHandle.Has<DoAttack>())
                {
                    HandleAttack(e, ref stats, ref gameState);
                    return;
                }

                if (collidedEntityHandle.Has<DropType>())
                {
                    HandleDropPickup(e, collidedEntityHandle);
                    return;
                }
            }

            if (e.Has<TriggerStageEnd>())
            {
                HandleWinCountdown(e, dt, ref gameState, world);
            }
        }
        finally
        {
            e.Remove<Collided>();
        }
    }

    private static void HandleWinCountdown(EntityHandle handle, float dt, ref State<GameState> gameState,
        World world)
    {
        ref TriggerStageEnd stageEnd = ref handle.Get<TriggerStageEnd>();
        ref Entity player = ref handle.Get<Collided>().CollidedWith;

        stageEnd.TimeRemaining -= dt;
        if (!(stageEnd.TimeRemaining <= 0))
        {
            return;
        }

        world.Remove<VisibleEntity>(player);
        gameState.QueueChange(GameState.Won);
    }

    private static void HandleAttack(EntityHandle playerHandle, ref EntityStats stats,
        ref State<GameState> gameState)
    {
        playerHandle.AddDamage(1);

        if (stats.Hitpoints <= 0)
        {
            gameState.QueueChange(GameState.Lost);
        }
    }

    private static void HandleDropPickup(EntityHandle playerHandle, EntityHandle collidedEntityHandle)
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