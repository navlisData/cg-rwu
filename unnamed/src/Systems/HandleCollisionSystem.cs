using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Map;
using unnamed.Components.Tags;

namespace unnamed.systems;

public class HandleCollisionSystem(World world) : EntitySetSystem<float>(world,
    world.Query()
        .With<Collided>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        if (e.Has<Enemy>())
        {
            e.Add(new MarkedToDestroy());
        }
    }
}