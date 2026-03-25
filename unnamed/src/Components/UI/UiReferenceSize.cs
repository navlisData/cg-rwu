using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Defines the authored UI size in reference-space pixels.
/// </summary>
/// <param name="Width">The width in reference pixels.</param>
/// <param name="Height">The height in reference pixels.</param>
public readonly record struct UiReferenceSize(float Width, float Height)
{
    public Vector2 ToVector2()
    {
        return new Vector2(this.Width, this.Height);
    }
}