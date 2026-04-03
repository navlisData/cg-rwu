using System.Drawing;

using OpenTK.Mathematics;

namespace engine.TextureProcessing;

public sealed class StaticSprite
{
    public SpriteSheetId SpriteSheetId { get; init; }
    public RectangleF RectPx { get; init; }
    public Vector2 Pivot { get; init; }
}

public sealed class SpriteSet : List<StaticSprite>
{
    public SpriteSet() { }
    public SpriteSet(IEnumerable<StaticSprite> s) : base(s) { }
}

public sealed class AnimationClip
{
    public IReadOnlyList<StaticSprite> Frames { get; init; } = [];
    public float Fps { get; init; } = 12f;
    public byte Priority { get; init; }
    public bool Loop { get; init; } = true;

    /// <summary>
    /// Calculates the animation duration using the clip's configured <see cref="Fps"/>.
    /// </summary>
    /// <returns>The duration in seconds.</returns>
    public float AnimationDuration() => AnimationDuration(Fps);

    /// <summary>
    /// Calculates the animation duration using a specified frames-per-second value.
    /// </summary>
    /// <param name="fps">Frames per second. Must be greater than 0.</param>
    /// <returns>The duration in seconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fps"/> is not greater than 0.</exception>
    public float AnimationDuration(float fps)
    {
        if (fps <= 0f)
            throw new ArgumentOutOfRangeException(nameof(fps), fps, "FPS must be greater than 0.");

        return Frames.Count / fps;
    }
}

public abstract record SheetKey
{
    public sealed record StaticSpriteKey(AssetRef<StaticSprite> Key) : SheetKey;

    public sealed record SpriteSetKey(AssetRef<SpriteSet> Key) : SheetKey;

    public sealed record AnimationSpriteKey(AssetRef<AnimationClip> Key) : SheetKey;
}

public abstract record VisualType
{
    public sealed record StaticSpriteKey(AssetRef<StaticSprite> Key) : VisualType;

    public sealed record AnimationSpriteKey(AssetRef<AnimationClip> Key) : VisualType;

    public static implicit operator VisualType(AssetRef<StaticSprite> key) =>
        new StaticSpriteKey(key);

    public static implicit operator VisualType(AssetRef<AnimationClip> key) =>
        new AnimationSpriteKey(key);
}