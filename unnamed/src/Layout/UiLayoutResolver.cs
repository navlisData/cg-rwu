using OpenTK.Mathematics;

using unnamed.Components.UI;

namespace unnamed.Layout;

public static class UiLayoutResolver
{
    /// <summary>
    ///     Resolves an authored UI element into absolute render-space position and size.
    /// </summary>
    /// <param name="context">The current UI layout context.</param>
    /// <param name="referenceSize">The authored size in reference pixels.</param>
    /// <param name="referenceOffset">The authored offset in reference pixels.</param>
    /// <param name="anchor">The normalized screen anchor.</param>
    /// <param name="pivot">The normalized local pivot.</param>
    /// <param name="scaleMode">The scaling mode.</param>
    /// <returns>The resolved render-space rectangle.</returns>
    public static UiResolvedRect Resolve(
        in UiLayoutContext context,
        in UiReferenceSize referenceSize,
        in UiReferenceOffset referenceOffset,
        in UiAnchor anchor,
        in UiPivot pivot,
        UiScaleMode scaleMode)
    {
        Vector2 scale = ComputeScale(context, scaleMode);

        Vector2 authoredSize = referenceSize.ToVector2();
        Vector2 authoredOffset = referenceOffset.ToVector2();

        Vector2 finalSize = authoredSize * scale;

        Vector2 anchorPosition = new(
            context.ViewportSize.X * anchor.X,
            context.ViewportSize.Y * anchor.Y);

        Vector2 finalTopLeftPosition =
            anchorPosition +
            (authoredOffset * scale) -
            (finalSize * pivot.ToVector2());

        Vector2 renderPosition = new(
            finalTopLeftPosition.X,
            context.ViewportSize.Y - finalTopLeftPosition.Y - finalSize.Y);

        return new UiResolvedRect(renderPosition, finalSize);
    }

    /// <summary>
    ///     Computes the scale vector for the current layout context.
    /// </summary>
    /// <param name="context">The current UI layout context.</param>
    /// <param name="scaleMode">The scaling mode.</param>
    /// <returns>The scale vector.</returns>
    private static Vector2 ComputeScale(in UiLayoutContext context, UiScaleMode scaleMode)
    {
        float scaleX = context.ViewportSize.X / context.ReferenceResolution.X;
        float scaleY = context.ViewportSize.Y / context.ReferenceResolution.Y;

        return scaleMode switch
        {
            UiScaleMode.None => Vector2.One,
            UiScaleMode.Stretch => new Vector2(scaleX, scaleY),
            UiScaleMode.Uniform => CreateUniformScale(scaleX, scaleY),
            _ => Vector2.One
        };
    }

    /// <summary>
    ///     Creates a uniform scale vector using the smaller axis scale.
    /// </summary>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <returns>A uniform scale vector.</returns>
    private static Vector2 CreateUniformScale(float scaleX, float scaleY)
    {
        float uniformScale = MathF.Min(scaleX, scaleY);
        return new Vector2(uniformScale, uniformScale);
    }
}