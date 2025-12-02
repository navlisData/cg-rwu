using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Prefabs;
using unnamed.Utils.Loot;

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
        EntityHandle handle = this.world.Handle(e);

        ref Position projectilePos = ref handle.Get<Position>();
        ref EntityCollisionBehavior collisionBehavior = ref handle.Get<EntityCollisionBehavior>();
        ref Projectile projectile = ref handle.Get<Projectile>();

        if (handle.Has<CollisionCooldown>())
        {
            handle.Get<CollisionCooldown>().RemainingTime -= dt;
            if (handle.Get<CollisionCooldown>().RemainingTime <= 0f)
            {
                handle.Remove<CollisionCooldown>();
            }
            else
            {
                return;
            }
        }

        foreach (Entity enemy in this.world.Query().With<Enemy>().With<EntityStats>().Build().AsEnumerator(this.world))
        {
            EntityHandle enemyHandle = this.world.Handle(enemy);

            ref Position enemyPos = ref enemyHandle.Get<Position>();
            ref EntityStats enemyStats = ref enemyHandle.Get<EntityStats>();
            Position distance = enemyPos - projectilePos;
            if (distance.LengthFast() <= 1f)
            {
                if (collisionBehavior.Explode())
                {
                    PrefabFactory.CreateExplosion(this.world, assetStore, projectilePos,
                        handle.Get<Transform>().Height, handle.Get<Projectile>().ExplosionAnimation);
                }

                if (collisionBehavior.DestroySelf())
                {
                    handle.Add(new MarkedToDestroy());
                    enemyHandle.Add(LootTableProvider.SlimeLootTable);
                }
                else
                {
                    handle.Add(new CollisionCooldown(0.5f));
                }

                enemyHandle.Add(new Collided());
                enemyStats.Hitpoints -= projectile.Damage;
            }
        }
    }
}