using System.Drawing;

using OpenTK.Mathematics;

namespace engine.TextureProcessing;

public sealed class StaticSprite
{
    public SpriteSheetId SpriteSheetId { get; init; }
    public RectangleF RectPx { get; init; }
    public Vector2 PivotPx { get; init; }
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
}

// Discriminated Unions
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