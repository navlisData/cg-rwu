using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.systems;

public class PlayerEnemyCollisionSystem(World world, IAssetStore assetStore)
    : ExtendedEntitySetSystem<(Entity player, float dt), Entity>(world,
        world.Query()
            .With<Enemy>()
            .With<Position>()
            .With<EntityStats>()
            .Without<Sleeping>()
            .Build()
    )
{
    protected override void BeforeUpdate((Entity player, float dt) args)
    {
        EntityHandle playerHandle = this.world.Handle(args.player);
        ;
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

        Position distance = playerPos - enemyPos;

        if (distance.LengthFast() > enemyStats.AttackRange)
        {
            return;
        }

        playerHandle.Add(new Collided());
        handle.Add(new DoAttack());

        this.doUpdate = false;
    }
}