using engine.Control;

using Engine.Ecs;
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

public class PlayerEntityCollisionSystem(
    World world,
    IAssetStore assetStore,
    ActionControlHandler<EnemyAction> actionHandler)
    : EntitySetSystem<Entity>(world,
        world.Query()
            .With<CanCollideWithPlayer>()
            .With<Position>()
            .With<EntityStats>()
            .Without<MarkedToDestroy>()
            .Without<Sleeping>()
            .Build()
    )
{
    protected override void Update(Entity player, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);
        EntityHandle playerHandle = this.world.Handle(player);

        ref Position playerPos = ref playerHandle.Get<Position>();
        ref Position entityPos = ref handle.Get<Position>();

        Position distance = playerPos - entityPos;
        float collisionRange = handle.Get<CanCollideWithPlayer>().Range;
        if (distance.LengthFast() > collisionRange)
        {
            return;
        }

        playerHandle.Add(new Collided { CollidedWith = e });

        if (handle.Has<Enemy>())
        {
            EnemyCollision(handle);
        }
    }

    private void EnemyCollision(EntityHandle enemyHandle)
    {
        ref EnemyActionState enemyState = ref enemyHandle.Get<EnemyActionState>();
        ref NonDirectionalCharacter nonDirectionalCharacter = ref enemyHandle.Get<NonDirectionalCharacter>();

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