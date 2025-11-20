namespace unnamed.Enums;

[Flags]
public enum TileFlags
{
    None = 0,
    Walkable = 1 << 0,
    Path = 1 << 1
}

public static class TileFlagsExtension
{
    public static bool IsWalkable(this TileFlags tile)
    {
        return (tile & TileFlags.Walkable) == TileFlags.Walkable;
    }

    public static bool IsPath(this TileFlags tile)
    {
        return (tile & TileFlags.Path) == TileFlags.Path;
    }

    public static bool IsWall(this TileFlags tile)
    {
        return !IsWalkable(tile);
    }
}