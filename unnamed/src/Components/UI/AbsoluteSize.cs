using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Represents the absolute pixel dimensions of an element on the screen.
/// </summary>
/// <param name="width">The horizontal size in pixels.</param>
/// <param name="height">The vertical size in pixels.</param>
public struct AbsoluteSize(float width, float height)
{
    public float Height = height;
    public float Width = width;

    public static explicit operator AbsoluteSize(Vector2 size)
    {
        return new AbsoluteSize(size.X, size.Y);
    }

    public static explicit operator Vector2(AbsoluteSize size)
    {
        return new Vector2(size.Width, size.Height);
    }
}