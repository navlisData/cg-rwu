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

public class ShadowRenderSystem(World world, IAssetStore assets) : ExtendedEntitySetSystem<int, Camera2D>(world,
    world.Query()
        .With<Sprite>()
        .With<Position>()
        .With<Transform>()
        .With<HasShadow>()
        .With<VisibleEntity>()
        .Without<Sleeping>()
        .Build())
{
    private readonly int elementBuffer = GL.GenBuffer();
    private readonly uint[] quadIndices = GraphicsUtils.QuadIndices;
    private readonly Color4 shadowColor = new(0f, 0f, 0f, 0.35f);
    private readonly Matrix4 shearMatrix = new(Vector4.UnitX, new Vector4(0.6f, 1, 0, 0), Vector4.UnitZ, Vector4.UnitW);

    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly float[] vertexScratch = new float[16];
    private int mvpUniformLocation;
    private int texCoordLocation;
    private int vertexLocation;

    protected override void BeforeUpdate(int shader)
    {
        GL.UseProgram(shader);
        GL.Uniform4(GL.GetUniformLocation(shader, "shadowColor"), this.shadowColor);

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

        Matrix4 shadowModel =
            Matrix4.CreateRotationZ(transform.Rotation) *
            Matrix4.CreateScale(transform.Scale * 0.75f) *
            Matrix4.CreateTranslation(0f, transform.Height, 0f) *
            this.shearMatrix *
            Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 mvpSquare = shadowModel * camera.ViewProjection;

        GraphicsUtils.FillSpriteQuadGeometry(
            in transform.Size,
            in rect, in texture, in this.vertexScratch, true, handle.Has<Projectile>());

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