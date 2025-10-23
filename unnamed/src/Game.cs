using Engine.Ecs;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Prefabs;
using unnamed.Rendering;
using unnamed.systems;
using unnamed.Utils;

namespace unnamed;

public class Game : GameWindow
{
    private static readonly Vector2i InitialGameSize = (500, 500);

    private static readonly NativeWindowSettings Settings = new()
    {
        Title = "Unnamed", Vsync = VSyncMode.On, ClientSize = InitialGameSize
    };

    private static readonly GameWindowSettings NativeSettings = new() { UpdateFrequency = 60 };

    private readonly CameraSystem cameraSystem;
    private readonly EllipsisRenderSystem ellipsisRenderer;
    private readonly FollowingSystem followSystem;
    private readonly MapRenderSystem mapRenderSystem;
    private readonly MoveSystem move;
    private readonly PlayerInputSystem playerInput;
    private readonly World world = new();

    private Entity camera;
    private Entity player;
    private int shaderProgram;

    public Game() : base(NativeSettings, Settings)
    {
        this.move = new MoveSystem(this.world);
        this.playerInput = new PlayerInputSystem(this.world, () => this.KeyboardState, () => this.MouseState);
        this.followSystem = new FollowingSystem(this.world);
        this.cameraSystem = new CameraSystem(this.world);
        this.ellipsisRenderer = new EllipsisRenderSystem(this.world);
        this.mapRenderSystem = new MapRenderSystem(this.world);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);

        this.shaderProgram = Shader.Setup();

        this.player = PrefabFactory.CreatePlayer(this.world,
            new Position(),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f)
        );

        this.camera =
            PrefabFactory.CreateFollowingCamera(this.world, this.player, InitialGameSize);

        foreach (int gridX in Enumerable.Range(-5, 10))
        {
            foreach (int gridY in Enumerable.Range(-5, 10))
            {
                Entity chunk = PrefabFactory.CreateMapChunk(this.world, new Vector2i(gridX, gridY));
                foreach (int x in Enumerable.Range(0, 16))
                {
                    foreach (int y in Enumerable.Range(0, 16))
                    {
                        Entity unused =
                            PrefabFactory.CreateMapTile(this.world,
                                (x + y) % 2 == 0 ? TileType.Floor1 : TileType.Floor2, chunk,
                                new Vector2i(x, y));
                    }
                }
            }
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        float dt = (float)args.Time;

        if (this.KeyboardState.IsKeyDown(Keys.Escape))
        {
            this.Close();
        }

        this.playerInput.Run((dt, this.camera.Get<Camera2D>(), this.player.Get<Position>()));
        this.followSystem.Run(dt);
        this.cameraSystem.Run(dt);
        this.move.Run(dt);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.UseProgram(this.shaderProgram);

        ref Camera2D cameraPosition = ref this.camera.Get<Camera2D>();

        this.mapRenderSystem.Run((this.shaderProgram, cameraPosition));
        this.ellipsisRenderer.Run((this.shaderProgram, cameraPosition));

        this.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, this.Size.X, this.Size.Y);
        this.camera.Get<Camera2D>().Viewport = this.Size;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        this.ellipsisRenderer.onUnload();
        this.mapRenderSystem.OnUnload();
        GL.DeleteProgram(this.shaderProgram);
    }
}