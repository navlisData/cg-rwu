using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.UI;

namespace unnamed.Rendering;

public class UiRenderSystem(World world, IAssetStore assets)
    : EntitySetSystem<RenderContext.RenderContext>(world,
        new QueryBuilder()
            .With<AbsolutePosition>()
            .With<AbsoluteSize>()
            .With<UiAlignment>()
            .WithAny<Sprite, StaticTextTexture>()
            .Build())
{
    protected override void Update(RenderContext.RenderContext ctx, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref AbsolutePosition position = ref handle.Get<AbsolutePosition>();
        ref UiAlignment alignment = ref handle.Get<UiAlignment>();
        Vector2 size = handle.Get<AbsoluteSize>();

        if (handle.Has<Sprite>())
        {
            ref Sprite sprite = ref handle.Get<Sprite>();
            ctx.BeginDraw().WithSprite(sprite.Frame).WithColoration(in sprite.Tint, 1f).WithAbsolutePosition(position)
                .WithAbsoluteSize(size, alignment).Draw();
        }
        else if (handle.Has<StaticTextTexture>())
        {
            ref StaticTextTexture text = ref handle.Get<StaticTextTexture>();
            ctx.BeginDraw().WithText(text).WithoutColoration().WithAbsolutePosition(position)
                .WithAbsoluteSize(size, alignment).Draw();
        }
    }
}