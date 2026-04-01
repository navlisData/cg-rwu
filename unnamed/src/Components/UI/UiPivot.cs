using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Defines a normalized pivot inside the element itself.
///     (0, 0) is top-left, (0.5, 0.5) is center, (1, 1) is bottom-right.
/// </summary>
/// <param name="X">Horizontal pivot in normalized local space.</param>
/// <param name="Y">Vertical pivot in normalized local space.</param>
public readonly record struct UiPivot(float X, float Y)
{
    public static readonly UiPivot TopLeft = new(0f, 0f);
    public static readonly UiPivot TopCenter = new(0.5f, 0f);
    public static readonly UiPivot TopRight = new(1f, 0f);
    public static readonly UiPivot CenterLeft = new(0f, 0.5f);
    public static readonly UiPivot Center = new(0.5f, 0.5f);
    public static readonly UiPivot CenterRight = new(1f, 0.5f);
    public static readonly UiPivot BottomLeft = new(0f, 1f);
    public static readonly UiPivot BottomCenter = new(0.5f, 1f);
    public static readonly UiPivot BottomRight = new(1f, 1f);

    public Vector2 ToVector2()
    {
        return new Vector2(this.X, this.Y);
    }
}