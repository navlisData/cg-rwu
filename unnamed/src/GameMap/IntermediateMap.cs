using unnamed.Enums;

namespace unnamed.GameMap;

/// <summary>
///     Represents a 2D tile map for intermediate processing.
///     <remarks>
///         Relative positions from a center tile (C) in a 3x3 section:
///         <code>
///             [-1,-1] [ 1,0] [-1,1]
///             [ 0,-1] [ 0,0] [ 0,1]
///             [ 1,-1] [-1,0] [ 1,1]
///         </code>
///         Here, [0,0] is the center tile.
///     </remarks>
/// </summary>
public readonly struct IntermediateMap(TileFlags[,] map)
{
    private readonly TileFlags[,] map = map;

    public int Width => this.map.GetLength(0);
    public int Height => this.map.GetLength(1);

    public TileFlags this[int x, int y]
    {
        get => this.map[x, y];
        set => this.map[x, y] = value;
    }

    public static implicit operator TileFlags[,](IntermediateMap self)
    {
        return self.map;
    }

    public static implicit operator IntermediateMap(TileFlags[,] map)
    {
        return new IntermediateMap(map);
    }

    private bool IsWallAtPosition(int x, int y)
    {
        try
        {
            return this.map[x, y].IsWall();
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    ///     Determines if the top-left neighbor of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallTopLeftOf(int x, int y)
    {
        return this.IsWallAtPosition(x - 1, y + 1);
    }

    /// <summary>
    ///     Determines if the top-center neighbor of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallTopCenterOf(int x, int y)
    {
        return this.IsWallAtPosition(x, y + 1);
    }

    /// <summary>
    ///     Determines if the top-right neighbor of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallTopRightOf(int x, int y)
    {
        return this.IsWallAtPosition(x + 1, y + 1);
    }

    /// <summary>
    ///     Determines if the left neighbor of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallLeftOf(int x, int y)
    {
        return this.IsWallAtPosition(x - 1, y);
    }

    /// <summary>
    ///     Determines if the right neighbor of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallRightOf(int x, int y)
    {
        return this.IsWallAtPosition(x + 1, y);
    }

    /// <summary>
    ///     Determines if the bottom-left neighbor of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallBottomLeftOf(int x, int y)
    {
        return this.IsWallAtPosition(x - 1, y - 1);
    }

    /// <summary>
    ///     Determines if the bottom-center neighbor of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallBottomCenterOf(int x, int y)
    {
        return this.IsWallAtPosition(x, y - 1);
    }

    /// <summary>
    ///     Determines if the bottom-right neighbor of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallBottomRightOf(int x, int y)
    {
        return this.IsWallAtPosition(x + 1, y - 1);
    }

    /// <summary>
    ///     Determines if the tile two rows below and one column left of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallBottomBottomLeftOf(int x, int y)
    {
        return this.IsWallAtPosition(x - 1, y - 2);
    }

    /// <summary>
    ///     Determines if the tile two rows below and in the same column as (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallBottomBottomCenterOf(int x, int y)
    {
        return this.IsWallAtPosition(x, y - 2);
    }

    /// <summary>
    ///     Determines if the tile two rows below and one column right of (x, y) is a wall.
    ///     <remarks>
    ///         Outside Tiles are treated as walls.
    ///     </remarks>
    /// </summary>
    public bool IsWallBottomBottomRightOf(int x, int y)
    {
        return this.IsWallAtPosition(x + 1, y - 2);
    }
}