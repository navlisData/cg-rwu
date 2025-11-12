using unnamed.Enums;

namespace unnamed.GameMap.MapGeneration;

public class GraphBasedGenerator : IMapGenerator
{
    private const int ChunkSizeSquared = Map.ChunkSize * Map.ChunkSize;
    private readonly Random rng = Random.Shared;
    private readonly int roomMaxSize = 10;
    private readonly int roomMinSize = 5;

    private readonly float roomsPerChunk = 3;

    public void GenerateMap(in TileFlags[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        int roomCount = (int)(this.roomsPerChunk * width * height / ChunkSizeSquared);

        List<Rect> rooms = new();
        List<(int x, int y)> centers = new();

        for (int i = 0; i < roomCount; i += 1)
        {
            int w = this.rng.Next(this.roomMinSize, this.roomMaxSize + 1);
            int h = this.rng.Next(this.roomMinSize, this.roomMaxSize + 1);
            int x = this.rng.Next(1, width - w - 1);
            int y = this.rng.Next(1, height - h - 1);

            Rect newRoom = new(x, y, w, h);
            bool overlaps = false;
            foreach (Rect r in rooms)
            {
                if (!newRoom.Intersects(r))
                {
                    continue;
                }

                overlaps = true;
            }

            if (overlaps)
            {
                continue;
            }

            rooms.Add(newRoom);
            centers.Add(newRoom.Center);
            CarveRoom(map, newRoom);
        }

        centers.Sort((a, b) => a.x.CompareTo(b.x));
        for (int i = 1; i < centers.Count; i += 1)
        {
            this.CarveCorridor(map, centers[i - 1], centers[i]);
        }
    }

    private static void CarveRoom(in TileFlags[,] map, Rect r)
    {
        for (int x = r.X1; x <= r.X2; x += 1)
        for (int y = r.Y1; y <= r.Y2; y += 1)
        {
            map[x, y] = TileFlags.Walkable;
        }
    }

    private void CarveCorridor(in TileFlags[,] map, (int x, int y) a, (int x, int y) b)
    {
        if (this.rng.Next(0, 2) == 0)
        {
            CarveHorizontal(map, a.x, b.x, a.y);
            CarveVertical(map, a.y, b.y, b.x);
        }
        else
        {
            CarveVertical(map, a.y, b.y, a.x);
            CarveHorizontal(map, a.x, b.x, b.y);
        }
    }

    private static void CarveHorizontal(in TileFlags[,] map, int x1, int x2, int y)
    {
        int min = Math.Min(x1, x2);
        int max = Math.Max(x1, x2);
        for (int x = min; x <= max; x += 1)
        {
            map[x, y] = TileFlags.Walkable | TileFlags.Path;
        }
    }

    private static void CarveVertical(in TileFlags[,] map, int y1, int y2, int x)
    {
        int min = Math.Min(y1, y2);
        int max = Math.Max(y1, y2);
        for (int y = min; y <= max; y += 1)
        {
            map[x, y] = TileFlags.Walkable | TileFlags.Path;
        }
    }

    private class Rect(int x, int y, int w, int h)
    {
        public readonly int X1 = x;
        public readonly int X2 = x + w;
        public readonly int Y1 = y;
        public readonly int Y2 = y + h;

        public (int x, int y) Center => ((this.X1 + this.X2) / 2, (this.Y1 + this.Y2) / 2);

        public bool Intersects(Rect other)
        {
            return this.X1 <= other.X2 && this.X2 >= other.X1 && this.Y1 <= other.Y2 && this.Y2 >= other.Y1;
        }
    }
}