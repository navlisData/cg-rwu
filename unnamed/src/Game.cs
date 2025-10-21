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
    private readonly FollowingSystem followSystem;

    private readonly MoveSystem move;
    private readonly PlayerInputSystem playerInput;
    private readonly uint[] quadIndices = [0, 1, 2, 2, 3, 0];
    private readonly float[] quadVertices = [-0.5f, -0.5f, 0.5f, -0.5f, 0.5f, 0.5f, -0.5f, 0.5f];
    private readonly World world = new();

    private Entity camera;
    private int elementBuffer;
    private EllipsisRenderSystem ellipsisRenderer;
    private Entity player;
    private int shaderProgram;
    private int vertexArray;
    private int vertexBuffer;

    public Game() : base(NativeSettings, Settings)
    {
        this.move = new MoveSystem(this.world);
        this.playerInput = new PlayerInputSystem(this.world, () => this.KeyboardState);
        this.followSystem = new FollowingSystem(this.world);
        this.cameraSystem = new CameraSystem(this.world);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);
        this.vertexArray = GL.GenVertexArray();
        this.vertexBuffer = GL.GenBuffer();
        this.elementBuffer = GL.GenBuffer();

        GL.BindVertexArray(this.vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, this.quadVertices.Length * sizeof(float), this.quadVertices,
            BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, this.quadIndices.Length * sizeof(uint), this.quadIndices,
            BufferUsageHint.StaticDraw);

        this.shaderProgram = SetupShader();
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        this.ellipsisRenderer = new EllipsisRenderSystem(this.world, this.shaderProgram);

        this.player = PrefabFactory.CreatePlayer(this.world,
            new Vector2(0, 0),
            new Vector2(0f, 0f),
            new Vector2(0.05f, 0.05f)
        );

        this.camera =
            PrefabFactory.CreateFollowingCamera(this.world, this.player, InitialGameSize.X, InitialGameSize.Y);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        ref Camera2D cameraPos = ref this.camera.Get<Camera2D>();
        cameraPos.Zoom *= (float)Math.Pow(1.1f, e.OffsetY);
        cameraPos.Zoom = Math.Clamp(cameraPos.Zoom, 0.1f, 5.0f);
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

        int mvpUniformLocation = GL.GetUniformLocation(this.shaderProgram, "uMVP");
        int colorUniformLocation = GL.GetUniformLocation(this.shaderProgram, "uColor");

        Matrix4 modelSquare = Matrix4.Identity;
        Matrix4 mvpSquare = modelSquare * cameraPosition.ViewProjection;
        GL.UniformMatrix4(mvpUniformLocation, false, ref mvpSquare);
        GL.Uniform4(colorUniformLocation, new Vector4(1f, 0f, 0f, 1f));

        GL.BindVertexArray(this.vertexArray);
        GL.DrawElements(PrimitiveType.Triangles, this.quadIndices.Length, DrawElementsType.UnsignedInt, 0);

        this.ellipsisRenderer.Run(cameraPosition);

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

        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
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