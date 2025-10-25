using System.Drawing;

namespace unnamed.Utils;

public static class GameSprites
{
    public static Dictionary<string, RectangleF> GetPathwaySprites()
    {
        var dict = new Dictionary<string, RectangleF>();
        for (int row = 4; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                dict.Add($"floor_pathway_{row}_{column}", new RectangleF(row*32, column*32, 32, 32));
            }
        }
        return dict;
    }
    
    public static Dictionary<string, RectangleF> GetGrassSprites()
    {
        var dict = new Dictionary<string, RectangleF>();
        for (int row = 0; row < 4; row++)
        {
            for (int column = 0; column < 4; column++)
            {
                dict.Add($"floor_grass_{row}_{column}", new RectangleF(row*32, column*32, 32, 32));
            }
        }
        return dict;
    }
    
    public static Dictionary<string, RectangleF> GetFlowerSprites()
    {
        var dict = new Dictionary<string, RectangleF>();
        for (int row = 0; row < 8; row++)
        {
            for (int column = 4; column < 8; column++)
            {
                dict.Add($"floor_flowers_{row}_{column}", new RectangleF(row*32, column*32, 32, 32));
            }
        }
        return dict;
    }
    
    public static Dictionary<string, RectangleF> GetAllFloorSprites()
    {
        return new[] { GetPathwaySprites(), GetGrassSprites(), GetFlowerSprites() }
            .SelectMany(d => d)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}