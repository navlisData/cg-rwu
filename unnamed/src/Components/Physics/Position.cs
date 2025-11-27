using System.Diagnostics.Contracts;

using OpenTK.Mathematics;

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

    public Position(Vector2i chunk, Vector2i tile, Vector2 pos)
    {
        this.Chunk = chunk;
        this.Tile = tile;
        this.Pos = pos;
        this.ReAlign();
    }

    public Position(int chunkX, int chunkY, int tileX, int tileY, float posX, float posY)
    {
        this.Chunk.X = chunkX;
        this.Chunk.Y = chunkY;
        this.Tile.X = tileX;
        this.Tile.Y = tileY;
        this.Pos.X = posX;
        this.Pos.Y = posY;
        this.ReAlign();
    }

    /// <summary>
    ///     Converts the map-relative <c>Position</c> to the respective world position
    /// </summary>
    /// <returns>World position as <c>Vector2</c></returns>
    [Pure]
    public Vector2 ToWorldPosition()
    {
        return new Vector2(
            (((this.Chunk.X * GameMap.Map.ChunkSize) + this.Tile.X) * GameMap.Map.TileSize) + this.Pos.X,
            (((this.Chunk.Y * GameMap.Map.ChunkSize) + this.Tile.Y) * GameMap.Map.TileSize) + this.Pos.Y);
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
        return new Position(Vector2i.Zero, Vector2i.Zero, world);
    }

    public float LengthFast()
    {
        Vector2 global = this.ToWorldPosition();
        return 1.0f / MathHelper.InverseSqrtFast((global.X * global.X) + (global.Y * global.Y));
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
        left.Chunk -= right.Chunk;
        left.Tile -= right.Tile;
        left.Pos -= right.Pos;
        left.ReAlign();
        return left;
    }

    [Pure]
    public static Position operator -(Position left, in Vector2 right)
    {
        left.Pos -= right;
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

    private void ReAlign()
    {
        while (this.Pos.X >= GameMap.Map.TileSize)
        {
            this.Pos.X -= GameMap.Map.TileSize;
            this.Tile.X += 1;
        }

        while (this.Pos.X < 0f)
        {
            this.Pos.X += GameMap.Map.TileSize;
            this.Tile.X -= 1;
        }

        while (this.Pos.Y >= GameMap.Map.TileSize)
        {
            this.Pos.Y -= GameMap.Map.TileSize;
            this.Tile.Y += 1;
        }

        while (this.Pos.Y < 0f)
        {
            this.Pos.Y += GameMap.Map.TileSize;
            this.Tile.Y -= 1;
        }

        while (this.Tile.X >= GameMap.Map.ChunkSize)
        {
            this.Tile.X -= GameMap.Map.ChunkSize;
            this.Chunk.X += 1;
        }

        while (this.Tile.X < 0f)
        {
            this.Tile.X += GameMap.Map.ChunkSize;
            this.Chunk.X -= 1;
        }

        while (this.Tile.Y >= GameMap.Map.ChunkSize)
        {
            this.Tile.Y -= GameMap.Map.ChunkSize;
            this.Chunk.Y += 1;
        }

        while (this.Tile.Y < 0f)
        {
            this.Tile.Y += GameMap.Map.ChunkSize;
            this.Chunk.Y -= 1;
        }
    }
}