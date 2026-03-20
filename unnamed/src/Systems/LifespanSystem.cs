using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Map;

namespace unnamed.systems;

public sealed class LifespanSystem(World world)
    : EntitySetSystem<float>(world,
        new QueryBuilder()
            .With<Lifespan>()
            .Build()
    )
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Lifespan lifespan = ref handle.Get<Lifespan>();

        lifespan.Current += dt;
        if (lifespan.Current > lifespan.Max)
        {
            handle.Add<MarkedToDestroy>();
        }
    }
}