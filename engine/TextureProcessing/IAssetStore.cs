namespace engine.TextureProcessing
{
    /// <summary>
    ///     Public interface for managing sprite sheets, sprites, sprite sets and animation clips.
    /// </summary>
    public interface IAssetStore : IDisposable
    {
        /// <summary>
        ///     Loads a sprite sheet from the specified texture path or returns it from cache if already loaded.
        /// </summary>
        /// <param name="texturePath">Absolute or relative file path to the image.</param>
        /// <param name="generateMipmaps">Whether mipmaps should be generated for the GL texture.</param>
        /// <returns>A <see cref="SpriteSheet"/> containing the assigned identifier and pixel size.</returns>
        SpriteSheet LoadSpriteSheet(string texturePath, bool generateMipmaps = false);

        /// <summary>
        ///     Disposes the GL texture associated with the given sprite sheet identifier
        ///     and detaches registered keys referencing that sheet.
        /// </summary>
        /// <param name="id">The sprite sheet identifier.</param>
        void UnloadSpriteSheet(SpriteSheetId id);

        /// <summary>
        ///     Returns the GL texture corresponding to a sprite sheet identifier.
        /// </summary>
        /// <param name="id">The sprite sheet identifier.</param>
        /// <returns>The associated <see cref="Texture2D"/> instance.</returns>
        Texture2D GetTextureById(SpriteSheetId id);

        /// <summary>
        ///     Registers a single static sprite under the specified key.
        /// </summary>
        /// <param name="key">Stable asset reference.</param>
        /// <param name="value">Sprite to store.</param>
        void Register(AssetRef<StaticSprite> key, StaticSprite value);

        /// <summary>
        ///     Registers a sprite set under the specified key. The set must contain at least one sprite.
        /// </summary>
        /// <param name="key">Stable asset reference.</param>
        /// <param name="value">Sprite set to store.</param>
        void Register(AssetRef<SpriteSet> key, SpriteSet value);

        /// <summary>
        ///     Registers an animation clip under the specified key. The clip must contain at least one frame.
        /// </summary>
        /// <param name="key">Stable asset reference.</param>
        /// <param name="value">Animation clip to store.</param>
        void Register(AssetRef<AnimationClip> key, AnimationClip value);

        /// <summary>
        ///     Returns the first frame of an animation clip for initial rendering.
        /// </summary>
        /// <param name="animation">Asset reference to an animation clip.</param>
        /// <returns>The first <see cref="StaticSprite"/> frame.</returns>
        StaticSprite FirstAnimationFrame(AssetRef<AnimationClip> animation);

        /// <summary>
        ///     Retrieves an asset by a typed key.
        ///     Supported types are <see cref="StaticSprite"/>, <see cref="SpriteSet"/>, and <see cref="AnimationClip"/>.
        /// </summary>
        /// <typeparam name="T">The requested asset type.</typeparam>
        /// <param name="key">Stable asset reference.</param>
        /// <returns>The stored asset instance.</returns>
        T Get<T>(AssetRef<T> key);

        /// <summary>
        ///     Attempts to retrieve an asset by a typed key.
        /// </summary>
        /// <typeparam name="T">The requested asset type.</typeparam>
        /// <param name="key">Stable asset reference.</param>
        /// <param name="value">Outputs the retrieved value when found; otherwise the default value.</param>
        /// <returns><c>true</c> if the asset is found; otherwise <c>false</c>.</returns>
        bool TryGet<T>(AssetRef<T> key, out T value);
    }
}