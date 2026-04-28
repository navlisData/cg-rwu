using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;
using unnamed.Rendering.RenderContext;

namespace unnamed.Rendering;

public class UiRenderSystem()
    : EntitySetSystem<RenderContext.RenderContext>(
        new QueryBuilder()
            .With<UiElement>()
            .WithAny<AbsoluteSize, UiReferenceSize>()
            .WithAny<AbsolutePosition, UiReferenceOffset>()
            .WithAny<Sprite, StaticTextTexture>()
            .Build())
{
    protected override void Update(ref RenderContext.RenderContext ctx, EntityHandle e)
    {
        bool isReferenceUi =
            e.Has<UiReferenceOffset>() &&
            e.Has<UiReferenceSize>() &&
            e.Has<UiAnchor>() &&
            e.Has<UiScaleMode>();

        bool isAbsoluteUi =
            e.Has<AbsolutePosition>() &&
            e.Has<AbsoluteSize>();

        if (!isReferenceUi && !isAbsoluteUi)
        {
            return;
        }

        if (e.Has<Sprite>())
        {
            ref Sprite sprite = ref e.Get<Sprite>();
            RenderUi(ctx.BeginUi(), e, sprite.Frame.Pivot, isReferenceUi)
                .WithSprite(sprite.Frame)
                .WithColoration(in sprite.Tint, 1f)
                .Draw();
            return;
        }

        if (e.Has<StaticTextTexture>())
        {
            ref StaticTextTexture text = ref e.Get<StaticTextTexture>();
            RenderUi(ctx.BeginUi(), e, text.Pivot, isReferenceUi)
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