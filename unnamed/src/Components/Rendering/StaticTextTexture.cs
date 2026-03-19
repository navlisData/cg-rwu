using engine.TextureProcessing;

using OpenTK.Mathematics;

namespace unnamed.Components.Rendering;

public readonly struct StaticTextTexture
{
    public Texture2D Texture { get; }

    public Vector2i Size => this.Texture.Size;

    /// <summary>
    ///     Initializes the component with a pre-rasterized text texture.
    /// </summary>
    /// <param name="texture">The texture that contains the rendered text.</param>
    public StaticTextTexture(Texture2D texture)
    {
        this.Texture = texture ?? throw new ArgumentNullException(nameof(texture));
    }
}