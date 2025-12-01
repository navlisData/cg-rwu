using System.Drawing;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.UI;
using unnamed.Utils;

namespace unnamed.Rendering;

public class UiRenderSystem(World world, IAssetStore assets)
    : ExtendedEntitySetSystem<(int shader, Vector2i windowSize), Vector2i>(
        world, world.Query()
            .With<AbsolutePosition>()
            .With<AbsoluteSize>()
            .With<UiAlignment>()
            .With<Sprite>()
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
            0, windowSize.X,
            windowSize.Y, 0,
            -1f, 1f
        );

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

        GL.BufferData(BufferTarget.ElementArrayBuffer, this.quadIndices.Length * sizeof(uint), this.quadIndices,
            BufferUsageHint.StaticDraw);
    }

    protected override void Update(Vector2i windowSize, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Sprite sprite = ref handle.Get<Sprite>();
        AbsolutePosition position = handle.Get<AbsolutePosition>().WrapToScreen(windowSize);
        Vector2 size = (Vector2)handle.Get<AbsoluteSize>();
        UiAlignment alignment = handle.Get<UiAlignment>();

        StaticSprite frame = sprite.Frame;
        Texture2D texture = assets.GetTextureById(frame.SpriteSheetId);
        RectangleF rect = frame.RectPx;

        Matrix4 modelSquare = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 mvpSquare = modelSquare * this.screenProjection;

        GraphicsUtils.FillSpriteQuadGeometry(in size, in rect, in texture, in this.vertexScratch,
            alignment.HorizontallyCentered,
            alignment.VerticallyCentered);

        GraphicsUtils.RenderSpriteQuad(texture.Handle, this.mvpUniformLocation, in this.vertexScratch,
            ref mvpSquare);
    }

    public void OnUnload()
    {
        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
    }
}