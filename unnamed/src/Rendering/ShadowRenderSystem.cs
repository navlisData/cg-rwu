using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public class ShadowRenderSystem(World world) : EntitySetSystem<RenderContext.RenderContext>(world,
    new QueryBuilder()
        .With<Sprite>()
        .With<Position>()
        .With<Transform>()
        .With<HasShadow>()
        .With<VisibleEntity>()
        .Without<Sleeping>()
        .Build())
{
    private readonly Color4 shadow = new(0f, 0f, 0f, 0.35f);
    private readonly Matrix4 shearMatrix = new(Vector4.UnitX, new Vector4(0.6f, 1, 0, 0), Vector4.UnitZ, Vector4.UnitW);

    protected override void Update(RenderContext.RenderContext ctx, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Sprite sprite = ref handle.Get<Sprite>();
        Vector2 position = handle.Get<Position>().ToWorldPosition();
        ref Transform transform = ref handle.Get<Transform>();

        Matrix4 distortion =
            Matrix4.CreateRotationZ(transform.Rotation) *
            Matrix4.CreateScale(transform.Scale * 0.75f) *
            Matrix4.CreateTranslation(0f, transform.Height, 0f) *
            this.shearMatrix;

        ctx.BeginDraw().WithSprite(sprite.Frame).WithColoration(this.shadow, 1f)
            .WithPositionAndDistortion(position, distortion)
            .WithSize(transform.Size, true, handle.Has<Projectile>()).Draw();
    }
}