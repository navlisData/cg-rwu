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

public class ProjectileRenderingSystem(World world, AssetStore assets) : EntitySetSystem<(int shader, Camera2D camera)>(
    world,
    world.Query()
        .With<Projectile>()
        .With<Sprite>()
        .With<Position>()
        .With<Transform>()
        .With<Velocity>()
        .Without<Sleeping>()
        .Build())
{
    private readonly int elementBuffer = GL.GenBuffer();
    private readonly uint[] quadIndices = GraphicsUtils.QuadIndices;

    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly float[] vertexScratch = new float[16];

    protected override void Update((int shader, Camera2D camera) param, in Entity e)
    {
        ref Sprite sprite = ref e.Get<Sprite>();
        Vector2 position = e.Get<Position>().ToWorldPosition();
        ref Transform transform = ref e.Get<Transform>();

        SpriteSheet spriteSheet = assets.GetSpriteSheet(sprite.Frame.Sheet);
        if (!assets.TryGetTexture(spriteSheet.Texture, out Texture2D? texture))
        {
            return;
        }

        RectangleF rect = spriteSheet.Frames[sprite.Frame.Index];

        GraphicsUtils.FillSpriteQuadGeometry(in transform.Size, in rect, in texture, in this.vertexScratch, true,
            true);

        GL.BindVertexArray(this.vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, this.vertexScratch.Length * sizeof(float), this.vertexScratch,
            BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, this.quadIndices.Length * sizeof(uint), this.quadIndices,
            BufferUsageHint.StaticDraw);

        int vertexLocation = GL.GetAttribLocation(param.shader, "aPosition");
        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        int texCoordLocation = GL.GetAttribLocation(param.shader, "aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
            2 * sizeof(float));


        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
        GL.ActiveTexture(TextureUnit.Texture0);

        Matrix4 modelSquare = Matrix4.CreateRotationZ(transform.Rotation) *
                              Matrix4.CreateScale(transform.Scale) *
                              Matrix4.CreateTranslation(position.X, position.Y + transform.Height, 0f);

        Matrix4 mvpSquare = modelSquare * param.camera.ViewProjection;

        int mvpUniformLocation = GL.GetUniformLocation(param.shader, "uMVP");
        GL.UniformMatrix4(mvpUniformLocation, false, ref mvpSquare);

        GL.BindVertexArray(this.vertexArray);
        GL.DrawElements(PrimitiveType.Triangles, this.quadIndices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void OnUnload()
    {
        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
    }
}