using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public class ShadowRenderSystem() : EntitySetSystem<RenderContext.RenderContext>(
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

    protected override void Update(ref RenderContext.RenderContext ctx, EntityHandle e)
    {
        ref Sprite sprite = ref e.Get<Sprite>();
        Vector2 position = e.Get<Position>().ToWorldPosition();
        ref Transform transform = ref e.Get<Transform>();

        Matrix4 distortion =
            Matrix4.CreateRotationZ(transform.Rotation) *
            Matrix4.CreateScale(transform.Scale * 0.75f) *
            Matrix4.CreateTranslation(0f, transform.Height, 0f) *
            this.shearMatrix;

        ctx.BeginDraw().WithPositionAndDistortion(position, distortion, transform.Size, sprite.Frame.Pivot)
            .WithSprite(sprite.Frame).WithColoration(this.shadow, 1f)
            .Draw();
    }
}