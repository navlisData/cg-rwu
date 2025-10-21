using Engine.Ecs;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using unnamed.Components.Rendering;
using unnamed.Prefabs;
using unnamed.Rendering;
using unnamed.systems;

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
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);

        this.shaderProgram = SetupShader();

        this.player = PrefabFactory.CreatePlayer(this.world,
            new Vector2(0, 0),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f)
        );

        this.camera =
            PrefabFactory.CreateFollowingCamera(this.world, this.player, InitialGameSize.X, InitialGameSize.Y);

        Random random = new();
        for (int _ = 0; _ < 100; _++)
        {
            PrefabFactory.CreateEllipsis(this.world,
                new Vector2(random.Next(-100, 100), random.Next(-100, 100)),
                new Vector2(random.Next(1, 5), random.Next(1, 5)),
                new Vector4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1f));
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

        this.playerInput.Run(dt);
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

        this.ellipsisRenderer.Run((this.shaderProgram, cameraPosition));

        this.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, this.Size.X, this.Size.Y);
        this.camera.Get<Camera2D>().AspectRatio = (float)this.Size.X / this.Size.Y;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        this.ellipsisRenderer.onUnload();
        GL.DeleteProgram(this.shaderProgram);
    }

    private static int SetupShader()
    {
        const string vertexShaderSource = """
                                          #version 330 core
                                          layout (location = 0) in vec2 aPos;
                                          uniform mat4 uMVP;
                                          void main()
                                          {
                                              gl_Position = uMVP * vec4(aPos, 0.0, 1.0);
                                          }
                                          """;

        const string fragmentShaderSource = """
                                            #version 330 core
                                            uniform vec4 uColor;
                                            out vec4 FragColor;
                                            void main()
                                            {
                                                FragColor = uColor;
                                            }
                                            """;

        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexShaderSource);
        GL.CompileShader(vertex);
        CheckShader(vertex);

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentShaderSource);
        GL.CompileShader(fragment);
        CheckShader(fragment);

        int shader = GL.CreateProgram();
        GL.AttachShader(shader, vertex);
        GL.AttachShader(shader, fragment);
        GL.LinkProgram(shader);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return shader;
    }

    private static void CheckShader(int shader)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status != 0)
        {
            return;
        }

        Console.WriteLine(GL.GetShaderInfoLog(shader));
        throw new Exception("Shader compilation failed!");
    }
}