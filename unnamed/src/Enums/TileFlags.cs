namespace unnamed.Enums;

[Flags]
public enum TileFlags
{
    None = 0,
    Walkable = 1 << 0,
    Path = 1 << 1
}