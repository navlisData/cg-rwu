using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.UI;

namespace unnamed.Components.Rendering;

public readonly struct StaticTextTexture(Texture2D texture, UiPivot pivot)
{
    public readonly Texture2D Texture = texture;
    public readonly UiPivot UiPivot = pivot;

    public Vector2i Size => this.Texture.Size;
}