using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;

namespace unnamed.systems;

public sealed class SetToMousePositionSystem(World world, Func<MouseState> mouseProvider) : EntitySetSystem<Camera2D>(
    world, world.Query()
        .With<SetPositionToMouse>()
        .With<AbsolutePosition>()
        .Build()
)
{
    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    protected override void Update(Camera2D camera2D, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        MouseState mouseState = this.mouseStateProvider();

        ref AbsolutePosition screenPos = ref handle.Get<AbsolutePosition>();

        try
        {
            screenPos = (AbsolutePosition)mouseState.Position;
        }
        catch
        {
            // ignored, this will fail if the Window is not initialized yet
        }
    }
}