using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;

namespace unnamed.systems;

public sealed class SetToMousePositionSystem(
    World world,
    Func<MouseState> mouseProvider,
    Func<Vector2i> clientSizeProvider) : EntitySetSystem<Camera2D>(
    world, new QueryBuilder()
        .With<SetPositionToMouse>()
        .With<AbsoluteSize>()
        .With<AbsolutePosition>()
        .Build()
)
{
    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    private readonly Func<Vector2i> clientSizeProvider =
        clientSizeProvider ?? throw new ArgumentNullException(nameof(clientSizeProvider));


    protected override void Update(Camera2D camera2D, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        MouseState mouseState = this.mouseStateProvider();
        Vector2i clientSize = this.clientSizeProvider();

        if (clientSize.X <= 0 || clientSize.Y <= 0 || camera2D.Viewport.X <= 0 || camera2D.Viewport.Y <= 0)
        {
            return;
        }

        // Scale the mouse position from client-space to viewport-space coordinates
        Vector2 mouseClient = mouseState.Position;
        Vector2 mouseViewport = new(
            mouseClient.X * camera2D.Viewport.X / clientSize.X,
            mouseClient.Y * camera2D.Viewport.Y / clientSize.Y);

        AbsoluteSize absoluteSize = handle.Get<AbsoluteSize>();
        Vector2 size = new(absoluteSize.Width, absoluteSize.Height);

        ref AbsolutePosition screenPos = ref handle.Get<AbsolutePosition>();

        float x = mouseViewport.X - (size.X * 0.5f);
        float y = camera2D.Viewport.Y - mouseViewport.Y - (size.Y * 0.5f);

        screenPos = new AbsolutePosition(x, y, screenPos.AllowWrapping);
    }
}