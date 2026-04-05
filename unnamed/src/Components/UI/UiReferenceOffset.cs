using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Defines the authored UI offset in reference-space units relative to the selected anchor.
/// </summary>
/// <param name="X">Horizontal offset in reference-space units.</param>
/// <param name="Y">Vertical offset in reference-space units.</param>
public readonly record struct UiReferenceOffset(float X = 0f, float Y = 0f)
{
    public static implicit operator Vector2(UiReferenceOffset offset)
    {
        return new Vector2(offset.X, offset.Y);
    }
}