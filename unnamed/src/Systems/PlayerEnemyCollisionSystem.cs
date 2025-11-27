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
        Entity player = args.player;
        float dt = args.dt;

        if (!player.Has<Invincible>())
        {
            return;
        }

        player.Get<Invincible>().RemainingTime -= dt;

        if (player.Get<Invincible>().RemainingTime <= 0f)
        {
            player.Remove<Invincible>();
        }
        else
        {
            this.doUpdate = false;
        }
    }

    protected override void Update(Entity player, in Entity e)
    {
        ref Position playerPos = ref player.Get<Position>();
        ref Position enemyPos = ref e.Get<Position>();
        ref EntityStats enemyStats = ref e.Get<EntityStats>();

        Position distance = playerPos - enemyPos;

        if (distance.LengthFast() > enemyStats.AttackRange)
        {
            return;
        }

        player.Add(new Collided());
        e.Add(new DoAttack());

        this.doUpdate = false;
    }
}