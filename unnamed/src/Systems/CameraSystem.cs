using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Resources;

namespace unnamed.Systems;

public sealed class CameraSystem(World world) : BaseSystem<float>(world)
{
    public override void Run(float dt)
    {
        ref Camera2D camera = ref this.world.GetResource<Camera2D>();

        Entity player =
            new QueryBuilder().With<Player>().Build().Single(this.world);
        Position playerPos = this.world.Get<Position>(player);

        camera.Position = Position.Lerp(camera.Position, playerPos, 5f * dt);

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