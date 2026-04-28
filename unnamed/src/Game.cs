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

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
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

    private readonly UiRenderSystem uiRenderSystem;

    private readonly SystemScheduler<GameState, World> updateScheduler = new();
    private readonly WindSystem windSystem;

    private readonly World world = new();

    private StaticTextTextureFactory textFactory;
    private readonly StaticTextTextureFactory textFactoryLarge;
    private readonly StaticTextTextureFactory textFactoryMedium;
    private uint level = 1;

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
        this.world.AddResource(assetStore);

        this.world.AddResource(new RenderContext(assetStore, Shader.Setup("sprite.vert", "sprite.frag"),
            ReferenceUiResolution));
        this.ConfigureSchedulers();

        this.textFactoryLarge =
            new StaticTextTextureFactory(
                Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "flavina.regular.ttf"), 48f);

        this.textFactoryMedium =
            new StaticTextTextureFactory(
                Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "flavina.regular.ttf"), 28f);

        this.Reset();
    }

    private void Reset()
    {
        this.world = new();

        this.gameState = GameState.InGame;

        Map gameMap = new(new SpriteMapper(assetStore), new GraphBasedGenerator());
        gameMap.GenerateMap(
            this.world,
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
            pos => PrefabFactory.CreateEnemy(this.world, pos,
                new EntityStats(20, 20), player));

        List<StaticSprite> deco = [];
        deco.AddRange(assetStore.Get(GameAssets.MapDecoration.Bricks));
        deco.AddRange(assetStore.Get(GameAssets.MapDecoration.Bushes));
        deco.AddRange(assetStore.Get(GameAssets.MapDecoration.Grass));
        deco.AddRange(assetStore.Get(GameAssets.MapDecoration.SmallStones));

        gameMap.SpawnEntitiesRandomlyOnMap(this.world, 10,
            pos => PrefabFactory.CreateMapDeco(this.world, pos + new Vector2(ShiftInTile(), ShiftInTile()),
                new Vector2(2f, 2f),
                deco[Random.Shared.Next(deco.Count)]), false);


        this.world.AddResource(gameMap);
        this.world.AddResource(new Camera2D
        {
            Zoom = 1f, OrthographicSize = 20f, Viewport = this.FramebufferSize, Position = playerStartPosition
        });
        this.world.AddResource(new ActionControlHandler<PlayerAction>(PlayerActionExtensions.Priority));
        this.world.AddResource(new ActionControlHandler<EnemyAction>(EnemyActionExtensions.Priority));
        this.world.AddResource(new NonDirectionalActionDatabase());
        this.world.AddResource(new DirectedActionDatabase());
        this.world.AddState(GameState.InGame);

        this.CursorState = CursorState.Confined;
        this.Cursor = MouseCursor.Empty;

        PrefabFactory.CreateCrossHairSpawner(this.world,
            PrefabFactory.CreateCrossHair);
        return;

        float ShiftInTile()
        {
            return (Random.Shared.NextSingle() * Map.TileSize) - (Map.TileSize / 2);
        }
    }

    private void UpdateLevelText()
    {
        PrefabFactory.CreateText(this.world, "Level: " + this.level, Color.White,
            this.textFactoryMedium, Pivot.TopRight, UiAnchor.TopRight, new UiReferenceOffset(-10, 10));
    }

    private void InitSystems()
    {
        // Rendering systems
        this.cameraSystem = new CameraSystem(this.world);
        this.entityRenderSystem = new EntityRenderSystem(this.world);
        this.followSystem = new FollowingSystem(this.world);
        this.mapRenderSystem = new MapRenderSystem(this.world);
        this.shadowRenderSystem = new ShadowRenderSystem(this.world);
        this.projectileRenderSystem = new ProjectileRenderingSystem(this.world);
        this.spriteAnimationSystem = new SpriteAnimationSystem(this.world);
        this.uiRenderSystem = new UiRenderSystem(this.world, this.assetStore);
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
        GL.Viewport(0, 0, this.FramebufferSize.X, this.FramebufferSize.Y);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        this.renderContext =
            new RenderContext(this.assetStore, Shader.Setup("sprite.vert", "sprite.frag"), ReferenceUiResolution);
        this.ConfigureSchedulers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        this.world.AddOrUpdateResource(new DeltaTime((float)args.Time));
        this.world.AddOrUpdateResource(new WindowSize(this.ClientSize));
        KeyboardState keyboard = this.KeyboardState;

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            this.Close();
        }

        ref State<GameState> state = ref this.world.GetState<GameState>();
        GameState currentState = state.Current();

        if (keyboard.IsKeyPressed(Keys.Space))
        {
            if (currentState.Equals(GameState.Lost) || currentState.Equals(GameState.Won))
            {
                // TODO: FIXME
                this.Reset();
            }
        }

        if (keyboard.IsKeyPressed(Keys.P) &&
            (currentState.Equals(GameState.InGame) || currentState.Equals(GameState.Paused)))
        {
            if (currentState.Equals(GameState.Paused))
            {
                state.QueueChange(GameState.InGame);
                this.CursorState = CursorState.Confined;
                this.Cursor = MouseCursor.Empty;
            }
            else
            {
                state.QueueChange(GameState.Paused);
                this.CursorState = CursorState.Normal;
                this.Cursor = MouseCursor.Default;
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
        if (!state.HasChanged(out GameState nextState))
        {
            return;
        }

        switch (nextState)
        {
            case GameState.Lost:
                {
                    PrefabFactory.CreateCenteredText(this.world, "You've died\n\nPress Space to restart", Color.Red,
                        this.textFactoryLarge);
                    this.level = 1;
                }
                break;
            case GameState.Won:
                {
                    PrefabFactory.CreateCenteredText(this.world,
                        "You've reached the end of this level.\nPress Space to continue",
                        Color.Green, this.textFactoryLarge);
                    this.level++;
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