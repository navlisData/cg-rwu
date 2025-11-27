using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Tags;

namespace unnamed.systems;

public class HandleCollisionSystem(World world) : EntitySetSystem<float>(world,
    world.Query()
        .With<Collided>()
        .With<EntityStats>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref EntityStats stats = ref e.Get<EntityStats>();

        if (e.Has<Enemy>())
        {
            if (stats.Hitpoints <= 0)
            {
                e.Add(new MarkedToDestroy());
            }
        }
    }
}