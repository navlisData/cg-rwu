using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Map;

namespace unnamed.systems;

public sealed class OwnerCleanupSystem(World world) : EntitySetSystem<float>(world,
    world.Query()
        .With<OwnedBy>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);
        ref Entity owner = ref handle.Get<OwnedBy>().Owner;

        if (!this.world.IsAlive(owner))
        {
            handle.Add(new MarkedToDestroy());
        }
    }
}