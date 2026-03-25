using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Defines the authored UI offset in reference-space pixels.
///     The offset is applied relative to the selected anchor.
/// </summary>
/// <param name="X">The horizontal offset in reference pixels.</param>
/// <param name="Y">The vertical offset in reference pixels.</param>
public readonly record struct UiReferenceOffset(float X, float Y)
{
    public Vector2 ToVector2()
    {
        return new Vector2(this.X, this.Y);
    }
}