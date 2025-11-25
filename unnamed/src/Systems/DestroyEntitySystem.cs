using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Tags;

namespace unnamed.systems;

public sealed class DestroyEntitySystem(World world) : EntitySetSystem<float>(world,
    world.Query()
        .With<MarkedToDestroy>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        this.world.DestroyEntity(e);
    }
}