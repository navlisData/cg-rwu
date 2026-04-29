using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Resources;
using unnamed.Utils;

namespace unnamed.systems;

public sealed class CameraInputSystem(Func<KeyboardState> keyboardProvider, Func<MouseState> mouseProvider)
    : BaseSystem
{
    private readonly Func<KeyboardState> keyboardStateProvider =
        keyboardProvider ?? throw new ArgumentNullException(nameof(keyboardProvider));

    private readonly Func<MouseState> mouseStateProvider =
        mouseProvider ?? throw new ArgumentNullException(nameof(mouseProvider));

    private bool rotationLocked;

    public override void Run(World world)
    {
        MouseState mouseState = this.mouseStateProvider();
        KeyboardState keyboardState = this.keyboardStateProvider();

        ref Camera2D camera = ref world.GetResource<Camera2D>();

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