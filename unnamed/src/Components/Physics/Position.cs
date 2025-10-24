using System.Diagnostics.Contracts;

using OpenTK.Mathematics;

using unnamed.Utils;

namespace unnamed.Components.Physics;

public struct Position : IEquatable<Position>
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
    ///     The tile relative position
    /// </summary>
    public Vector2 Pos;

    /// <summary>
    ///     Converts the map-relative <c>Position</c> to the respective world position
    /// </summary>
    /// <returns>World position as <c>Vector2</c></returns>
    [Pure]
    public Vector2 ToWorldPosition()
    {
        return new Vector2(
            (((this.Chunk.X * Constants.GridSizeX) + this.Tile.X) * Constants.TileSizeX) + this.Pos.X,
            (((this.Chunk.Y * Constants.GridSizeY) + this.Tile.Y) * Constants.TileSizeY) + this.Pos.Y);
    }

    [Pure]
    public static Position FromWorldPosition(in Vector2 world)
    {
        Position pos = new();

        const float chunkWorldSizeX = Constants.GridSizeX * Constants.TileSizeX;
        const float chunkWorldSizeY = Constants.GridSizeY * Constants.TileSizeY;

        pos.Chunk = new Vector2i(
            (int)Math.Floor(world.X / chunkWorldSizeX),
            (int)Math.Floor(world.Y / chunkWorldSizeY)
        );

        float localX = world.X - (pos.Chunk.X * chunkWorldSizeX);
        float localY = world.Y - (pos.Chunk.Y * chunkWorldSizeY);

        pos.Tile = new Vector2i(
            (int)Math.Floor(localX / Constants.TileSizeX),
            (int)Math.Floor(localY / Constants.TileSizeY)
        );

        pos.Pos = new Vector2(
            world.X - (((pos.Chunk.X * Constants.GridSizeX) + pos.Tile.X) * Constants.TileSizeX),
            world.Y - (((pos.Chunk.Y * Constants.GridSizeY) + pos.Tile.Y) * Constants.TileSizeY)
        );

        return pos;
    }


    private void ReAlign()
    {
        while (this.Pos.X > Constants.TileSizeX)
        {
            this.Pos.X -= Constants.TileSizeX;
            this.Tile.X += 1;
        }

        while (this.Pos.X < 0f)
        {
            this.Pos.X += Constants.TileSizeX;
            this.Tile.X -= 1;
        }

        while (this.Pos.Y > Constants.TileSizeX)
        {
            this.Pos.Y -= Constants.TileSizeX;
            this.Tile.Y += 1;
        }

        while (this.Pos.Y < 0f)
        {
            this.Pos.Y += Constants.TileSizeX;
            this.Tile.Y -= 1;
        }

        while (this.Tile.X > Constants.GridSizeX)
        {
            this.Tile.X -= Constants.GridSizeX;
            this.Chunk.X += 1;
        }

        while (this.Tile.X < 0f)
        {
            this.Tile.X += Constants.GridSizeX;
            this.Chunk.X -= 1;
        }

        while (this.Tile.Y > Constants.GridSizeX)
        {
            this.Tile.Y -= Constants.GridSizeX;
            this.Chunk.Y += 1;
        }

        while (this.Tile.Y < 0f)
        {
            this.Tile.Y += Constants.GridSizeX;
            this.Chunk.Y -= 1;
        }
    }

    /// <summary>
    ///     Returns a new position that is the linear blend of the 2 given positions.
    /// </summary>
    /// <param name="a">First input positions.</param>
    /// <param name="b">Second input positions.</param>
    /// <param name="blend">The blend factor.</param>
    /// <returns>a when blend=0, b when blend=1, and a linear combination otherwise.</returns>
    [Pure]
    public static Position Lerp(in Position a, in Position b, in float blend)
    {
        Vector2.Lerp(a.ToWorldPosition(), b.ToWorldPosition(), blend, out Vector2 world);
        return FromWorldPosition(world);
    }

    [Pure]
    public static Position operator +(Position left, in Position right)
    {
        left.Chunk += right.Chunk;
        left.Tile += right.Tile;
        left.Pos += right.Pos;
        left.ReAlign();
        return left;
    }

    [Pure]
    public static Position operator +(Position left, in Vector2 right)
    {
        left.Pos += right;
        left.ReAlign();
        return left;
    }

    [Pure]
    public static Position operator -(Position left, in Position right)
    {
        left.Chunk = -right.Chunk;
        left.Tile = -right.Chunk;
        left.Pos = -right.Pos;
        left.ReAlign();
        return left;
    }

    [Pure]
    public static Position operator -(Position left)
    {
        left.Chunk = -left.Chunk;
        left.Tile = -left.Chunk;
        left.Pos = -left.Pos;
        left.ReAlign();
        return left;
    }

    [Pure]
    public static bool operator ==(Position left, Position right)
    {
        return left.Equals(right);
    }

    [Pure]
    public static bool operator !=(Position left, Position right)
    {
        return !(left == right);
    }

    public bool Equals(Position other)
    {
        return this.Chunk.Equals(other.Chunk) && this.Tile.Equals(other.Tile) &&
               this.Pos.Equals(other.Pos);
    }

    public override bool Equals(object? obj)
    {
        return obj is Position other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Chunk, this.Tile, this.Pos);
    }
}