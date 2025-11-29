using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Utils;

namespace unnamed.systems;

public sealed class UiScreenAnchorSystem(World world)
    : EntitySetSystem<Camera2D>(world, world.Query()
        .With<Ui>()
        .With<UiScreenAnchor>()
        .With<Position>()
        .With<Transform>()
        .Build())
{
    protected override void Update(Camera2D camera2D, in Entity e)
    {
        ref UiScreenAnchor anchor = ref e.Get<UiScreenAnchor>();
        ref Position pos = ref e.Get<Position>();
        ref Transform transform = ref e.Get<Transform>();

        try
        {
            Vector2 worldPosition = Projection.ScreenToWorldCoordinates(
                anchor.ScreenPixels,
                camera2D.Viewport,
                camera2D.ViewProjection);

            pos = new Position(Vector2i.Zero, Vector2i.Zero, worldPosition);

            float zoom = camera2D.Zoom <= 0f ? 1f : camera2D.Zoom;
            transform.Scale = 1f / zoom;
        }
        catch
        {
            // ignored, this will fail if the Window or camera matrices are not initialized yet
        }
    }
}