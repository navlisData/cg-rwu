using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Enums;

namespace unnamed.GameMap.MapGeneration;

public class RandomTileGenerator(List<StaticSprite> possibleMapSprites) : IMapGenerator
{
    private readonly Random rnd = Random.Shared;

    public Tile GenerateTile(Vector2i unused)
    {
        StaticSprite sprite = possibleMapSprites.ElementAt(this.rnd.Next(possibleMapSprites.Count - 1));

        return new Tile { Flags = TileFlags.Walkable, Sprite = sprite };
    }
}