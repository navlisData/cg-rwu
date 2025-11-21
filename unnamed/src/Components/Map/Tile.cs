using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.Components.Map;

public class Tile
{
    public TileFlags Flags;
    public ushort layer;
    public StaticSprite? OverlaySprite;
    public StaticSprite Sprite;
}