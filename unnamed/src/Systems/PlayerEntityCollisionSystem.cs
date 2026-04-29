using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Texture;

namespace unnamed.systems;

public class PlayerEntityCollisionSystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<CanCollideWithPlayer>()
        .With<Position>()
        .Without<MarkedToDestroy>()
        .Without<Sleeping>()
        .Build();

    private static readonly Query PlayerQuery = new QueryBuilder()
        .With<Player>().Build();

    public override void Run(World world)
    {
        ref AssetStore assetStore = ref world.GetResource<AssetStore>();
        ref ActionControlHandler<EnemyAction> ach = ref world.GetResource<ActionControlHandler<EnemyAction>>();

        Entity player = PlayerQuery.Single(world);

        foreach (Entity e in Query.AsEnumerator(world))
        {
            Update(ref ach, ref assetStore, world.Handle(player), world.Handle(e));
        }
    }

    private static void Update(ref ActionControlHandler<EnemyAction> actionHandlerEntityHandle,
        ref AssetStore assetStore, EntityHandle player, EntityHandle e)
    {
        ref Position playerPos = ref player.Get<Position>();
        ref Position entityPos = ref e.Get<Position>();

        Position distance = playerPos - entityPos;
        float collisionRange = e.Get<CanCollideWithPlayer>().Range;
        if (distance.LengthFast() > collisionRange)
        {
            return;
        }

        player.Add(new Collided { CollidedWith = e.ToEntity() });

        if (e.Has<Enemy>())
        {
            EnemyCollision(e, ref assetStore, ref actionHandlerEntityHandle);
        }
        else
        {
            e.Add(new Collided { CollidedWith = player.ToEntity() });
        }
    }

    private static void EnemyCollision(EntityHandle enemyHandle, ref AssetStore assetStore,
        ref ActionControlHandler<EnemyAction> actionHandler)
    {
        ref EnemyActionState enemyState = ref enemyHandle.Get<EnemyActionState>();
        ref NonDirectionalCharacter nonDirectionalCharacter = ref enemyHandle.Get<NonDirectionalCharacter>();

        //TODO: Check if this makes sense to be hardcoded here
        AnimationClip clip = assetStore.Get(GameAssets.Enemy.Slime1.Attack);
        EnemyAction currentState = actionHandler.TryUpdateAction(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            EnemyAction.Attack,
            clip.AnimationDuration(),
            out bool success
        );

        if (success)
        {
            ref Transform transform = ref enemyHandle.Get<Transform>();
            transform.Scale *= 1.3f;

            enemyHandle.Add(new DoAttack());
            enemyHandle.Add(new MarkedToDestroy { RemainingLifetime = clip.AnimationDuration() });
            enemyHandle.Remove<Follows>();
            enemyHandle.Remove<Velocity>();
        }

        nonDirectionalCharacter.ActionIndex = (byte)currentState;
    }
}