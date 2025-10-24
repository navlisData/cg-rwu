using System.Drawing;

namespace unnamed.Utils;

public static class GameSprites
{
    public static Dictionary<string, RectangleF> Get()
    {
        return new Dictionary<string, RectangleF>() {
            ["floor_1"]  = new RectangleF(0, 0, 32, 32),
        };
    }
}