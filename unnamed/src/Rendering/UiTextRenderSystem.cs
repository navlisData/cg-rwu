using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.UI;
using unnamed.Utils;

namespace unnamed.Rendering;

public sealed class UiTextRenderSystem(World world)
    : ExtendedEntitySetSystem<(int shader, Vector2i windowSize), Vector2i>(
        world,
        new QueryBuilder()
            .With<AbsolutePosition>()
            .With<UiAlignment>()
            .With<StaticTextTexture>()
            .Build())
{
    private readonly int elementBuffer = GL.GenBuffer();
    private readonly uint[] quadIndices = GraphicsUtils.QuadIndices;
    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly float[] vertexScratch = new float[16];

    private int mvpUniformLocation;
    private Matrix4 screenProjection;
    private int texCoordLocation;
    private int vertexLocation;

    protected override void BeforeUpdate((int shader, Vector2i windowSize) args)
    {
        (int shader, Vector2i windowSize) = args;

        GL.UseProgram(shader);

        this.screenProjection = Matrix4.CreateOrthographicOffCenter(
            0,
            windowSize.X,
            0,
            windowSize.Y,
            -1f,
            1f);

        GL.BindVertexArray(this.vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);

        this.vertexLocation = GL.GetAttribLocation(shader, "aPosition");
        this.texCoordLocation = GL.GetAttribLocation(shader, "aTexCoord");
        this.mvpUniformLocation = GL.GetUniformLocation(shader, "uMVP");

        GL.EnableVertexAttribArray(this.vertexLocation);
        GL.EnableVertexAttribArray(this.texCoordLocation);

        GL.VertexAttribPointer(this.vertexLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.VertexAttribPointer(this.texCoordLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
            2 * sizeof(float));

        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            this.quadIndices.Length * sizeof(uint),
            this.quadIndices,
            BufferUsageHint.StaticDraw);
    }

    protected override void Update(Vector2i windowSize, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref readonly StaticTextTexture text = ref handle.Get<StaticTextTexture>();

        AbsolutePosition position = handle.Get<AbsolutePosition>();
        position.Y = windowSize.Y - position.Y;
        position = position.WrapToScreen(windowSize);

        UiAlignment alignment = handle.Get<UiAlignment>();
        Vector2 size = new(text.Size.X, text.Size.Y);

        Matrix4 model = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 mvp = model * this.screenProjection;

        GraphicsUtils.FillSolidQuadGeometry(
            in size,
            this.vertexScratch,
            alignment.HorizontallyCentered,
            alignment.VerticallyCentered);

        GraphicsUtils.RenderSpriteQuad(
            text.Texture.Handle,
            this.mvpUniformLocation,
            in this.vertexScratch,
            ref mvp);
    }

    public void OnUnload()
    {
        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
    }
}