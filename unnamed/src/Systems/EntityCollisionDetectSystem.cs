using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Prefabs;

namespace unnamed.systems;

public class EntityCollisionDetectSystem(World world, IAssetStore assetStore) : EntitySetSystem<float>(world,
    world.Query()
        .With<Projectile>()
        .With<EntityCollisionBehavior>()
        .With<Position>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref Position projectilePos = ref e.Get<Position>();
        ref EntityCollisionBehavior collisionBehavior = ref e.Get<EntityCollisionBehavior>();
        ref Projectile projectile = ref e.Get<Projectile>();

        if (e.Has<CollisionCooldown>())
        {
            e.Get<CollisionCooldown>().RemainingTime -= dt;
            if (e.Get<CollisionCooldown>().RemainingTime <= 0f)
            {
                e.Remove<CollisionCooldown>();
            }
            else
            {
                return;
            }
        }

        foreach (Entity enemy in this.world.Query().With<Enemy>().With<EntityStats>().Build().AsEnumerator(this.world))
        {
            ref Position enemyPos = ref enemy.Get<Position>();
            ref EntityStats enemyStats = ref enemy.Get<EntityStats>();
            Position distance = enemyPos - projectilePos;
            if (distance.LengthFast() <= 1f)
            {
                if (collisionBehavior.Explode())
                {
                    PrefabFactory.CreateExplosion(this.world, assetStore, projectilePos, e.Get<Transform>().Height,
                        e.Get<Projectile>().ExplosionAnimation);
                }

                if (collisionBehavior.DestroySelf())
                {
                    e.Add(new MarkedToDestroy());
                }
                else
                {
                    e.Add(new CollisionCooldown(0.5f));
                }

                enemy.Add(new Collided());
                enemyStats.Hitpoints -= projectile.Damage;
            }
        }
    }
}