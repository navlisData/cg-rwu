using System.Drawing;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Texture;
using unnamed.Utils;

namespace unnamed.Rendering;

public class ShadowRenderSystem(World world, AssetStore assets) : ExtendedEntitySetSystem<int, Camera2D>(world,
    world.Query()
        .With<Sprite>()
        .With<Position>()
        .With<Transform>()
        .Without<Sleeping>()
        .Build())
{
    private readonly int elementBuffer = GL.GenBuffer();
    private readonly uint[] quadIndices = GraphicsUtils.QuadIndices;
    private readonly Color4 shadowColor = new(0f, 0f, 0f, 0.35f);
    private readonly Matrix4 shearMatrix = new(Vector4.UnitX, new Vector4(1.6f, 1, 0, 0), Vector4.UnitZ, Vector4.UnitW);

    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly float[] vertexScratch = new float[16];
    private int mvpUniformLocation;
    private int texCoordLocation;
    private int vertexLocation;

    protected override bool BeforeUpdate(int shader)
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

        return false;
    }

    protected override void Update(Camera2D camera, in Entity e)
    {
        ref Sprite sprite = ref e.Get<Sprite>();
        Vector2 position = e.Get<Position>().ToWorldPosition();
        ref Transform transform = ref e.Get<Transform>();

        StaticSprite frame = sprite.Frame;
        Texture2D texture = assets.GetTextureById(frame.SpriteSheetId);
        RectangleF rect = frame.RectPx;

        Matrix4 shadowModel =
            Matrix4.CreateRotationZ(transform.Rotation) *
            Matrix4.CreateScale(transform.Scale * 0.5f) *
            Matrix4.CreateTranslation(0f, transform.Height, 0f) *
            this.shearMatrix *
            Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 mvpSquare = shadowModel * camera.ViewProjection;

        GraphicsUtils.FillSpriteQuadGeometry(
            in transform.Size,
            in rect, in texture, in this.vertexScratch, true, e.Has<Projectile>());

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