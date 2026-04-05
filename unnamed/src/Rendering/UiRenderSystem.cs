using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;
using unnamed.Rendering.RenderContext;

namespace unnamed.Rendering;

public class UiRenderSystem(World world, IAssetStore assets)
    : EntitySetSystem<RenderContext.RenderContext>(world,
        new QueryBuilder()
            .With<UiElement>()
            .WithAny<AbsoluteSize, UiReferenceSize>()
            .WithAny<AbsolutePosition, UiReferenceOffset>()
            .WithAny<Sprite, StaticTextTexture>()
            .Build())
{
    protected override void Update(RenderContext.RenderContext ctx, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        bool isReferenceUi =
            handle.Has<UiReferenceOffset>() &&
            handle.Has<UiReferenceSize>() &&
            handle.Has<UiAnchor>() &&
            handle.Has<UiScaleMode>();

        bool isAbsoluteUi =
            handle.Has<AbsolutePosition>() &&
            handle.Has<AbsoluteSize>();

        if (!isReferenceUi && !isAbsoluteUi)
        {
            return;
        }

        if (handle.Has<Sprite>())
        {
            ref Sprite sprite = ref handle.Get<Sprite>();
            RenderUi(ctx.BeginUi(), handle, sprite.Frame.Pivot, isReferenceUi)
                .WithSprite(sprite.Frame)
                .WithColoration(in sprite.Tint, 1f)
                .Draw();
            return;
        }

        if (handle.Has<StaticTextTexture>())
        {
            ref StaticTextTexture text = ref handle.Get<StaticTextTexture>();
            RenderUi(ctx.BeginUi(), handle, text.Pivot, isReferenceUi)
                .WithSprite(text)
                .WithoutColoration()
                .Draw();
        }
    }

    private static ISpriteStep RenderUi(
        IProjectionUiStep projectionStep,
        EntityHandle handle,
        Vector2 pivot,
        bool isReferenceUi)
    {
        if (isReferenceUi)
        {
            ref UiReferenceOffset referenceOffset = ref handle.Get<UiReferenceOffset>();
            ref UiReferenceSize referenceSize = ref handle.Get<UiReferenceSize>();
            ref UiAnchor anchor = ref handle.Get<UiAnchor>();
            ref UiScaleMode scaleMode = ref handle.Get<UiScaleMode>();

            return projectionStep.WithReferencePosition(
                in referenceOffset,
                in referenceSize,
                in pivot,
                in anchor,
                scaleMode);
        }

        ref AbsolutePosition position = ref handle.Get<AbsolutePosition>();
        ref AbsoluteSize size = ref handle.Get<AbsoluteSize>();

        return projectionStep.WithAbsolutePosition(in position, in size, in pivot);
    }
}