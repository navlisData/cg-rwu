using engine.TextureProcessing;

using OpenTK.Mathematics;

namespace unnamed.Components.Rendering;

public readonly struct StaticTextTexture(Texture2D texture, Vector2 pivot)
{
    public readonly Texture2D Texture = texture;
    public readonly Vector2 Pivot = pivot;

    public Vector2i Size => this.Texture.Size;
}