using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Prefabs;
using unnamed.Resources;
using unnamed.Utils.Health;

namespace unnamed.systems;

public class EntityCollisionDetectSystem : BaseSystem
{
    private static readonly Query ProjectileQuery = new QueryBuilder()
        .With<Projectile>()
        .With<EntityCollisionBehavior>()
        .With<Position>()
        .Build();

    private static readonly Query EnemyQuery = new QueryBuilder()
        .With<Enemy>().With<EntityStats>().Build();

    public override void Run(World world)
    {
        ref DeltaTime dt = ref world.GetResource<DeltaTime>();

        foreach (Entity e in ProjectileQuery.AsEnumerator(world))
        {
            EntityHandle projectile = world.Handle(e);

            ref Position projectilePos = ref projectile.Get<Position>();
            ref EntityCollisionBehavior collisionBehavior = ref projectile.Get<EntityCollisionBehavior>();
            ref Projectile projectileInfo = ref projectile.Get<Projectile>();

            if (projectile.Has<CollisionCooldown>())
            {
                projectile.Get<CollisionCooldown>().RemainingTime -= dt;
                if (projectile.Get<CollisionCooldown>().RemainingTime <= 0f)
                {
                    projectile.Remove<CollisionCooldown>();
                }
                else
                {
                    return;
                }
            }

            foreach (Entity enemy in EnemyQuery.AsEnumerator(world))
            {
                EntityHandle enemyHandle = world.Handle(enemy);

                ref Position enemyPos = ref enemyHandle.Get<Position>();
                Position distance = enemyPos - projectilePos;

                if (!(distance.LengthFast() <= 1f))
                {
                    continue;
                }

                if (collisionBehavior.Explode())
                {
                    PrefabFactory.CreateExplosion(world, projectilePos,
                        projectile.Get<Transform>().Height, projectileInfo.ExplosionAnimation);
                }

                if (collisionBehavior.DestroySelf())
                {
                    projectile.Add(new MarkedToDestroy());
                }
                else
                {
                    projectile.Add(new CollisionCooldown(0.5f));
                }

                enemyHandle.Add(new Collided());
                enemyHandle.AddDamage(projectileInfo.Damage);
            }
        }
    }
}