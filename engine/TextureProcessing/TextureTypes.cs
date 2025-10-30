using System.Drawing;

using OpenTK.Mathematics;

namespace engine.TextureProcessing;

public sealed class StaticSprite
{
    public SpriteSheetId SpriteSheetId { get; init; }
    public RectangleF RectPx { get; init; }     // x,y,w,h in Pixeln
    public Vector2 PivotPx { get; init; }     // optional
}

public sealed class SpriteSet : List<StaticSprite> { public SpriteSet() {} public SpriteSet(IEnumerable<StaticSprite> s) : base(s) {} }

public sealed class AnimationClip
{
    public IReadOnlyList<StaticSprite> Frames { get; init; } = [];
    public float Fps { get; init; } = 12f;
    public bool Loop { get; init; } = true;
}

// Discriminated Union für "ein Element einer Sheet-Liste"
public abstract record SheetKey
{
    public sealed record StaticSpriteKey(AssetRef<StaticSprite> Key) : SheetKey;
    public sealed record SpriteSetKey(AssetRef<SpriteSet> Key) : SheetKey;
    public sealed record AnimationSpriteKey(AssetRef<AnimationClip> Key) : SheetKey;
}