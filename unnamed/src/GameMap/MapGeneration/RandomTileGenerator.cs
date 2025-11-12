using unnamed.Enums;

namespace unnamed.GameMap.MapGeneration;

public class RandomTileGenerator : IMapGenerator
{
    private readonly Random rng = Random.Shared;

    public void GenerateMap(in TileFlags[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int y = 0; y < width; y += 1)
        for (int x = 0; x < height; x += 1)
        {
            map[x, y] = this.rng.Next(0, 2) switch
            {
                0 => TileFlags.Walkable | TileFlags.Path,
                _ => TileFlags.Walkable
            };
        }
    }
}