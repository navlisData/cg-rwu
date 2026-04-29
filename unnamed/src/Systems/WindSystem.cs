using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.UI;

namespace unnamed.systems;

public sealed class WindSystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<InfluencedByWind>()
        .WithAny<Position, AbsolutePosition>()
        .Build();

    private Vector2 windDirection = -Vector2.UnitY;
    private float windSpeed = 0.1f;

    public override void Run(World world)
    {
        this.windDirection = this.windDirection;
        this.windSpeed = this.windSpeed;

        foreach (Entity e in Query.AsEnumerator(world))
        {
            EntityHandle handle = world.Handle(e);
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
}