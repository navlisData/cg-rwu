using OpenTK.Mathematics;

using unnamed.Enums;

namespace unnamed.GameMap.MapGeneration;

public class RandomTileGenerator : IMapGenerator
{
    private readonly Random rng = Random.Shared;

    public List<Vector2i> GenerateMap(in IntermediateMap map)
    {
        int width = map.Width;
        int height = map.Height;
        List<Vector2i> validPositions = new(1);

        for (int y = 0; y < width; y += 1)
        for (int x = 0; x < height; x += 1)
        {
            map[x, y] = this.rng.Next(0, 2) switch
            {
                0 => TileFlags.Walkable | TileFlags.Path,
                _ => TileFlags.Walkable
            };
        }

        validPositions.Add(new Vector2i(width / 2, height / 2));
        return validPositions;
    }
}