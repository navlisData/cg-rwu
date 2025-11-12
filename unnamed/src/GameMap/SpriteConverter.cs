using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.GameMap;

public class SpriteConverter(
    List<StaticSprite> wallSprites,
    List<StaticSprite> fieldSprites,
    List<StaticSprite> pathSprites)
{
    private readonly Random rng = Random.Shared;

    internal StaticSprite ConvertTileToSprite(TileFlags flags)
    {
        List<StaticSprite> spriteList = wallSprites;

        if ((flags & TileFlags.Walkable) == TileFlags.Walkable)
        {
            spriteList = fieldSprites;
        }

        if ((flags & TileFlags.Path) == TileFlags.Path)
        {
            spriteList = pathSprites;
        }

        return spriteList.ElementAt(this.rng.Next(spriteList.Count - 1));
    }
}