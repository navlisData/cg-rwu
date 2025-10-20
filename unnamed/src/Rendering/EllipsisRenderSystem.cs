using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public sealed class EllipsisRenderSystem(World world) : EntitySetSystem<float>(world, world.Query()
    .With<Position>()
    .With<Circle>()
    .With<Transform>()
    .Without<Hidden>()
    .Build()
)
{
    protected override void Update(float _, in Entity e)
    {
        ref Position position = ref e.Get<Position>();
        ref Transform transform = ref e.Get<Transform>();

        if (e.Has<ObjectColor>())
        {
            ref ObjectColor color = ref e.Get<ObjectColor>();
            GL.Color4(color.Rgba.X, color.Rgba.Y, color.Rgba.Z, color.Rgba.W);
        }
        else
        {
            GL.Color4(1f, 1f, 1f, 1f);
        }

        Vector2 radius = new(
            Math.Max(0.0001f, transform.Size.X),
            MathF.Max(0.0001f, transform.Size.Y)
        );

        GL.PushMatrix();
        GL.Translate(position.Value.X, position.Value.Y, 0.0);
        GL.Begin(PrimitiveType.TriangleFan);
        for (int i = 0; i < 360; i++)
        {
            double radians = Math.PI * i / 180.0;
            GL.Vertex2(Math.Cos(radians) * radius.Y, Math.Sin(radians) * radius.X);
        }

        GL.End();
        GL.PopMatrix();
    }
}