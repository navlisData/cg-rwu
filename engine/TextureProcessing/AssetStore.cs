using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace engine.TextureProcessing;

public sealed class AssetStore : IAssetStore, IDisposable
{
    /// <summary>
    /// Maps an absolute/normalized texture path to its stable <see cref="TextureId"/>.
    /// Prevents loading the same image multiple times.
    /// </summary>
    private readonly Dictionary<string, TextureId> pathToTextureId = new();

    /// <summary>
    /// Storage for loaded textures; index equals <see cref="TextureId.Value"/>.
    /// Non-nullable list (no holes); entries are disposed in <see cref="Dispose"/>.
    /// </summary>
    private readonly List<Texture2D> textures = new();
    
    /// <summary>
    /// Maps a sprite sheet image path to its stable <see cref="SpriteSheetId"/>.
    /// Prevents constructing duplicate sheets for the same PNG path.
    /// </summary>
    private readonly Dictionary<string, SpriteSheetId> pathToSheetId = new();
    
    /// <summary>
    /// Storage for loaded sprite sheets; index equals <see cref="SpriteSheetId.Value"/>.
    /// Cleared on <see cref="Dispose"/>.
    /// </summary>
    private readonly List<SpriteSheet> spriteSheets = new();
    
    /// <summary>
    /// Loads a texture from <paramref name="path"/> if not already loaded and returns its stable ID.
    /// Reuses the existing ID for duplicate paths.
    /// </summary>
    /// <param name="path">Image file path (absolute or resolved by your app).</param>
    /// <param name="generateMipmaps">True to generate mipmaps after upload; false otherwise.</param>
    /// <returns>Stable <see cref="TextureId"/> for the texture.</returns>
    public TextureId LoadTexture(string path, bool generateMipmaps = true)
    {
        if (this.pathToTextureId.TryGetValue(path, out var id))
            return id;

        var tex = new Texture2D(path, generateMipmaps);
        var newId = new TextureId(this.textures.Count);
        this.textures.Add(tex);
        this.pathToTextureId[path] = newId;
        return newId;
    }
    
    /// <summary>
    /// Loads a sprite sheet for the given texture path using the provided named frame rectangles.
    /// If the sheet for <paramref name="texturePath"/> already exists, the existing ID is returned.
    /// The underlying texture is loaded without mipmaps to reduce atlas bleeding unless later configured otherwise.
    /// </summary>
    /// <param name="texturePath">Path to the sprite sheet image.</param>
    /// <param name="frames">Mapping of frame names to pixel rectangles within the sheet.</param>
    /// <returns>Stable <see cref="SpriteSheetId"/> for the sprite sheet.</returns>
    public SpriteSheetId LoadSpriteSheet(string texturePath, IReadOnlyDictionary<string, RectangleF> frames)
    {
        if (this.pathToSheetId.TryGetValue(texturePath, out var spriteSheetId))
            return spriteSheetId;

        TextureId textureId = this.LoadTexture(texturePath, false);

        SpriteSheet sheet = new SpriteSheet { Texture = textureId, NameToIndex = new Dictionary<string, int>() };

        int index = 0;
        foreach (var kv in frames)
        {
            sheet.NameToIndex![kv.Key] = index++;
            sheet.Frames.Add(kv.Value);
        }

        var newId = new SpriteSheetId(this.spriteSheets.Count);
        sheet.Id = newId;
        this.spriteSheets.Add(sheet);
        this.pathToSheetId[texturePath] = newId;
        return newId;
    }

    /// <summary>
    /// Tries to get a texture by ID without throwing.
    /// </summary>
    /// <param name="id">The texture identifier.</param>
    /// <param name="texture">Receives the texture when found; otherwise null.</param>
    /// <returns>True if the texture exists; otherwise false.</returns>
    public bool TryGetTexture(TextureId id, [MaybeNullWhen(false)] out Texture2D texture)
    {
        if (id.Value >= this.textures.Count)
        {
            texture = null;
            return false;
        }

        texture = this.textures[id.Value];
        return true;
    }

    /// <summary>
    /// Gets a sprite sheet by ID or throws if it is missing.
    /// </summary>
    /// <param name="id">The sprite sheet identifier.</param>
    /// <returns>The corresponding <see cref="SpriteSheet"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sheet is not loaded or was disposed.</exception>
    public SpriteSheet GetSpriteSheet(SpriteSheetId id) =>
        this.spriteSheets[id.Value] ?? throw new InvalidOperationException($"SpriteSheet {id} not loaded or already disposed.");

    /// <summary>
    /// Resolves a named frame within a sprite sheet to a <see cref="SpriteFrameId"/>.
    /// </summary>
    /// <param name="sheet">The sprite sheet identifier.</param>
    /// <param name="name">The frame name as defined in <see cref="LoadSpriteSheet(string, System.Collections.Generic.IReadOnlyDictionary{string, System.Drawing.RectangleF})"/>.</param>
    /// <returns>A frame identifier pointing to the named frame within the sheet.</returns>
    public SpriteFrameId GetFrame(SpriteSheetId sheet, string name)
        => new(sheet, this.GetSpriteSheet(sheet).IndexOf(name));
    
    /// <summary>
    /// Disposes all loaded textures and clears all internal caches and indices.
    /// </summary>
    public void Dispose()
    {
        this.spriteSheets.Clear();
        
        foreach (var texture2D in this.textures) texture2D.Dispose();
        this.textures.Clear();

        this.pathToSheetId.Clear();
        this.pathToTextureId.Clear();
    }
}