using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Defines a normalized screen anchor.
///     (0, 0) is top-left, (1, 1) is bottom-right.
/// </summary>
/// <param name="X">Horizontal anchor in normalized screen space.</param>
/// <param name="Y">Vertical anchor in normalized screen space.</param>
public readonly record struct UiAnchor(float X, float Y)
{
    public static readonly UiAnchor TopLeft = new(0f, 0f);
    public static readonly UiAnchor TopCenter = new(0.5f, 0f);
    public static readonly UiAnchor TopRight = new(1f, 0f);
    public static readonly UiAnchor CenterLeft = new(0f, 0.5f);
    public static readonly UiAnchor Center = new(0.5f, 0.5f);
    public static readonly UiAnchor CenterRight = new(1f, 0.5f);
    public static readonly UiAnchor BottomLeft = new(0f, 1f);
    public static readonly UiAnchor BottomCenter = new(0.5f, 1f);
    public static readonly UiAnchor BottomRight = new(1f, 1f);

    public Vector2 ToVector2()
    {
        return new Vector2(this.X, this.Y);
    }
}