using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Map;

namespace unnamed.systems;

public sealed class DestroyEntitySystem(World world) : EntitySetSystem<float>(world,
    world.Query()
        .With<MarkedToDestroy>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref MarkedToDestroy mtd = ref handle.Get<MarkedToDestroy>();

        mtd.RemainingLifetime -= dt;
        if (mtd.RemainingLifetime <= 0f)
        {
            this.world.DestroyEntity(e);
        }
    }
}