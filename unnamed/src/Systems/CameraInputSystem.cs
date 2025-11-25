using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Utils;

namespace unnamed.systems;

public sealed class CameraInputSystem(World world, Func<KeyboardState> keyboardProvider, Func<MouseState> mouseProvider)
    : EntitySetSystem<float>(world, world.Query()
        .With<ReceivesCameraControl>()
        .With<Camera2D>()
        .Build()
    )
{
    private readonly Func<KeyboardState> keyboardStateProvider =
        keyboardProvider ?? throw new ArgumentNullException(nameof(keyboardProvider));

    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    protected override void Update(float dt, in Entity e)
    {
        MouseState mouseState = this.mouseStateProvider();
        KeyboardState keyboardState = this.keyboardStateProvider();

        ref Camera2D camera = ref e.Get<Camera2D>();

        camera.Zoom *= (float)Math.Pow(1.1f, mouseState.ScrollDelta.Y);
        camera.Zoom = Math.Clamp(camera.Zoom, 0.01f, 5.0f);

        if (keyboardState.IsKeyDown(Controls.RotateCamCW))
        {
            camera.Rotation += .1f;
        }

        if (keyboardState.IsKeyDown(Controls.RotateCamCCW))
        {
            camera.Rotation -= .1f;
        }
    }
}