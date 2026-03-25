using OpenTK.Mathematics;

namespace unnamed.Layout;

/// <summary>
///     Contains the reference resolution and current viewport size used for UI layout.
/// </summary>
/// <param name="ReferenceResolution">The authored UI reference resolution.</param>
/// <param name="ViewportSize">The current viewport size in pixels.</param>
public readonly record struct UiLayoutContext(Vector2 ReferenceResolution, Vector2 ViewportSize)
{
    public Vector2 ReferenceResolution { get; } = ReferenceResolution;
    public Vector2 ViewportSize { get; } = ViewportSize;

    /// <summary>
    ///     Creates a layout context from integer viewport values.
    /// </summary>
    /// <param name="referenceResolution">The authored UI reference resolution.</param>
    /// <param name="viewportSize">The current viewport size in pixels.</param>
    public UiLayoutContext(Vector2i referenceResolution, Vector2i viewportSize)
        : this(new Vector2(referenceResolution.X, referenceResolution.Y),
            new Vector2(viewportSize.X, viewportSize.Y))
    {
    }
}