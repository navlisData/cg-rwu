using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Utils;

namespace unnamed.Rendering;

public class EntityRenderSystem(World world) : EntitySetSystem<RenderContext.RenderContext>(
    world, new QueryBuilder()
        .With<VisibleEntity>()
        .With<Sprite>()
        .With<Position>()
        .With<Transform>()
        .Without<Sleeping>()
        .OrderWith(EntityOrder.ByPositionY)
        .Build())
{
    protected override void Update(RenderContext.RenderContext ctx, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Sprite sprite = ref handle.Get<Sprite>();
        Vector2 position = handle.Get<Position>().ToWorldPosition();
        ref Transform transform = ref handle.Get<Transform>();
        Vector2 pivot = sprite.Frame.Pivot;

        ctx.BeginDraw().WithSprite(sprite.Frame)
            .WithoutColoration()
            .WithPositionAndTransform(position, transform, transform.Size, pivot)
            .WithUnitQuad().Draw();
    }
}