using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Defines a normalized pivot inside the element itself.
///     (0, 0) is top-left, (0.5, 0.5) is centered, (1, 1) is bottom-right.
/// </summary>
/// <param name="X">The horizontal pivot in normalized local space.</param>
/// <param name="Y">The vertical pivot in normalized local space.</param>
public readonly record struct UiPivot(float X, float Y)
{
    public static readonly UiPivot TopLeft = new(0f, 0f);
    public static readonly UiPivot Center = new(0.5f, 0.5f);
    public static readonly UiPivot BottomRight = new(1f, 1f);

    public Vector2 ToVector2()
    {
        return new Vector2(this.X, this.Y);
    }
}