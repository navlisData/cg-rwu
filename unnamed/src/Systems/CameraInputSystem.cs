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
    private bool rotationLocked;

    private readonly Func<KeyboardState> keyboardStateProvider =
        keyboardProvider ?? throw new ArgumentNullException(nameof(keyboardProvider));

    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        MouseState mouseState = this.mouseStateProvider();
        KeyboardState keyboardState = this.keyboardStateProvider();

        ref Camera2D camera = ref handle.Get<Camera2D>();

        camera.Zoom *= (float)Math.Pow(1.1f, mouseState.ScrollDelta.Y);
        camera.Zoom = Math.Clamp(camera.Zoom, 0.01f, 5.0f);

        bool cwDown = keyboardState.IsKeyDown(Controls.RotateCamCW);
        bool ccwDown = keyboardState.IsKeyDown(Controls.RotateCamCCW);
        bool resetChordDown = cwDown && ccwDown;

        if (resetChordDown)
        {
            camera.Rotation = 0f;
            this.rotationLocked = true;
        }

        if (this.rotationLocked)
        {
            if (!cwDown && !ccwDown)
            {
                this.rotationLocked = false;
            }

            return;
        }

        float direction = (cwDown ? 0.1f : 0f) + (ccwDown ? -0.1f : 0f);
        camera.Rotation += direction;
    }
}