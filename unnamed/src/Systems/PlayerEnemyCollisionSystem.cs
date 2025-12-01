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

public class PlayerEnemyCollisionSystem(
    World world,
    IAssetStore assetStore,
    ActionControlHandler<EnemyAction> actionHandler)
    : ExtendedEntitySetSystem<(Entity player, float dt), Entity>(world,
        world.Query()
            .With<Enemy>()
            .With<Position>()
            .With<EntityStats>()
            .Without<MarkedToDestroy>()
            .Without<Sleeping>()
            .Build()
    )
{
    protected override void BeforeUpdate((Entity player, float dt) args)
    {
        EntityHandle playerHandle = this.world.Handle(args.player);
        float dt = args.dt;

        if (!playerHandle.Has<Invincible>())
        {
            return;
        }

        playerHandle.Get<Invincible>().RemainingTime -= dt;

        if (playerHandle.Get<Invincible>().RemainingTime <= 0f)
        {
            playerHandle.Remove<Invincible>();
        }
        else
        {
            this.doUpdate = false;
        }
    }

    protected override void Update(Entity player, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);
        EntityHandle playerHandle = this.world.Handle(player);

        ref Position playerPos = ref playerHandle.Get<Position>();
        ref Position enemyPos = ref handle.Get<Position>();
        ref EntityStats enemyStats = ref handle.Get<EntityStats>();
        ref EnemyActionState enemyState = ref handle.Get<EnemyActionState>();
        ref NonDirectionalCharacter nonDirectionalCharacter = ref handle.Get<NonDirectionalCharacter>();

        Position distance = playerPos - enemyPos;

        if (distance.LengthFast() > enemyStats.AttackRange)
        {
            return;
        }

        playerHandle.Add(new Collided { CollidedWith = e });

        AnimationClip clip = assetStore.Get(GameAssets.Enemy.Slime1.Attack);
        EnemyAction currentState = actionHandler.TryUpdateAction(
            ref enemyState.CurrentAction,
            ref enemyState.RemainingTime,
            EnemyAction.Attack,
            clip.AnimationDuration(),
            out bool _
        );
        handle.Add(new DoAttack { DamageIn = clip.AnimationDuration() });
        handle.Add(new MarkedToDestroy { RemainingLifetime = clip.AnimationDuration() });
        handle.Remove<Follows>();
        handle.Remove<Velocity>();

        nonDirectionalCharacter.ActionIndex = (byte)currentState;
        this.doUpdate = false;
    }
}