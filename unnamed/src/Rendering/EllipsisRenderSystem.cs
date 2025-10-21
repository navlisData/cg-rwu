using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public sealed class EllipsisRenderSystem : EntitySetSystem<(int shader, Camera2D camera)>
{
    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();
    private int ellipseVertexCount;

    public EllipsisRenderSystem(World world) : base(world, world.Query()
        .With<Position>()
        .With<Circle>()
        .With<Transform>()
        .Without<Hidden>()
        .Build())
    {
        GL.BindVertexArray(this.vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    protected override void Update((int shader, Camera2D camera) param, in Entity e)
    {
        ref Position position = ref e.Get<Position>();
        ref Transform transform = ref e.Get<Transform>();

        const int segments = 64;
        float[] ellipseVertices = new float[(segments + 2) * 2];
        ellipseVertices[0] = 0f;
        ellipseVertices[1] = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i / (float)segments * MathHelper.TwoPi;
            ellipseVertices[((i + 1) * 2) + 0] = MathF.Cos(angle) * transform.Size.X * transform.Scale;
            ellipseVertices[((i + 1) * 2) + 1] = MathF.Sin(angle) * transform.Size.Y * transform.Scale;
        }

        this.ellipseVertexCount = segments + 2;

        GL.BufferData(BufferTarget.ArrayBuffer, ellipseVertices.Length * sizeof(float), ellipseVertices,
            BufferUsageHint.StaticDraw);

        int vertexUniform = GL.GetUniformLocation(param.shader, "uMVP");
        int fragmentUniform = GL.GetUniformLocation(param.shader, "uColor");

        Matrix4 ellipseModel =
            Matrix4.CreateTranslation(position.Value.X, position.Value.Y, 0f);
        Matrix4 ellipseViewProjection = ellipseModel * param.camera.ViewProjection;
        GL.UniformMatrix4(vertexUniform, false, ref ellipseViewProjection);
        Vector4 color = e.Has<ObjectColor>() ? e.Get<ObjectColor>().Rgba : new Vector4(1f, 1f, 1f, 1f);
        GL.Uniform4(fragmentUniform, color);
        GL.DrawArrays(PrimitiveType.TriangleFan, 0, this.ellipseVertexCount);
    }

    public void onUnload()
    {
        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.vertexBuffer);
    }
}