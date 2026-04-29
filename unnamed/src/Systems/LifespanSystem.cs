using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Resources;

namespace unnamed.systems;

public sealed class LifespanSystem()
    : EntitySetSystem<DeltaTime>(
        new QueryBuilder()
            .With<Lifespan>()
            .Build()
    )
{
    protected override void Update(ref DeltaTime dt, EntityHandle e)
    {
        ref Lifespan lifespan = ref e.Get<Lifespan>();

        lifespan.Current += dt;
        if (lifespan.Current > lifespan.Max)
        {
            e.Add<MarkedToDestroy>();
        }
    }
}