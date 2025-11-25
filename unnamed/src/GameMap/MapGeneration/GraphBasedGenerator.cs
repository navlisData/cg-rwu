using OpenTK.Mathematics;

using unnamed.Enums;

namespace unnamed.GameMap.MapGeneration;

public class GraphBasedGenerator : IMapGenerator
{
    private const int ChunkSizeSquared = Map.ChunkSize * Map.ChunkSize;
    private readonly Random rng = Random.Shared;
    private readonly int roomMaxSize = 12;
    private readonly int roomMinSize = 6;

    private readonly float roomTriesPerChunk = 3;

    public List<Vector2i> GenerateMap(in IntermediateMap map)
    {
        int width = map.Width;
        int height = map.Height;

        int roomTries = (int)(this.roomTriesPerChunk * width * height / ChunkSizeSquared);

        List<Rect> rooms = new();
        List<Vector2i> centers = new();

        for (int i = 0; i < roomTries; i += 1)
        {
            int w = this.rng.Next(this.roomMinSize, this.roomMaxSize + 1);
            int h = this.rng.Next(this.roomMinSize, this.roomMaxSize + 1);
            int x = this.rng.Next(1, width - w);
            int y = this.rng.Next(1, height - h);

            Rect newRoom = new(x, y, w, h);
            bool overlaps = false;

            foreach (Rect r in rooms)
            {
                Rect inflated = r.Inflate(2);

                if (inflated.Intersects(newRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (overlaps)
            {
                continue;
            }

            rooms.Add(newRoom);
            centers.Add(newRoom.Center);
            CarveRoom(map, newRoom);
        }

        centers.Sort((a, b) => a.X.CompareTo(b.X));
        for (int i = 1; i < centers.Count; i += 1)
        {
            this.CarveCorridor(map, centers[i - 1], centers[i]);
        }

        while (EraseIllegalWalls(in map)) { }

        return centers;
    }

    private static void CarveRoom(in IntermediateMap map, Rect r)
    {
        for (int x = r.X1; x <= r.X2; x += 1)
        for (int y = r.Y1; y <= r.Y2; y += 1)
        {
            map[x, y] = TileFlags.Walkable;
        }
    }

    private void CarveCorridor(in IntermediateMap map, Vector2i a, Vector2i b)
    {
        if (this.rng.Next(0, 2) == 0)
        {
            CarveHorizontal(map, a.X, b.X, a.Y);
            CarveVertical(map, a.Y, b.Y, b.X);
        }
        else
        {
            CarveVertical(map, a.Y, b.Y, a.X);
            CarveHorizontal(map, a.X, b.X, b.Y);
        }
    }

    private static void CarveHorizontal(in IntermediateMap map, int x1, int x2, int y)
    {
        int min = Math.Min(x1, x2);
        int max = Math.Max(x1, x2);

        for (int x = min; x <= max; x += 1)
        {
            for (int dy = -1; dy <= 0; dy += 1)
            {
                int ny = y + dy;
                if (ny >= 0 && ny < map.Height)
                {
                    map[x, ny] = TileFlags.Walkable | TileFlags.Path;
                }
            }
        }
    }


    private static void CarveVertical(in IntermediateMap map, int y1, int y2, int x)
    {
        int min = Math.Min(y1, y2);
        int max = Math.Max(y1, y2);

        for (int y = min; y <= max; y += 1)
        {
            for (int dx = -1; dx <= 0; dx += 1)
            {
                int nx = x + dx;
                if (nx >= 0 && nx < map.Width)
                {
                    map[nx, y] = TileFlags.Walkable | TileFlags.Path;
                }
            }
        }
    }

    private static bool EraseIllegalWalls(in IntermediateMap map)
    {
        bool changed = false;

        for (int y = 0; y < map.Height; y += 1)
        for (int x = 0; x < map.Width; x += 1)
        {
            if (map[x, y].IsWalkable()) { continue; }

            if ((!map.IsWallLeftOf(x, y) && !map.IsWallRightOf(x, y)) ||
                (!map.IsWallTopCenterOf(x, y) && !map.IsWallBottomCenterOf(x, y)))
            {
                changed = true;
                map[x, y] = TileFlags.Walkable;
            }
        }

        return changed;
    }

    private class Rect(int x, int y, int w, int h)
    {
        public readonly int X1 = x;
        public readonly int X2 = x + w;
        public readonly int Y1 = y;
        public readonly int Y2 = y + h;

        public Vector2i Center => ((this.X1 + this.X2) / 2, (this.Y1 + this.Y2) / 2);

        public bool Intersects(Rect other)
        {
            return this.X1 <= other.X2 && this.X2 >= other.X1 && this.Y1 <= other.Y2 && this.Y2 >= other.Y1;
        }

        public Rect Inflate(int amount)
        {
            return new Rect(
                this.X1 - amount,
                this.Y1 - amount,
                this.X2 - this.X1 + (amount * 2),
                this.Y2 - this.Y1 + (amount * 2)
            );
        }
    }
}