using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;

namespace unnamed.Rendering;

public sealed class CameraSystem(World world) : EntitySetSystem<float>(world, world.Query()
    .With<Camera2D>()
    .With<Position>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref Camera2D camera = ref e.Get<Camera2D>();
        Vector2 position = e.Get<Position>().ToWorldPosition();

        float halfHeight = camera.OrthographicSize * 0.5f / camera.Zoom;
        float aspectRatio = camera.Viewport.X / (float)camera.Viewport.Y;
        float halfWidth = halfHeight * aspectRatio;

        camera.View =
            Matrix4.CreateTranslation(-position.X, -position.Y, 0f) *
            Matrix4.CreateRotationZ(-camera.Rotation);

        camera.Projection = Matrix4.CreateOrthographicOffCenter(
            -halfWidth, halfWidth,
            -halfHeight, halfHeight,
            -1f, 1f
        );

        camera.ViewProjection = camera.View * camera.Projection;
    }
}