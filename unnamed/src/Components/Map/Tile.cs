using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.Components.Map;

public enum TileKind
{
    Empty = 0,
    Filled = 1
}

public readonly struct Tile
{
    public readonly TileFlags Flags;
    public readonly TileKind Kind;
    public readonly StaticSprite Sprite;
    public readonly StaticSprite? OverlaySprite;

    private Tile(TileKind kind)
    {
        this.Kind = kind;
        this.Flags = default;
        this.Sprite = default!;
        this.OverlaySprite = null;
    }

    private Tile(
        TileFlags flags,
        StaticSprite sprite,
        StaticSprite? overlaySprite)
    {
        this.Kind = TileKind.Filled;
        this.Flags = flags;
        this.Sprite = sprite;
        this.OverlaySprite = overlaySprite;
    }

    public static Tile Empty => new(TileKind.Empty);

    public static Tile Filled(
        TileFlags flags,
        StaticSprite sprite,
        StaticSprite? overlaySprite = null)
    {
        return new Tile(flags, sprite, overlaySprite);
    }
}