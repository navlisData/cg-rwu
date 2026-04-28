using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Resources;

namespace unnamed.Systems;

public sealed class CameraSystem : BaseSystem
{
    private static readonly Query PlayerQuery = new QueryBuilder().With<Player>().Build();

    public override void Run(World world)
    {
        ref Camera2D camera = ref world.GetResource<Camera2D>();
        ref DeltaTime dt = ref world.GetResource<DeltaTime>();

        Entity player = PlayerQuery.Single(world);
        Position playerPos = world.Get<Position>(player);

        camera.Position = Position.Lerp(camera.Position, playerPos, 5f * dt.Value);

        Vector2 cameraPos = camera.Position.ToWorldPosition();

        float halfHeight = camera.OrthographicSize * 0.5f / camera.Zoom;
        float aspectRatio = camera.Viewport.X / (float)camera.Viewport.Y;
        float halfWidth = halfHeight * aspectRatio;

        camera.View =
            Matrix4.CreateTranslation(-cameraPos.X, -cameraPos.Y, 0f) *
            Matrix4.CreateRotationZ(-camera.Rotation);

        camera.Projection = Matrix4.CreateOrthographicOffCenter(
            -halfWidth, halfWidth,
            -halfHeight, halfHeight,
            -1f, 1f
        );

        camera.ViewProjection = camera.View * camera.Projection;
    }
}