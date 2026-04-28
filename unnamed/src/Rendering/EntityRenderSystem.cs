using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Utils;

namespace unnamed.Rendering;

public class EntityRenderSystem() : EntitySetSystem<RenderContext.RenderContext>(
    new QueryBuilder()
        .With<VisibleEntity>()
        .With<Sprite>()
        .With<Position>()
        .With<Transform>()
        .Without<Sleeping>()
        .OrderWith(EntityOrder.ByPositionY)
        .Build())
{
    protected override void Update(ref RenderContext.RenderContext ctx, EntityHandle e)
    {
        ref Sprite sprite = ref e.Get<Sprite>();
        Vector2 position = e.Get<Position>().ToWorldPosition();
        ref Transform transform = ref e.Get<Transform>();
        Vector2 pivot = sprite.Frame.Pivot;

        ctx.BeginDraw().WithPositionAndTransform(position, transform, transform.Size, pivot).WithSprite(sprite.Frame)
            .WithoutColoration().Draw();
    }
}