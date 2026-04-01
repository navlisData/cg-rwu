using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;

namespace unnamed.systems;

public sealed class SetToMousePositionSystem(World world, Func<MouseState> mouseProvider) : EntitySetSystem<Camera2D>(
    world, new QueryBuilder()
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
            Vector2 mouse = mouseState.Position;

            Vector2 viewport = camera2D.Viewport;
            Vector2 reference = new(500, 500);

            float scale = MathF.Min(
                viewport.X / reference.X,
                viewport.Y / reference.Y);

            screenPos = new AbsolutePosition(mouseState.Position.X / scale, mouseState.Position.Y / scale);
        }
        catch
        {
            // ignored, this will fail if the Window is not initialized yet
        }
    }
}