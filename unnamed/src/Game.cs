using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
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
using unnamed.Systems.SystemScheduler;
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

    private readonly SystemScheduler<GameState, UpdateContext> updateScheduler = new();
    private readonly SystemScheduler<GameState, RenderContext> renderScheduler = new();

    private static readonly GameWindowSettings NativeSettings = new() { UpdateFrequency = 60 };
    private readonly IAssetStore assetStore = new AssetStore();
    private readonly CameraInputSystem cameraInputSystem;
    private readonly CameraSystem cameraSystem;
    private readonly CharacterVisualSystem characterVisualSystem;
    private readonly DestroyEntitySystem destroyEntitySystem;

    private readonly DirectedActionDatabase directedActionDatabase = DirectedActionDatabase.CreateDefault();
    private readonly ActionControlHandler<EnemyAction> enemyActionHandler = new(EnemyActionExtensions.Priority);
    private readonly EnemyControlSystem enemyControlSystem;
    private readonly EnemyHealthRenderSystem enemyHealthRenderSystem;
    private readonly EntityCollisionDetectSystem entityCollisionDetectSystem;
    private readonly EntityRenderSystem entityRenderSystem;
    private readonly FollowingSystem followSystem;
    private readonly Map gameMap;
    private readonly HandleCollisionSystem handleCollisionSystem;
    private readonly HealthHudLayoutSystem healthLayoutSystem;

    // Health
    private readonly HealthHudSyncSystem healthSyncSystem;
    private readonly MapLoadingSystem mapLoadingSystem;
    private readonly MapPropsRenderSystem mapPropsRenderingSystem;
    private readonly MapRenderSystem mapRenderSystem;
    private readonly MoveSystem move;

    private readonly NonDirectionalActionDatabase nonDirectionalActionDatabase =
        NonDirectionalActionDatabase.CreateDefault();

    private readonly ActionControlHandler<PlayerAction> playerActionHandler = new(PlayerActionExtensions.Priority);
    private readonly PlayerEntityCollisionSystem playerEntityCollisionSystem;
    private readonly PlayerInputSystem playerInput;
    private readonly ProjectileRenderingSystem projectileRenderSystem;
    private readonly PulseAnimationSystem pulseAnimationSystem;

    private readonly SetToMousePositionSystem setToMousePositionSystem;
    private readonly ShadowRenderSystem shadowRenderSystem;
    private readonly SpriteAnimationSystem spriteAnimationSystem;
    private readonly UiRenderSystem uiRenderSystem;

    private readonly World world = new();
    private GameState gameState = GameState.InGame;

    private Entity camera;
    private int healthbarProgram;
    private Entity player;
    private int shaderProgram;
    private int shadowProgram;

    public Game() : base(NativeSettings, Settings)
    {
        this.gameMap = new Map(this.world, new GraphBasedGenerator());

        // Rendering systems
        this.cameraSystem = new CameraSystem(this.world);
        this.entityRenderSystem = new EntityRenderSystem(this.world, this.assetStore);
        this.followSystem = new FollowingSystem(this.world);
        this.mapRenderSystem = new MapRenderSystem(this.world, this.assetStore);
        this.shadowRenderSystem = new ShadowRenderSystem(this.world, this.assetStore);
        this.projectileRenderSystem = new ProjectileRenderingSystem(this.world, this.assetStore);
        this.spriteAnimationSystem = new SpriteAnimationSystem(this.world);
        this.uiRenderSystem = new UiRenderSystem(this.world, this.assetStore);
        this.enemyHealthRenderSystem = new EnemyHealthRenderSystem(this.world);
        this.mapPropsRenderingSystem = new MapPropsRenderSystem(this.world, this.assetStore);

        // General systems
        this.characterVisualSystem =
            new CharacterVisualSystem(this.world, this.assetStore, this.directedActionDatabase,
                this.nonDirectionalActionDatabase);
        this.move = new MoveSystem(this.world, this.gameMap, this.assetStore);
        this.playerInput = new PlayerInputSystem(this.world, () => this.KeyboardState, () => this.MouseState);
        this.mapLoadingSystem = new MapLoadingSystem(this.world);
        this.setToMousePositionSystem = new SetToMousePositionSystem(this.world, () => this.MouseState);
        this.cameraInputSystem = new CameraInputSystem(this.world, () => this.KeyboardState, () => this.MouseState);
        this.destroyEntitySystem = new DestroyEntitySystem(this.world, this.assetStore);
        this.enemyControlSystem = new EnemyControlSystem(this.world);
        this.entityCollisionDetectSystem = new EntityCollisionDetectSystem(this.world, this.assetStore);
        this.playerEntityCollisionSystem =
            new PlayerEntityCollisionSystem(this.world, this.assetStore, this.enemyActionHandler);
        this.handleCollisionSystem = new HandleCollisionSystem(this.world);
        this.healthSyncSystem = new HealthHudSyncSystem(this.world, this.assetStore);
        this.healthLayoutSystem = new HealthHudLayoutSystem(this.world, this.assetStore);
        this.pulseAnimationSystem = new PulseAnimationSystem(this.world);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        this.shaderProgram = Shader.Setup("sprite.vert", "sprite.frag");
        this.shadowProgram = Shader.Setup("sprite.vert", "shadow.frag");
        this.healthbarProgram = Shader.Setup("sprite.vert", "healthbar.frag");

        GameSprites.Init(this.assetStore);
        ConfigureSchedulers();

        this.gameMap.SpriteMapper = new SpriteMapper(this.assetStore);
        this.gameMap.GenerateMap(
            new Vector2i(-2, -2),
            new Vector2i(2, 2));

        this.gameMap.NextValidPosition(out Position portalPosition);
        PrefabFactory.CreatePortal(
            this.world,
            portalPosition,
            this.assetStore);

        this.gameMap.NextValidPosition(out Position playerStartPosition);
        this.player = PrefabFactory.CreatePlayer(
            this.world,
            playerStartPosition,
            new Vector2(0f, 0f),
            new Vector2(2, 5),
            this.assetStore);

        this.gameMap.SpawnEntitiesRandomlyOnMap(10,
            pos => PrefabFactory.CreateEnemy(this.world, pos, new Vector2(1, 3),
                new EntityStats(20, 20), this.player,
                this.assetStore));

        Random rng = Random.Shared;
        List<StaticSprite> deco = [];
        deco.AddRange(this.assetStore.Get(GameAssets.MapDecoration.Bricks));
        deco.AddRange(this.assetStore.Get(GameAssets.MapDecoration.Bushes));
        deco.AddRange(this.assetStore.Get(GameAssets.MapDecoration.Grass));
        deco.AddRange(this.assetStore.Get(GameAssets.MapDecoration.SmallStones));

        this.gameMap.SpawnEntitiesRandomlyOnMap(10,
            pos => PrefabFactory.CreateMapDeco(this.world, pos + new Vector2(ShiftInTile(), ShiftInTile()),
                new Vector2(2f, 2f),
                deco[rng.Next(deco.Count)]), false);

        this.camera =
            PrefabFactory.CreateFollowingCamera(this.world, this.player, InitialGameSize, playerStartPosition);

        this.CursorState = CursorState.Confined;
        this.Cursor = MouseCursor.Empty;

        PrefabFactory.CreateCrossHair(this.world, this.assetStore);
        return;

        float ShiftInTile()
        {
            return (rng.NextSingle() * Map.TileSize) - (Map.TileSize / 2);
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

        var context = new UpdateContext(dt, this.world.Get<Camera2D>(this.camera));
        this.updateScheduler.Run(this.gameState, context);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        var context = new RenderContext(this.world.Get<Camera2D>(this.camera));
        this.renderScheduler.Run(this.gameState, context);

        this.SwapBuffers();
    }

    private void ConfigureSchedulers()
    {
        this.updateScheduler
            .InGame(ctx => this.cameraInputSystem.Run(ctx.dt))
            .InGame(ctx => this.setToMousePositionSystem.Run(ctx.Camera))
            .InGame(ctx => this.healthLayoutSystem.Run(ctx.dt))
            .InGame(ctx => this.healthSyncSystem.Run(ctx.dt))
            .InGame(ctx => this.playerInput.Run((
                ctx.dt,
                ctx.Camera,
                this.ClientSize,
                this.assetStore,
                this.playerActionHandler)))
            .InGame(ctx => this.followSystem.Run(ctx.dt))
            .InGame(ctx => this.cameraSystem.Run(ctx.dt))
            .InGame(ctx => this.enemyControlSystem.Run((ctx.dt, this.enemyActionHandler)))
            .InGame(ctx => this.characterVisualSystem.Run(ctx.dt))
            .InGame(ctx => this.spriteAnimationSystem.Run(ctx.dt))
            .InGame(ctx => this.move.Run(ctx.dt))
            .InGame(ctx => this.mapLoadingSystem.Run(this.world.Get<Position>(this.camera)))
            .InGame(ctx => this.entityCollisionDetectSystem.Run(ctx.dt))
            .InGame(ctx => this.playerEntityCollisionSystem.Run(this.player))
            .InGame(ctx => this.handleCollisionSystem.Run((
                ctx.dt,
                this.enemyActionHandler,
                this.assetStore,
                state => this.gameState = state)))
            .InGame(ctx => this.pulseAnimationSystem.Run(ctx.dt))
            .InGame(ctx => this.destroyEntitySystem.Run((ctx.dt, this.player)));

        this.renderScheduler
            .InGame(ctx => this.mapRenderSystem.Run(this.shaderProgram, (ctx.Camera, 0)))
            .InGame(ctx => this.mapPropsRenderingSystem.Run(ctx.Camera))
            .InGame(ctx => this.shadowRenderSystem.Run(this.shadowProgram, ctx.Camera))
            .InGame(ctx => this.projectileRenderSystem.Run(this.shaderProgram, ctx.Camera))
            .InGame(ctx => this.entityRenderSystem.Run(this.shaderProgram, ctx.Camera))
            .InGame(ctx => this.enemyHealthRenderSystem.Run(this.healthbarProgram, ctx.Camera))
            .InGame(ctx => this.mapRenderSystem.Run(this.shaderProgram, (ctx.Camera, 1)))
            .InGame(ctx => this.uiRenderSystem.Run((this.shaderProgram, this.ClientSize), this.ClientSize));
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, this.ClientSize.X, this.ClientSize.Y);
        this.world.Get<Camera2D>(this.camera).Viewport = this.ClientSize;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        this.assetStore.Dispose();
        this.mapRenderSystem.OnUnload();
        this.shadowRenderSystem.OnUnload();
        this.projectileRenderSystem.OnUnload();
        this.entityRenderSystem.OnUnload();
        this.enemyHealthRenderSystem.OnUnload();
        this.uiRenderSystem.OnUnload();
        GL.DeleteProgram(this.shaderProgram);
    }
}