using engine.TextureProcessing;

namespace unnamed.Texture;

public class AssetStore : IDisposable
{
    /// <summary>
    ///     Maps a sprite sheet image path to its stable <see cref="SpriteSheetId"/>.
    ///     Prevents constructing duplicate sheets for the same PNG path.
    /// </summary>
    private readonly Dictionary<string, SpriteSheetId> pathToSheetId = new();

    /// <summary>
    ///     Maps a sprite sheet id to the registered asset keys that use it.
    ///     Useful for tracking usage and cleanup.
    /// </summary>
    private readonly Dictionary<SpriteSheetId, List<SheetKey>> sheetItemsBySpriteSheet = new();

    /// <summary>
    ///     Storage for loaded textures; the index equals <see cref="SpriteSheetId.Value"/>.
    ///     Entries are disposed in <see cref="Dispose"/>.
    /// </summary>
    private readonly List<Texture2D> textures = new();

    private readonly Dictionary<ulong, StaticSprite> spritesByHash = new();
    private readonly Dictionary<ulong, SpriteSet> spriteSetByHash = new();
    private readonly Dictionary<ulong, AnimationClip> animationSetByHash = new();

    /// <summary>
    ///     Loads (or returns a cached) sprite sheet for the given texture path.
    /// </summary>
    /// <param name="texturePath">Absolute or relative file path to the image.</param>
    /// <param name="generateMipmaps">Whether to generate mipmaps for the GL texture.</param>
    /// <returns>A <see cref="SpriteSheet"/> containing the assigned id and pixel size.</returns>
    public SpriteSheet LoadSpriteSheet(string texturePath, bool generateMipmaps = false)
    {
        if (this.pathToSheetId.TryGetValue(texturePath, out var spriteSheetId))
            return new SpriteSheet { Id = spriteSheetId, Size = GetTextureById(spriteSheetId).Size };

        var texture = new Texture2D(texturePath, generateMipmaps);
        var newSpriteSheetId = new SpriteSheetId(this.textures.Count);

        this.textures.Add(texture);
        this.pathToSheetId[texturePath] = newSpriteSheetId;

        return new SpriteSheet { Id = newSpriteSheetId, Size = texture.Size };
    }

    /// <summary>
    ///     Disposes the GL texture associated with the given sprite sheet id and detaches registered keys for that sheet.
    /// </summary>
    /// <param name="id">The sprite sheet identifier.</param>
    public void UnloadSpriteSheet(SpriteSheetId id)
    {
        // Keep slot position stable to preserve ids; only dispose the underlying texture.
        this.textures[id.Value].Dispose();
        this.sheetItemsBySpriteSheet.Remove(id);
    }

    /// <summary>
    ///     Returns the GL texture for a given sprite sheet id.
    /// </summary>
    /// <param name="id">The sprite sheet identifier.</param>
    /// <returns>The corresponding <see cref="Texture2D"/> instance.</returns>
    public Texture2D GetTextureById(SpriteSheetId id) => textures[id.Value];

    /// <summary>
    ///     Helper function. Returns (and creates if necessary) the list of asset keys associated with a given sheet id.
    /// </summary>
    /// <param name="id">The sprite sheet identifier.</param>
    /// <returns>A mutable list of <see cref="SheetKey"/> items.</returns>
    private List<SheetKey> ListFor(SpriteSheetId id)
    {
        if (!sheetItemsBySpriteSheet.TryGetValue(id, out var list))
            sheetItemsBySpriteSheet[id] = list = new List<SheetKey>();
        return list;
    }

    /// <summary>
    ///     Registers a single static sprite by key.
    /// </summary>
    /// <param name="key">Stable asset reference.</param>
    /// <param name="value">Sprite value to store.</param>
    public void Register(AssetRef<StaticSprite> key, StaticSprite value)
    {
        spritesByHash[key.Id] = value;
        ListFor(value.SpriteSheetId).Add(new SheetKey.StaticSpriteKey(key));
    }

