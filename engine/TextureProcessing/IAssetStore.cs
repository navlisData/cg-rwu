using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace engine.TextureProcessing;

public interface IAssetStore
{
    /// <summary>
    /// Loads a texture from <paramref name="path"/> if not already loaded and returns its stable ID.
    /// </summary>
    /// <param name="path">File path to the image (absolute or relative to the app base directory).</param>
    /// <param name="generateMipmaps">True to generate mipmaps after upload; false otherwise.</param>
    /// <returns>A <see cref="TextureId"/> referencing the loaded texture.</returns>
    TextureId LoadTexture(string path, bool generateMipmaps = true);

    /// <summary>
    /// Tries to retrieve a previously loaded texture by ID.
    /// </summary>
    /// <param name="id">The texture identifier.</param>
    /// <param name="texture">Receives the texture when found; otherwise null.</param>
    /// <returns>True if the texture exists; otherwise false.</returns>
    bool TryGetTexture(TextureId id, [MaybeNullWhen(false)] out Texture2D texture);

    /// <summary>
    /// Loads a sprite sheet for the texture at <paramref name="texturePath"/> using the provided named frame rectangles.
    /// </summary>
    /// <param name="texturePath">Path to the sprite sheet image.</param>
    /// <param name="frames">Mapping of frame names to pixel rectangles within the sheet.</param>
    /// <returns>A <see cref="SpriteSheetId"/> referencing the loaded sprite sheet.</returns>
    SpriteSheetId LoadSpriteSheet(string texturePath, IReadOnlyDictionary<string, RectangleF> frames);
    
    /// <summary>
    /// Retrieves a previously loaded sprite sheet by ID.
    /// </summary>
    /// <param name="id">The sprite sheet identifier.</param>
    /// <returns>The corresponding <see cref="SpriteSheet"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sheet is not loaded or already disposed.</exception>
    SpriteSheet GetSpriteSheet(SpriteSheetId id);
    
    /// <summary>
    /// Resolves a frame within a sprite sheet to a frame identifier.
    /// </summary>
    /// <param name="sheet">The sprite sheet identifier.</param>
    /// <param name="name">The frame name.</param>
    /// <returns>A <see cref="SpriteFrameId"/> pointing to the frame within the sheet.</returns>
    SpriteFrameId GetFrame(SpriteSheetId sheet, string name);
}