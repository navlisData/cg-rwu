using Engine.Ecs;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Prefabs;
using unnamed.Rendering;
using unnamed.systems;

namespace unnamed;

public class Game : GameWindow
{
    private readonly World world = new();
    
    private readonly MoveSystem move;
    private readonly PlayerInputSystem playerInput;

    private readonly EllipsisRenderSystem ellipsisRenderer;
    
    private Entity player;

    private static readonly NativeWindowSettings Settings = new ()
    {
        Profile = ContextProfile.Compatability,
        Flags = ContextFlags.Default,
        Title = "Unnamed",
        Vsync = VSyncMode.On,
        ClientSize = new Vector2i(500, 500)
    };

    private static readonly GameWindowSettings NativeSettings = new()
    {
        UpdateFrequency = 60,
    };

    public Game() : base(NativeSettings, Settings)
    {
        move = new MoveSystem(world);
        playerInput = new PlayerInputSystem(world, () => KeyboardState);

        ellipsisRenderer = new  EllipsisRenderSystem(world);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.08f, 0.08f, 0.1f, 1f);
        GL.Disable(EnableCap.DepthTest);

        player = PrefabFactory.CreatePlayer(
            world,
            startPos: new Vector2(0, 0),
            startVel: new Vector2(0f, 0f),
            size: new Vector2(0.05f, 0.05f)
        );
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        float dt = (float)args.Time;

        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

        playerInput.Run(dt);
        move.Run(dt);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        ellipsisRenderer.Run(0f);

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
}