    /// <summary>
    ///     Registers a sprite set by key. The set must contain at least one sprite.
    /// </summary>
    /// <param name="key">Stable asset reference.</param>
    /// <param name="value">Sprite set to store.</param>
    /// <exception cref="ArgumentException">Thrown when the set is null or empty.</exception>
    public void Register(AssetRef<SpriteSet> key, SpriteSet value)
    {
        if (value is null || value.Count == 0)
            throw new ArgumentException("SpriteSet must have at least one sprite", nameof(value));

        spriteSetByHash[key.Id] = value;
        foreach (var sheetId in value.Select(s => s.SpriteSheetId).Distinct())
            ListFor(sheetId).Add(new SheetKey.SpriteSetKey(key));
    }

    /// <summary>
    ///     Registers an animation clip by key. The clip must contain at least one frame.
    /// </summary>
    /// <param name="key">Stable asset reference.</param>
    /// <param name="value">Animation clip to store.</param>
    /// <exception cref="ArgumentException">Thrown when the clip is null or has no frames.</exception>
    public void Register(AssetRef<AnimationClip> key, AnimationClip value)
    {
        if (value is null || value.Frames.Count == 0)
            throw new ArgumentException("AnimationClip must have at least one frame", nameof(value));

        animationSetByHash[key.Id] = value;
        foreach (var sheetId in value.Frames.Select(f => f.SpriteSheetId).Distinct())
            ListFor(sheetId).Add(new SheetKey.AnimationSpriteKey(key));
    }

    /// <summary>
    ///     Returns the first frame of an animation clip for initial rendering.
    /// </summary>
    /// <param name="animation">Asset reference to an animation clip.</param>
    /// <returns>The first <see cref="StaticSprite"/> frame.</returns>
    public StaticSprite FirstAnimationFrame(AssetRef<AnimationClip> animation)
    {
        return Get(animation).Frames[0];
    }

    /// <summary>
    ///     Retrieves an asset by typed key.
    /// </summary>
    /// <typeparam name="T">One of <see cref="StaticSprite"/>, <see cref="SpriteSet"/>, or <see cref="AnimationClip"/>.</typeparam>
    /// <param name="key">Stable asset reference.</param>
    /// <returns>The stored asset instance.</returns>
    /// <exception cref="NotSupportedException">When <typeparamref name="T"/> is not supported.</exception>
    public T Get<T>(AssetRef<T> key) =>
        typeof(T) == typeof(StaticSprite) ? (T)(object)this.spritesByHash[key.Id] :
        typeof(T) == typeof(SpriteSet) ? (T)(object)this.spriteSetByHash[key.Id] :
        typeof(T) == typeof(AnimationClip) ? (T)(object)this.animationSetByHash[key.Id] :
        throw new NotSupportedException(typeof(T).Name);

    /// <summary>
    ///     Tries to retrieve an asset by typed key.
    /// </summary>
    /// <typeparam name="T">One of <see cref="StaticSprite"/>, <see cref="SpriteSet"/>, or <see cref="AnimationClip"/>.</typeparam>
    /// <param name="key">Stable asset reference.</param>
    /// <param name="value">The retrieved value when found; otherwise default.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    public bool TryGet<T>(AssetRef<T> key, out T value)
    {
        if (typeof(T) == typeof(StaticSprite) && spritesByHash.TryGetValue(key.Id, out var staticSprite))
        {
            value = (T)(object)staticSprite;
            return true;
        }

        if (typeof(T) == typeof(SpriteSet) && spriteSetByHash.TryGetValue(key.Id, out var spriteSet))
        {
            value = (T)(object)spriteSet;
            return true;
        }

        if (typeof(T) == typeof(AnimationClip) && animationSetByHash.TryGetValue(key.Id, out var animationClip))
        {
            value = (T)(object)animationClip;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    ///     Disposes all loaded textures and clears all registrations.
    /// </summary>
    public void Dispose()
    {
        foreach (var texture2D in textures) texture2D.Dispose();
        textures.Clear();
        sheetItemsBySpriteSheet.Clear();
        spritesByHash.Clear();
        spriteSetByHash.Clear();
        animationSetByHash.Clear();
    }
}
