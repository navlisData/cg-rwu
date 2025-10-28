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

public class ProjectileRenderingSystem(World world, AssetStore assets) : ExtendedEntitySetSystem<int, Camera2D>(
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
    private int mvpUniformLocation;
    private int texCoordLocation;
    private int vertexLocation;


    protected override bool BeforeUpdate(int shader)
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

        return false;
    }

    protected override void Update(Camera2D camera, in Entity e)
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

        Matrix4 modelSquare = Matrix4.CreateRotationZ(transform.Rotation) *
                              Matrix4.CreateScale(transform.Scale) *
                              Matrix4.CreateTranslation(position.X, position.Y + transform.Height, 0f);

        Matrix4 mvpSquare = modelSquare * camera.ViewProjection;

        GraphicsUtils.FillSpriteQuadGeometry(in transform.Size, in rect, in texture, in this.vertexScratch, true,
            true);

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