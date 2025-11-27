using engine.Control;

using Engine.Ecs;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Enums;
using unnamed.GameMap;
using unnamed.GameMap.MapGeneration;
using unnamed.Prefabs;
using unnamed.Rendering;
using unnamed.systems;
using unnamed.Texture;
using unnamed.Texture.DirectedAction;
using unnamed.Texture.NonDirectionalAction;
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
    private readonly IAssetStore assetStore = new AssetStore();
    private readonly CameraInputSystem cameraInputSystem;
    private readonly CameraSystem cameraSystem;
    private readonly CharacterVisualSystem characterVisualSystem;
    private readonly CharacterRenderSystem characterRenderSystem;
    private readonly DestroyEntitySystem destroyEntitySystem;
    private readonly EnemyControlSystem enemyControlSystem;
    private readonly EntityCollisionDetectSystem ecds;
    private readonly FollowingSystem followSystem;
    private readonly Map gameMap;
    private readonly HandleCollisionSystem hcs;
    private readonly MapLoadingSystem mapLoadingSystem;
    private readonly MapRenderSystem mapRenderSystem;
    private readonly MoveSystem move;
    private readonly PlayerInputSystem playerInput;
    private readonly ProjectileRenderingSystem projectileRenderSystem;

    private readonly DirectedActionDatabase directedActionDatabase = DirectedActionDatabase.CreateDefault();

    private readonly NonDirectionalActionDatabase nonDirectionalActionDatabase =
        NonDirectionalActionDatabase.CreateDefault();

    private readonly ActionControlHandler<PlayerAction> playerActionHandler = new(PlayerActionExtensions.Priority);
    private readonly ActionControlHandler<EnemyAction> enemyActionHandler = new(EnemyActionExtensions.Priority);

    private readonly SetToMousePositionSystem setToMousePositionSystem;
    private readonly ShadowRenderSystem shadowRenderSystem;
    private readonly SpriteAnimationSystem spriteAnimationSystem;
    private readonly UiRenderSystem uiRenderSystem;

    private readonly World world = new();

    private Entity camera;
    private Entity player;
    private int shaderProgram;
    private int shadowProgram;

    public Game() : base(NativeSettings, Settings)
    {
        this.gameMap = new Map(this.world, new GraphBasedGenerator());

        // Rendering systems
        this.cameraSystem = new CameraSystem(this.world);
        this.characterRenderSystem = new CharacterRenderSystem(this.world, this.assetStore);
        this.followSystem = new FollowingSystem(this.world);
        this.mapRenderSystem = new MapRenderSystem(this.world, this.assetStore);
        this.shadowRenderSystem = new ShadowRenderSystem(this.world, this.assetStore);
        this.projectileRenderSystem = new ProjectileRenderingSystem(this.world, this.assetStore);
        this.spriteAnimationSystem = new SpriteAnimationSystem(this.world);
        this.uiRenderSystem = new UiRenderSystem(this.world, this.assetStore);

        // General systems
        this.characterVisualSystem =
            new CharacterVisualSystem(this.world, this.assetStore, this.directedActionDatabase,
                this.nonDirectionalActionDatabase);
        this.move = new MoveSystem(this.world, this.gameMap, this.assetStore);
        this.playerInput = new PlayerInputSystem(this.world, () => this.KeyboardState, () => this.MouseState);
        this.mapLoadingSystem = new MapLoadingSystem(this.world);
        this.setToMousePositionSystem = new SetToMousePositionSystem(this.world, () => this.MouseState);
        this.cameraInputSystem = new CameraInputSystem(this.world, () => this.KeyboardState, () => this.MouseState);
        this.destroyEntitySystem = new DestroyEntitySystem(this.world);
        this.enemyControlSystem = new EnemyControlSystem(this.world);
        this.ecds = new EntityCollisionDetectSystem(this.world, this.assetStore);
        this.hcs = new HandleCollisionSystem(this.world);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        this.shaderProgram = Shader.Setup("sprite.vert", "sprite.frag");
        this.shadowProgram = Shader.Setup("sprite.vert", "shadow.frag");

        GameSprites.Init(this.assetStore);

        this.gameMap.SpriteMapper = new SpriteMapper(this.assetStore);
        this.gameMap.GenerateMap(
            new Vector2i(-2, -2),
            new Vector2i(2, 2));

        this.gameMap.NextValidPosition(out Position playerStartPosition);
        this.player = PrefabFactory.CreatePlayer(
            this.world,
            playerStartPosition,
            new Vector2(0f, 0f),
            new Vector2(2, 5),
            this.assetStore);

        Random rng = new();
        for (int mc_y = -2; mc_y <= 2; mc_y += 1)
        for (int mc_x = -2; mc_x <= 2; mc_x += 1)
        {
            for (int mt_y = 0; mt_y < Map.ChunkSize; mt_y += 1)
            for (int mt_x = 0; mt_x < Map.ChunkSize; mt_x += 1)
            {
                Position pos = new(mc_x, mc_y, mt_x, mt_y, 2, 2);
                if (!this.gameMap.IsWallAt(pos))
                {
                    if (rng.Next(0, 10) == 0)
                    {
                        PrefabFactory.CreateEnemy(this.world, pos, new Vector2(1, 3),
                            new EntityStats { Hitpoints = 20 }, this.assetStore);
                    }
                }
            }
        }

        this.camera =
            PrefabFactory.CreateFollowingCamera(this.world, this.player, InitialGameSize, playerStartPosition);

        this.CursorState = CursorState.Hidden;
        PrefabFactory.CreateCrossHair(this.world, this.assetStore);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        float dt = (float)args.Time;

        if (this.KeyboardState.IsKeyDown(Keys.Escape))
        {
            this.Close();
        }

        this.cameraInputSystem.Run(dt);
        this.setToMousePositionSystem.Run(this.camera.Get<Camera2D>());
        this.playerInput.Run((dt, this.camera.Get<Camera2D>(), this.ClientSize,
            this.assetStore, this.playerActionHandler));
        this.followSystem.Run(dt);
        this.cameraSystem.Run(dt);
        this.enemyControlSystem.Run((dt, this.enemyActionHandler));
        this.characterVisualSystem.Run(dt);
        this.spriteAnimationSystem.Run(dt);
        this.move.Run(dt);
        this.mapLoadingSystem.Run(this.camera.Get<Position>());
        this.ecds.Run(dt);
        this.hcs.Run((dt, enemyActionHandler, this.assetStore));
        this.destroyEntitySystem.Run(dt);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        ref Camera2D cameraPosition = ref this.camera.Get<Camera2D>();

        this.mapRenderSystem.Run(this.shaderProgram, (cameraPosition, 0));
        this.shadowRenderSystem.Run(this.shadowProgram, cameraPosition);
        this.projectileRenderSystem.Run(this.shaderProgram, cameraPosition);
        this.characterRenderSystem.Run(this.shaderProgram, cameraPosition);
        this.mapRenderSystem.Run(this.shaderProgram, (cameraPosition, 1));
        this.uiRenderSystem.Run(this.shaderProgram, cameraPosition);

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

        this.assetStore.Dispose();
        this.mapRenderSystem.OnUnload();
        this.shadowRenderSystem.OnUnload();
        this.projectileRenderSystem.OnUnload();
        this.characterRenderSystem.OnUnload();
        this.uiRenderSystem.OnUnload();
        GL.DeleteProgram(this.shaderProgram);
    }
}