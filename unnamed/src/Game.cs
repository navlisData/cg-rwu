using engine.Control;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;
using engine.TextureProcessing.Text;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using SixLabors.Fonts;
using SixLabors.ImageSharp;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Enums;
using unnamed.GameMap;
using unnamed.GameMap.MapGeneration;
using unnamed.Prefabs;
using unnamed.Rendering;
using unnamed.Rendering.RenderContext;
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
    private readonly FadeAnimationSystem fadeAnimationSystem;
    private readonly FollowingSystem followSystem;
    private readonly Map gameMap;
    private readonly HandleCollisionSystem handleCollisionSystem;
    private readonly HealthHudLayoutSystem healthLayoutSystem;

    // Health
    private readonly HealthHudSyncSystem healthSyncSystem;
    private readonly LifespanSystem lifespanSystem;
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
    private readonly SystemScheduler<GameState, RenderContext> renderScheduler = new();

    private readonly SetToMousePositionSystem setToMousePositionSystem;
    private readonly ShadowRenderSystem shadowRenderSystem;
    private readonly SpawnerSystem spawnerSystem;
    private readonly SpriteAnimationSystem spriteAnimationSystem;

    private readonly UiRenderSystem uiRenderSystem;

    private readonly SystemScheduler<GameState, UpdateContext> updateScheduler = new();
    private readonly WindSystem windSystem;

    private readonly World world = new();

    private Entity camera;
    private GameState gameState = GameState.InGame;
    private Entity player;

    private RenderContext renderContext;

    private StaticTextTextureFactory textFactory;

    public Game() : base(NativeSettings, Settings)
    {
        this.gameMap = new Map(this.world, new GraphBasedGenerator());

        // Rendering systems
        this.cameraSystem = new CameraSystem(this.world);
        this.entityRenderSystem = new EntityRenderSystem(this.world);
        this.followSystem = new FollowingSystem(this.world);
        this.mapRenderSystem = new MapRenderSystem(this.world);
        this.shadowRenderSystem = new ShadowRenderSystem(this.world);
        this.projectileRenderSystem = new ProjectileRenderingSystem(this.world);
        this.spriteAnimationSystem = new SpriteAnimationSystem(this.world);
        this.uiRenderSystem = new UiRenderSystem(this.world, this.assetStore);
        // this.uiTextRenderSystem = new UiTextRenderSystem(this.world);
        this.enemyHealthRenderSystem = new EnemyHealthRenderSystem(this.world);
        this.mapPropsRenderingSystem = new MapPropsRenderSystem(this.world);

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
        this.spawnerSystem = new SpawnerSystem(this.world);
        this.lifespanSystem = new LifespanSystem(this.world);
        this.windSystem = new WindSystem(this.world);
        this.fadeAnimationSystem = new FadeAnimationSystem(this.world);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GameSprites.Init(this.assetStore);
        this.renderContext = new RenderContext(this.assetStore, Shader.Setup("sprite.vert", "sprite.frag"));
        this.ConfigureSchedulers();

        this.textFactory =
            new StaticTextTextureFactory(
                Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "linkage-free.regular.ttf"), 48f);

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
            this.assetStore);

        this.gameMap.SpawnEntitiesRandomlyOnMap(10,
            pos => PrefabFactory.CreateEnemy(this.world, pos,
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

        PrefabFactory.CreateCrossHairSpawner(this.world,
            (w, p) => PrefabFactory.CreateCrossHair2(w, p, this.assetStore));
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
        KeyboardState keyboard = this.KeyboardState;

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            this.Close();
        }

        if (keyboard.IsKeyPressed(Keys.P) &&
            (this.gameState.Equals(GameState.InGame) || this.gameState.Equals(GameState.Paused)))
        {
            if (this.gameState.Equals(GameState.Paused))
            {
                this.gameState = GameState.InGame;
                this.CursorState = CursorState.Confined;
                this.Cursor = MouseCursor.Empty;
            }
            else
            {
                this.gameState = GameState.Paused;
                this.CursorState = CursorState.Normal;
                this.Cursor = MouseCursor.Default;
            }
        }

        UpdateContext context = new(dt, this.world.Get<Camera2D>(this.camera));
        this.updateScheduler.Run(this.gameState, context);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        this.renderContext.UpdateState(this.world.Get<Camera2D>(this.camera));
        this.renderScheduler.Run(this.gameState, this.renderContext);

        this.SwapBuffers();
    }

    private void ConfigureSchedulers()
    {
        this.updateScheduler
            .DuringGameplay(ctx => this.cameraInputSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.setToMousePositionSystem.Run(ctx.Camera))
            .DuringGameplay(ctx => this.healthLayoutSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.healthSyncSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.playerInput.Run((
                ctx.dt,
                ctx.Camera,
                this.ClientSize,
                this.assetStore,
                this.playerActionHandler)))
            .DuringGameplay(ctx => this.followSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.cameraSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.enemyControlSystem.Run((ctx.dt, this.enemyActionHandler)))
            .DuringGameplay(ctx => this.characterVisualSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.spriteAnimationSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.spawnerSystem.Run((ctx.dt, this.gameMap)))
            .DuringGameplay(ctx => this.windSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.move.Run(ctx.dt))
            .DuringGameplay(ctx => this.mapLoadingSystem.Run(this.world.Get<Position>(this.camera)))
            .DuringGameplay(ctx => this.entityCollisionDetectSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.playerEntityCollisionSystem.Run(this.player))
            .DuringGameplay(ctx => this.handleCollisionSystem.Run((
                ctx.dt,
                this.enemyActionHandler,
                this.assetStore, this.UpdateGameState)))
            .DuringGameplay(ctx => this.pulseAnimationSystem.Run(ctx.dt))
            .Always(ctx => this.fadeAnimationSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.lifespanSystem.Run(ctx.dt))
            .DuringGameplay(ctx => this.destroyEntitySystem.Run((ctx.dt, this.player)));

        this.renderScheduler
            .DuringGame(ctx => this.mapRenderSystem.Run((ctx, 0)))
            .DuringGame(ctx => this.mapPropsRenderingSystem.Run(ctx))
            .DuringGame(ctx => this.shadowRenderSystem.Run(ctx))
            .DuringGame(ctx => this.projectileRenderSystem.Run(ctx))
            .DuringGame(ctx => this.entityRenderSystem.Run(ctx))
            .DuringGame(ctx => this.enemyHealthRenderSystem.Run(ctx))
            .DuringGame(ctx => this.mapRenderSystem.Run((ctx, 1)))
            .Always(ctx => this.uiRenderSystem.Run(ctx));
    }

    private void UpdateGameState(GameState state)
    {
        this.gameState = state;

        switch (state)
        {
            case GameState.Lost:
                {
                    PrefabFactory.CreateText(this.world, "You've died\n\nPress ESC to exit", Color.Red,
                        this.textFactory,
                        this.ClientSize, TextAlignment.Center);
                }
                break;
            case GameState.Won:
                {
                    PrefabFactory.CreateText(this.world, "You've reached the end of this level.\nPress ESC to exit",
                        Color.Green, this.textFactory,
                        this.ClientSize, TextAlignment.Center);
                }
                break;
        }
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
        this.renderContext.OnUnload();
    }
}