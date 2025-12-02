using System.Drawing;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Utils;

namespace unnamed.Rendering;

public class EntityRenderSystem(World world, IAssetStore assets) : ExtendedEntitySetSystem<int, Camera2D>(
    world, world.Query()
        .With<VisibleEntity>()
        .With<Sprite>()
        .With<Position>()
        .With<Transform>()
        .Without<Sleeping>()
        .Build())
{
    private readonly int elementBuffer = GL.GenBuffer();
    private readonly uint[] quadIndices = GraphicsUtils.QuadIndices;
    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly float[] vertexScratch = new float[16];
    private int mvpUniformLocation;
    private int texCoordLocation;
    private int vertexLocation;

    protected override void BeforeUpdate(int shader)
    {
        GL.UseProgram(shader);

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

    protected override void Update(Camera2D camera, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Sprite sprite = ref handle.Get<Sprite>();
        Vector2 position = handle.Get<Position>().ToWorldPosition();
        ref Transform transform = ref handle.Get<Transform>();

        StaticSprite frame = sprite.Frame;
        Texture2D texture = assets.GetTextureById(frame.SpriteSheetId);
        RectangleF rect = frame.RectPx;

        Matrix4 modelSquare = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 mvpSquare = modelSquare * camera.ViewProjection;

        Vector2 scaledSize = transform.Size * transform.Scale;
        GraphicsUtils.FillSpriteQuadGeometry(in scaledSize, in rect, in texture, in this.vertexScratch, true,
            false);

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