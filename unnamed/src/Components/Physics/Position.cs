using System.Diagnostics.Contracts;

using OpenTK.Mathematics;

using unnamed.Utils;

namespace unnamed.Components.Physics;

public struct Position
{
    /// <summary>
    ///     The map chunk the entity is in
    /// </summary>
    public Vector2i Chunk;

    /// <summary>
    ///     The chunk tile the entity is in
    /// </summary>
    public Vector2i Tile;

    /// <summary>
    ///     The cell relative position
    /// </summary>
    public Vector2 CellPosition;

    /// <summary>
    ///     Converts the map-relative <c>Position</c> to the respective world position
    /// </summary>
    /// <returns>World position as <c>Vector2</c></returns>
    public Vector2 ToWorldPosition()
    {
        return new Vector2(
            (((this.Chunk.X * Constants.GridSizeX) + this.Tile.X) * Constants.TileSizeX) + this.CellPosition.X,
            (((this.Chunk.Y * Constants.GridSizeY) + this.Tile.Y) * Constants.TileSizeY) + this.CellPosition.Y);
    }

    internal void Add(Vector2 cellPosition, Vector2i cell, Vector2i chunk)
    {
        this.CellPosition += cellPosition;

        while (this.CellPosition.X > Constants.TileSizeX)
        {
            this.CellPosition.X -= Constants.TileSizeX;
            cell.X += 1;
        }

        while (this.CellPosition.X < 0f)
        {
            this.CellPosition.X += Constants.TileSizeX;
            cell.X -= 1;
        }

        while (this.CellPosition.Y > Constants.TileSizeX)
        {
            this.CellPosition.Y -= Constants.TileSizeX;
            cell.Y += 1;
        }

        while (this.CellPosition.Y < 0f)
        {
            this.CellPosition.Y += Constants.TileSizeX;
            cell.Y -= 1;
        }

        this.Tile += cell;

        while (this.Tile.X > Constants.GridSizeX)
        {
            this.Tile.X -= Constants.GridSizeX;
            chunk.X += 1;
        }

        while (this.Tile.X < 0f)
        {
            this.Tile.X += Constants.GridSizeX;
            chunk.X -= 1;
        }

        while (this.Tile.Y > Constants.GridSizeX)
        {
            this.Tile.Y -= Constants.GridSizeX;
            chunk.Y += 1;
        }

        while (this.Tile.Y < 0f)
        {
            this.Tile.Y += Constants.GridSizeX;
            chunk.Y -= 1;
        }

        this.Chunk += chunk;
    }

    internal void Add(Vector2 cellPosition, Vector2i cell)
    {
        this.Add(cellPosition, cell, Vector2i.Zero);
    }

    internal void Add(Vector2 cellPosition)
    {
        this.Add(cellPosition, Vector2i.Zero, Vector2i.Zero);
    }

    internal void Add(Position position)
    {
        this.Add(position.CellPosition, position.Tile, position.Chunk);
    }

    /// <summary>
    ///     Returns a new position that is the linear blend of the 2 given positions.
    /// </summary>
    /// <param name="a">First input positions.</param>
    /// <param name="b">Second input positions.</param>
    /// <param name="blend">The blend factor.</param>
    /// <returns>a when blend=0, b when blend=1, and a linear combination otherwise.</returns>
    [Pure]
    public static Position Lerp(Position a, Position b, float blend)
    {
        a.CellPosition.X = (blend * (a.CellPosition.X - a.CellPosition.X)) + a.CellPosition.X;
        a.CellPosition.Y = (blend * (a.CellPosition.Y - a.CellPosition.Y)) + a.CellPosition.Y;
        a.Tile.X = (int)(blend * (a.Tile.X - a.Tile.X)) + a.Tile.X;
        a.Tile.Y = (int)(blend * (a.Tile.Y - a.Tile.Y)) + a.Tile.Y;
        a.Chunk.X = (int)(blend * (a.Chunk.X - a.Chunk.X)) + a.Chunk.X;
        a.Chunk.Y = (int)(blend * (a.Chunk.Y - a.Chunk.Y)) + a.Chunk.Y;
        return a;
    }
}