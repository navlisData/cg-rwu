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
    public readonly ushort SpriteLayer;

    private Tile(TileKind kind)
    {
        this.Kind = kind;
        this.Flags = default;
        this.Sprite = default!;
        this.OverlaySprite = null;
        this.SpriteLayer = 0;
    }

    private Tile(
        TileFlags flags,
        StaticSprite sprite,
        StaticSprite? overlaySprite,
        ushort spriteLayer)
    {
        this.Kind = TileKind.Filled;
        this.Flags = flags;
        this.Sprite = sprite;
        this.OverlaySprite = overlaySprite;
        this.SpriteLayer = spriteLayer;
    }

    public static Tile Empty => new(TileKind.Empty);

    public static Tile Filled(
        TileFlags flags,
        ushort spriteLayer,
        StaticSprite sprite,
        StaticSprite? overlaySprite = null)
    {
        return new Tile(flags, sprite, overlaySprite, spriteLayer);
    }
}