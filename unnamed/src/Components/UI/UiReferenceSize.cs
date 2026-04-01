using OpenTK.Mathematics;

namespace unnamed.Components.UI;

/// <summary>
///     Defines the authored UI size in reference-space units.
/// </summary>
/// <param name="Width">Width in reference-space units.</param>
/// <param name="Height">Height in reference-space units.</param>
public readonly record struct UiReferenceSize(float Width, float Height)
{
    public Vector2 ToVector2()
    {
        return new Vector2(this.Width, this.Height);
    }
}