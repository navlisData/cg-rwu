using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public class ProjectileRenderingSystem() : EntitySetSystem<RenderContext.RenderContext>(
    new QueryBuilder()
        .With<Projectile>()
        .With<Sprite>()
        .With<Position>()
        .With<Transform>()
        .Without<Sleeping>()
        .Build())
{
    protected override void Update(ref RenderContext.RenderContext ctx, EntityHandle e)
    {
        ref Sprite sprite = ref e.Get<Sprite>();
        Vector2 position = e.Get<Position>().ToWorldPosition();
        ref Transform transform = ref e.Get<Transform>();

        ctx.BeginDraw().WithPositionAndTransform(position, transform, transform.Size, sprite.Frame.Pivot)
            .WithSprite(sprite.Frame).WithoutColoration()
            .Draw();
    }
}