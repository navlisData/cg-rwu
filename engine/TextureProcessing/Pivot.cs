using OpenTK.Mathematics;

namespace engine.TextureProcessing;

/// <summary>
///     Defines a normalized pivot inside the element itself.
///     (0, 0) is top-left, (0.5, 0.5) is center, (1, 1) is bottom-right.
/// </summary>
/// <param name="X">Horizontal pivot in normalized local space.</param>
/// <param name="Y">Vertical pivot in normalized local space.</param>
public readonly record struct Pivot(float X, float Y)
{
    public static readonly Pivot TopLeft = new(0f, 0f);
    public static readonly Pivot TopCenter = new(0.5f, 0f);
    public static readonly Pivot TopRight = new(1f, 0f);
    public static readonly Pivot CenterLeft = new(0f, 0.5f);
    public static readonly Pivot Center = new(0.5f, 0.5f);
    public static readonly Pivot CenterRight = new(1f, 0.5f);
    public static readonly Pivot BottomLeft = new(0f, 1f);
    public static readonly Pivot BottomCenter = new(0.5f, 1f);
    public static readonly Pivot BottomRight = new(1f, 1f);

    public static implicit operator Vector2(Pivot pivot)
    {
        return new Vector2(pivot.X, pivot.Y);
    }
}