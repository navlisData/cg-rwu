using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.systems;

public class EntityCollisionDetectSystem(World world) : EntitySetSystem<float>(world,
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
            // if (projectilePos - enemyPos <= new Position())
            // {
            //     
            // }
        }
    }
}