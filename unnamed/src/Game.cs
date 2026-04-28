using engine.Control;

using Engine.Ecs;

using engine.Ecs.State;

using Engine.Ecs.Systems;

using engine.TextureProcessing;
using engine.TextureProcessing.Text;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using SixLabors.ImageSharp;

using unnamed.Components.Physics;
using unnamed.Components.UI;
using unnamed.Enums;
using unnamed.GameMap;
using unnamed.GameMap.MapGeneration;
using unnamed.Prefabs;
using unnamed.Rendering;
using unnamed.Rendering.RenderContext;
using unnamed.Resources;
using unnamed.systems;
using unnamed.Systems;
using unnamed.Systems.SystemScheduler;
using unnamed.Texture;
using unnamed.Texture.DirectedAction;
using unnamed.Texture.NonDirectionalAction;
using unnamed.Utils;

namespace unnamed;

public class Game : GameWindow
{
    private static readonly Vector2i ReferenceUiResolution = (500, 500);

    private static readonly NativeWindowSettings Settings = new()
    {
        Title = "Unnamed", Vsync = VSyncMode.On, ClientSize = ReferenceUiResolution
    };

    private static readonly GameWindowSettings NativeSettings = new() { UpdateFrequency = 60 };
    private readonly CameraInputSystem cameraInputSystem;
    private readonly CameraSystem cameraSystem;
    private readonly CharacterVisualSystem characterVisualSystem;

    private readonly List<StaticSprite> deco = [];
    private readonly DestroyEntitySystem destroyEntitySystem;

    private readonly EnemyControlSystem enemyControlSystem;
    private readonly EnemyHealthRenderSystem enemyHealthRenderSystem;
    private readonly EntityCollisionDetectSystem entityCollisionDetectSystem;
    private readonly EntityRenderSystem entityRenderSystem;
    private readonly FadeAnimationSystem fadeAnimationSystem;
    private readonly FollowingSystem followSystem;
    private readonly HandleCollisionSystem handleCollisionSystem;
    private readonly HealthHudLayoutSystem healthLayoutSystem;

    // Health
    private readonly HealthHudSyncSystem healthSyncSystem;
    private readonly LifespanSystem lifespanSystem;
    private readonly MapLoadingSystem mapLoadingSystem;
    private readonly MapPropsRenderSystem mapPropsRenderingSystem;
    private readonly MapRenderSystem mapRenderSystem1;
    private readonly MapRenderSystem mapRenderSystem2;
    private readonly MoveSystem move;

    private readonly PlayerEntityCollisionSystem playerEntityCollisionSystem;
    private readonly PlayerInputSystem playerInput;
    private readonly ProjectileRenderingSystem projectileRenderSystem;
    private readonly PulseAnimationSystem pulseAnimationSystem;
    private readonly SystemScheduler<GameState, World> renderScheduler = new();

    private readonly SetToMousePositionSystem setToMousePositionSystem;
    private readonly ShadowRenderSystem shadowRenderSystem;
    private readonly SpawnerSystem spawnerSystem;
    private readonly SpriteAnimationSystem spriteAnimationSystem;

