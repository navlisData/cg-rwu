using System.Drawing;

using Engine.Ecs;

using engine.TextureProcessing;

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

    private readonly AssetStore assets = new();

    private readonly CameraSystem cameraSystem;
    private readonly FollowingSystem followSystem;
    private readonly MapLoadingSystem mapLoadingSystem;
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
        this.mapRenderSystem = new MapRenderSystem(this.world, this.assets);
        this.mapLoadingSystem = new MapLoadingSystem(this.world);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);

        this.shaderProgram = Shader.Setup();

        string spritePath = Path.Combine(AppContext.BaseDirectory, "Assets", "floor.png");
        Dictionary<string, RectangleF> floorSprites = GameSprites.GetAllFloorSprites();
        SpriteSheetId sheetId = this.assets.LoadSpriteSheet(spritePath, floorSprites);

        this.player = PrefabFactory.CreatePlayer(this.world,
            new Position(),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f)
        );

        this.camera =
            PrefabFactory.CreateFollowingCamera(this.world, this.player, InitialGameSize);

        Random rnd = Random.Shared;
        string[] keys = floorSprites.Keys.ToArray();
        foreach (int gridY in Enumerable.Range(-10, 20))
        {
            foreach (int gridX in Enumerable.Range(-10, 20))
            {
                Entity chunk = PrefabFactory.CreateMapChunk(this.world, new Vector2i(gridX, gridY));
                ref Entity[] tiles = ref chunk.Get<TileRef>().Tiles;
                foreach (int y in Enumerable.Range(0, Constants.GridSizeX))
                {
                    foreach (int x in Enumerable.Range(0, Constants.GridSizeY))
                    {
                        string spriteName = floorSprites.Keys.ElementAt(rnd.Next(keys.Length));
                        SpriteFrameId frameId = this.assets.GetFrame(sheetId, spriteName);
                        Entity mapTile = PrefabFactory.CreateMapTile(this.world, TileType.Pathway, frameId, chunk,
                            new Vector2i(x, y));
                        tiles[(y * Constants.GridSizeY) + x] = mapTile;
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
        this.mapLoadingSystem.Run(this.player.Get<Position>());
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.UseProgram(this.shaderProgram);

        ref Camera2D cameraPosition = ref this.camera.Get<Camera2D>();

        this.mapRenderSystem.Run((this.shaderProgram, cameraPosition));

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

        this.assets.Dispose();
        this.mapRenderSystem.OnUnload();
        GL.DeleteProgram(this.shaderProgram);
    }
}