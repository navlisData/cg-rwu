using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Tags;
using unnamed.Components.UI;

namespace unnamed.systems;

public sealed class SetToMousePositionSystem(Func<MouseState> mouseProvider)
    : EntitySetSystem(
        new QueryBuilder()
            .With<SetPositionToMouse>()
            .With<AbsolutePosition>()
            .Build()
    )
{
    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    protected override void Update(EntityHandle e)
    {
        MouseState mouseState = this.mouseStateProvider();

        ref AbsolutePosition screenPos = ref e.Get<AbsolutePosition>();

        try
        {
            screenPos = mouseState.Position;
        }
        catch
        {
            // ignored, this will fail if the Window is not initialized yet
        }
    }
}