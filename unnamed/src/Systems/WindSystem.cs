using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.UI;

namespace unnamed.systems;

public sealed class WindSystem(World world)
    : ExtendedEntitySetSystem<float, float>(world,
        new QueryBuilder()
            .With<InfluencedByWind>()
            .WithAny<Position, AbsolutePosition>()
            .Build()
    )
{
    private Vector2 windDirection = -Vector2.UnitY;
    private float windSpeed = 0.1f;

    protected override void BeforeUpdate(float context)
    {
        this.windDirection = this.windDirection;
        this.windSpeed = this.windSpeed;
    }

    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref InfluencedByWind wind = ref handle.Get<InfluencedByWind>();

        Vector2 movement = this.windDirection * this.windSpeed * wind.Drag;

        if (handle.Has<Position>())
        {
            ref Position position = ref handle.Get<Position>();
            position += movement;
        }
        else
        {
            ref AbsolutePosition position = ref handle.Get<AbsolutePosition>();
            position += movement;
        }
    }
}