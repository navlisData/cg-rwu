using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Prefabs;

namespace unnamed.systems;

public class EntityCollisionDetectSystem(World world, IAssetStore assetStore) : EntitySetSystem<float>(world,
    world.Query()
        .With<Projectile>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref Position projectilePos = ref e.Get<Position>();

        foreach (var enemy in this.world.Query().With<Enemy>().Build().AsEnumerator(this.world))
        {
            ref Position enemyPos = ref enemy.Get<Position>();
            Position distance = enemyPos - projectilePos;
            if (distance.LengthFast() <= 1f)
            {
                enemy.Add(new Collided());
                PrefabFactory.CreateExplosion(this.world, assetStore, projectilePos, e.Get<Transform>().Height,
                    e.Get<Projectile>().ExplosionAnimation);
                e.Add(new MarkedToDestroy());
            }
        }
    }
}