using System.Diagnostics;

using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

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
            handle.Has<UiPivot>() &&
            handle.Has<UiScaleMode>();

        bool isAbsoluteUi =
            handle.Has<AbsolutePosition>() &&
            handle.Has<AbsoluteSize>();

        Debug.Assert(isReferenceUi || isAbsoluteUi);
        if (!isReferenceUi && !isAbsoluteUi)
        {
            return;
        }

        if (handle.Has<Sprite>())
        {
            ref Sprite sprite = ref handle.Get<Sprite>();
            this.DrawSprite(ctx, handle, sprite, isReferenceUi);
            return;
        }

        if (handle.Has<StaticTextTexture>())
        {
            ref StaticTextTexture text = ref handle.Get<StaticTextTexture>();
            this.DrawText(ctx, handle, text, isReferenceUi);
        }
    }

    private void DrawSprite(
        RenderContext.RenderContext ctx,
        EntityHandle handle,
        in Sprite sprite,
        bool isReferenceUi)
    {
        var draw = ctx.BeginDraw()
            .WithSprite(sprite.Frame)
            .WithColoration(in sprite.Tint, 1f);

        this.ApplyUiTransform(draw, handle, isReferenceUi)
            .WithUiUnitQuad()
            .Draw();
    }

    private void DrawText(
        RenderContext.RenderContext ctx,
        EntityHandle handle,
        in StaticTextTexture text,
        bool isReferenceUi)
    {
        var draw = ctx.BeginDraw()
            .WithText(text)
            .WithoutColoration();

        this.ApplyUiTransform(draw, handle, isReferenceUi)
            .WithUiUnitQuad()
            .Draw();
    }

    private IVerticesAbsoluteStep ApplyUiTransform(
        IProjectionStep projectionStep,
        EntityHandle handle,
        bool isReferenceUi)
    {
        if (isReferenceUi)
        {
            ref UiReferenceOffset referenceOffset = ref handle.Get<UiReferenceOffset>();
            ref UiReferenceSize referenceSize = ref handle.Get<UiReferenceSize>();
            ref UiAnchor anchor = ref handle.Get<UiAnchor>();
            ref UiPivot pivot = ref handle.Get<UiPivot>();
            ref UiScaleMode scaleMode = ref handle.Get<UiScaleMode>();

            return projectionStep.WithReferenceUiTransform(
                in referenceOffset,
                in referenceSize,
                in anchor,
                in pivot,
                scaleMode);
        }

        ref AbsolutePosition position = ref handle.Get<AbsolutePosition>();
        ref AbsoluteSize size = ref handle.Get<AbsoluteSize>();

        UiPivot pivotForAbsolute = handle.Has<UiPivot>()
            ? handle.Get<UiPivot>()
            : UiPivot.TopLeft;

        return projectionStep.WithAbsoluteUiTransform(in position, in size, in pivotForAbsolute);
    }
}