using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Defines the authored UI offset in reference-space units relative to the selected anchor.
/// </summary>
/// <param name="X">Horizontal offset in reference-space units.</param>
/// <param name="Y">Vertical offset in reference-space units.</param>
public readonly record struct UiReferenceOffset(float X = 0f, float Y = 0f)
{
    public Vector2 ToVector2()
    {
        return new Vector2(this.X, this.Y);
    }
}