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
    private static readonly NativeWindowSettings Settings = new()
    {
        Profile = ContextProfile.Compatability,
        Flags = ContextFlags.Default,
        Title = "Unnamed",
        Vsync = VSyncMode.On,
        ClientSize = new Vector2i(500, 500)
    };

    private static readonly GameWindowSettings NativeSettings = new() { UpdateFrequency = 60 };

    private readonly EllipsisRenderSystem ellipsisRenderer;

    private readonly MoveSystem move;
    private readonly PlayerInputSystem playerInput;
    private readonly World world = new();

    private Entity player;

    public Game() : base(NativeSettings, Settings)
    {
        this.move = new MoveSystem(this.world);
        this.playerInput = new PlayerInputSystem(this.world, () => this.KeyboardState);

        this.ellipsisRenderer = new EllipsisRenderSystem(this.world);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.08f, 0.08f, 0.1f, 1f);
        GL.Disable(EnableCap.DepthTest);

        this.player = PrefabFactory.CreatePlayer(this.world,
            new Vector2(0, 0),
            new Vector2(0f, 0f),
            new Vector2(0.05f, 0.05f)
        );
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        float dt = (float)args.Time;

        if (this.KeyboardState.IsKeyDown(Keys.Escape))
        {
            this.Close();
        }

        this.playerInput.Run(dt);
        this.move.Run(dt);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        this.ellipsisRenderer.Run(0f);

        this.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, this.Size.X, this.Size.Y);
    }
}