    private readonly StaticTextTextureFactory textFactoryLarge = new(
        Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "flavina.regular.ttf"), 48f);

    private readonly StaticTextTextureFactory textFactoryMedium = new(
        Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "flavina.regular.ttf"), 28f);

    private readonly UiRenderSystem uiRenderSystem;

    private readonly SystemScheduler<GameState, World> updateScheduler = new();
    private readonly WindSystem windSystem;

    private World world = new();

    public Game() : base(NativeSettings, Settings)
    {
        // Rendering systems
        this.cameraSystem = new CameraSystem();
        this.entityRenderSystem = new EntityRenderSystem();
        this.followSystem = new FollowingSystem();
        this.mapRenderSystem1 = new MapRenderSystem(0);
        this.mapRenderSystem2 = new MapRenderSystem(1);
        this.shadowRenderSystem = new ShadowRenderSystem();
        this.projectileRenderSystem = new ProjectileRenderingSystem();
        this.spriteAnimationSystem = new SpriteAnimationSystem();
        this.uiRenderSystem = new UiRenderSystem();
        this.enemyHealthRenderSystem = new EnemyHealthRenderSystem();
        this.mapPropsRenderingSystem = new MapPropsRenderSystem();

        // General systems
        this.characterVisualSystem =
            new CharacterVisualSystem();
        this.move = new MoveSystem();
        this.playerInput = new PlayerInputSystem(() => this.KeyboardState, () => this.MouseState);
        this.mapLoadingSystem = new MapLoadingSystem();
        this.setToMousePositionSystem = new SetToMousePositionSystem(() => this.MouseState);
        this.cameraInputSystem = new CameraInputSystem(() => this.KeyboardState, () => this.MouseState);
        this.destroyEntitySystem = new DestroyEntitySystem();
        this.enemyControlSystem = new EnemyControlSystem();
        this.entityCollisionDetectSystem = new EntityCollisionDetectSystem();
        this.playerEntityCollisionSystem =
            new PlayerEntityCollisionSystem();
        this.handleCollisionSystem = new HandleCollisionSystem();
        this.healthSyncSystem = new HealthHudSyncSystem();
        this.healthLayoutSystem = new HealthHudLayoutSystem();
        this.pulseAnimationSystem = new PulseAnimationSystem();
        this.spawnerSystem = new SpawnerSystem();
        this.lifespanSystem = new LifespanSystem();
        this.windSystem = new WindSystem();
        this.fadeAnimationSystem = new FadeAnimationSystem();
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);

        GL.Enable(EnableCap.Blend);
        GL.Viewport(0, 0, this.FramebufferSize.X, this.FramebufferSize.Y);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        AssetStore assetStore = new();
        GameSprites.Init(assetStore);

        this.ConfigureSchedulers();

        this.deco.AddRange(assetStore.Get(GameAssets.MapDecoration.Bricks));
        this.deco.AddRange(assetStore.Get(GameAssets.MapDecoration.Bushes));
        this.deco.AddRange(assetStore.Get(GameAssets.MapDecoration.Grass));
        this.deco.AddRange(assetStore.Get(GameAssets.MapDecoration.SmallStones));

        this.Reset(assetStore, 1);
    }

    private void Reset(AssetStore assetStore, int level)
    {
        this.world = new World();
        this.world.AddResource(assetStore);

        this.world.AddResource(new RenderContext(assetStore, Shader.Setup("sprite.vert", "sprite.frag"),
            ReferenceUiResolution));

        Map gameMap = new();
        gameMap.GenerateMap(
            this.world,
            new GraphBasedGenerator(), new SpriteMapper(assetStore),
            new Vector2i(-2, -2),
            new Vector2i(2, 2));

        gameMap.NextValidPosition(out Position portalPosition);
        PrefabFactory.CreatePortal(
            this.world,
            portalPosition);

        gameMap.NextValidPosition(out Position playerStartPosition);
        Entity player = PrefabFactory.CreatePlayer(
            this.world,
            playerStartPosition);

        gameMap.SpawnEntitiesRandomlyOnMap(this.world, 10,
            pos => PrefabFactory.CreateEnemy(this.world, pos, player, level));

        gameMap.SpawnEntitiesRandomlyOnMap(this.world, 10,
            pos => PrefabFactory.CreateMapDeco(this.world, pos + new Vector2(ShiftInTile(), ShiftInTile()),
                new Vector2(2f, 2f), this.deco[Random.Shared.Next(this.deco.Count)]), false);

        this.world.AddResource(gameMap);
        this.world.AddResource(new Camera2D
        {
            Zoom = 1f, OrthographicSize = 20f, Viewport = this.FramebufferSize, Position = playerStartPosition
        });
        this.world.AddResource(new ActionControlHandler<PlayerAction>(PlayerActionExtensions.Priority));
        this.world.AddResource(new ActionControlHandler<EnemyAction>(EnemyActionExtensions.Priority));
        this.world.AddResource(new NonDirectionalActionDatabase());
        this.world.AddResource(new DirectedActionDatabase());
        this.world.AddResource(new Level(level));
        this.world.AddState(GameState.InGame);

        this.CursorState = CursorState.Confined;
        this.Cursor = MouseCursor.Empty;

        PrefabFactory.CreateText(this.world, $"Level: {level}", Color.White,
            this.textFactoryMedium, Pivot.TopRight, UiAnchor.TopRight, new UiReferenceOffset(-10, 10));

        PrefabFactory.CreateCrossHairSpawner(this.world,
            PrefabFactory.CreateCrossHair);
        return;

        float ShiftInTile()
        {
            return (Random.Shared.NextSingle() * Map.TileSize) - (Map.TileSize / 2);
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        this.world.AddOrUpdateResource(new DeltaTime((float)args.Time));
        this.world.AddOrUpdateResource(new WindowSize(this.ClientSize));

        if (this.KeyboardState.IsKeyDown(Keys.Escape))
        {
            this.Close();
        }

        ref State<GameState> state = ref this.world.GetState<GameState>();
        GameState currentState = state.Current();

        if (this.KeyboardState.IsKeyPressed(Keys.Space))
        {
            if (currentState.Equals(GameState.Lost) || currentState.Equals(GameState.Won))
            {
                state.QueueChange(GameState.InGame);
            }
        }

        if (this.KeyboardState.IsKeyPressed(Keys.P) &&
            (currentState.Equals(GameState.InGame) || currentState.Equals(GameState.Paused)))
        {
            if (currentState.Equals(GameState.Paused))
            {
                state.QueueChange(GameState.InGame);
            }
            else
            {
                state.QueueChange(GameState.Paused);
            }
        }

        this.updateScheduler.Run(currentState, this.world);
        this.UpdateGameState(ref state);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        ref State<GameState> gameState = ref this.world.GetState<GameState>();
        ref RenderContext renderContext = ref this.world.GetResource<RenderContext>();
        renderContext.UpdateState(this.world.GetResource<Camera2D>());
        this.renderScheduler.Run(gameState.Current(), this.world);

        this.SwapBuffers();
    }

    private void ConfigureSchedulers()
    {
        this.updateScheduler
            .DuringGameplay(this.cameraInputSystem.Run)
            .DuringGameplay(this.setToMousePositionSystem.Run)
            .DuringGameplay(this.healthLayoutSystem.Run)
            .DuringGameplay(this.healthSyncSystem.Run)
            .DuringGameplay(this.playerInput.Run)
            .DuringGameplay(this.followSystem.Run)
            .DuringGameplay(this.cameraSystem.Run)
            .DuringGameplay(this.enemyControlSystem.Run)
            .DuringGameplay(this.characterVisualSystem.Run)
            .DuringGameplay(this.spriteAnimationSystem.Run)
            .DuringGameplay(this.spawnerSystem.Run)
            .DuringGameplay(this.windSystem.Run)
            .DuringGameplay(this.move.Run)
            .DuringGameplay(this.mapLoadingSystem.Run)
            .DuringGameplay(this.entityCollisionDetectSystem.Run)
            .DuringGameplay(this.playerEntityCollisionSystem.Run)
            .DuringGameplay(this.handleCollisionSystem.Run)
            .DuringGameplay(this.pulseAnimationSystem.Run)
            .Always(this.fadeAnimationSystem.Run)
            .DuringGameplay(this.lifespanSystem.Run)
            .DuringGameplay(this.destroyEntitySystem.Run);

        this.renderScheduler
            .DuringGame(this.mapRenderSystem1.Run)
            .DuringGame(this.mapPropsRenderingSystem.Run)
            .DuringGame(this.shadowRenderSystem.Run)
            .DuringGame(this.projectileRenderSystem.Run)
            .DuringGame(this.entityRenderSystem.Run)
            .DuringGame(this.enemyHealthRenderSystem.Run)
            .DuringGame(this.mapRenderSystem2.Run)
            .Always(this.uiRenderSystem.Run);
    }

    private void UpdateGameState(ref State<GameState> state)
    {
        if (!state.HasChanged(out GameState currentState, out GameState nextState))
        {
            return;
        }

        switch (nextState)
        {
            case GameState.Lost:
                {
                    PrefabFactory.CreateCenteredText(this.world, "You've died\n\nPress Space to restart", Color.Red,
                        this.textFactoryLarge);
                    this.world.GetResource<Level>().Value = 1;
                }
                break;
            case GameState.Won:
                {
                    PrefabFactory.CreateCenteredText(this.world,
                        "You've reached the end of this level.\nPress Space to continue",
                        Color.Green, this.textFactoryLarge);
                    this.world.GetResource<Level>().Value += 1;
                    ;
                }
                break;
            case GameState.InGame:
                {
                    if (currentState.Equals(GameState.Lost) || currentState.Equals(GameState.Won))
                    {
                        this.Reset(this.world.GetResource<AssetStore>(), this.world.GetResource<Level>());
                    }
                    else
                    {
                        this.CursorState = CursorState.Confined;
                        this.Cursor = MouseCursor.Empty;
                    }
                }
                break;
            case GameState.Paused:
                {
                    this.CursorState = CursorState.Normal;
                    this.Cursor = MouseCursor.Default;
                }
                break;
        }

        state.DoChange();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
        this.world.GetResource<Camera2D>().Viewport = new Vector2i(e.Width, e.Height);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        this.world.GetResource<RenderContext>().OnUnload();
    }
}