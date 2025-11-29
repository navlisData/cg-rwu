using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

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
        MouseState mouseState = this.mouseStateProvider();

        if (!e.Has<UiScreenAnchor>())
        {
            e.Add(new UiScreenAnchor());
        }
            
        ref UiScreenAnchor anchor = ref e.Get<UiScreenAnchor>();
        anchor.ScreenPixels = mouseState.Position;
    }
}