using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Utils;

namespace unnamed.systems;

public sealed class SetToMousePositionSystem(World world, Func<MouseState> mouseProvider) : EntitySetSystem<Camera2D>(
    world, world.Query()
        .With<SetPositionToMouse>()
        .With<Position>()
        .Build()
)
{
    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    protected override void Update(Camera2D camera2D, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        MouseState mouseState = this.mouseStateProvider();

        ref Position pos = ref handle.Get<Position>();
        ref Transform transform = ref handle.Get<Transform>();

        try
        {
            Vector2 mousePositionWorld =
                Projection.ScreenToWorldCoordinates(mouseState.Position, camera2D.Viewport,
                    camera2D.ViewProjection);

            pos = new Position(Vector2i.Zero, Vector2i.Zero, mousePositionWorld);
            transform.Scale = 1 / camera2D.Zoom;
        }
        catch
        {
            // ignored, this will fail if the Window is not initialized yet
        }
    }
}