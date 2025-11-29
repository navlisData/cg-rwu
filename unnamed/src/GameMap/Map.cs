using System.Diagnostics;

using Engine.Ecs;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Enums;
using unnamed.GameMap.MapGeneration;

namespace unnamed.GameMap;

/// <summary>
///     Central manager for spatial world data. Owns chunk entities, handles tile lookup,
///     and bridges static tile data with ECS-driven entities.
/// </summary>
public sealed class Map
{
    /// <summary>
    ///     Amount of vertical/horizontal cells in a map chunk
    /// </summary>
    public const int ChunkSize = 16;

    /// <summary>
    ///     GameMap tile width/height
    /// </summary>
    public const float TileSize = 4;

    private readonly Dictionary<Vector2i, Entity> chunks = new();
    private readonly List<Position> validPositions;

    private readonly World world;
    public IMapGenerator MapGenerator;
    public SpriteMapper? SpriteMapper;
    private int validPositionsIndex;

    public Map(World world, IMapGenerator? mapGenerator = null)
    {
        this.world = world;
        this.MapGenerator = mapGenerator ?? new RandomTileGenerator();
        this.validPositions = new List<Position>();
    }

    /// <summary>
    ///     Returns the chunk entity for a position in chunk-space, creating one if necessary.
    /// </summary>
    public Entity GetOrCreateChunk(Vector2i chunkPos)
    {
        if (!this.chunks.TryGetValue(chunkPos, out Entity chunk))
        {
            chunk = this.world.CreateEntity();
            this.world.Add(chunk, new GridPosition(chunkPos));
            this.world.Add(chunk, new GridPosition(chunkPos));
            this.world.Add(chunk, new TileGrid { Tiles = new Tile[ChunkSize * ChunkSize] });
            this.chunks.Add(chunkPos, chunk);
        }

        return chunk;
    }

    /// <summary>
    ///     Returns the chunk entity for the given chunk position if it exists.
    /// </summary>
    public Entity? GetChunk(Vector2i chunkPos)
    {
        if (this.chunks.TryGetValue(chunkPos, out Entity chunk))
        {
            return chunk;
        }

        return null;
    }

    /// <summary>
    ///     Destroys a chunk and its ECS entity, freeing its resources.
    /// </summary>
    public void RemoveChunk(Vector2i chunkPos)
    {
        if (this.chunks.Remove(chunkPos, out Entity chunk))
        {
            this.world.DestroyEntity(chunk);
        }
    }

    /// <summary>
    ///     Retrieves a tile using an entity's Position component.
    /// </summary>
    public Tile? GetTileAt(in Position pos)
    {
        if (!this.chunks.TryGetValue(pos.Chunk, out Entity chunk))
        {
            return null;
        }

        ref TileGrid grid = ref this.world.Get<TileGrid>(chunk);
        int index = pos.Tile.X + (pos.Tile.Y * ChunkSize);
        return grid.Tiles[index];
    }

    /// <summary>
    ///     Returns <c>true</c> if the tile at the position is a wall else <c>false</c>.
    ///     <remarks>
    ///         Returns <c>true</c> for tiles outside the current map
    ///     </remarks>
    /// </summary>
    public bool IsWallAt(in Position pos)
    {
        Tile? tile = this.GetTileAt(in pos);

        if (tile == null) { return true; }

        return tile.Flags.IsWall();
    }

    /// <summary>
    ///     Sets a tile type at a world-space tile coordinate, creating the chunk if needed.
    /// </summary>
    public void SetTile(Position position, Tile tile)
    {
        Entity chunk = this.GetOrCreateChunk(position.Chunk);
        ref TileGrid grid = ref this.world.Get<TileGrid>(chunk);

        Vector2i tilePos = position.Tile;

        grid.Tiles[tilePos.X + (tilePos.Y * ChunkSize)] = tile;
    }

    /// <summary>
    ///     Generates a rectangular region of chunks using the current <see cref="MapGenerator" />.
    /// </summary>
    public void GenerateMap(Vector2i minChunk, Vector2i maxChunk)
    {
        Debug.Assert(this.SpriteMapper != null);
        Debug.Assert(minChunk.X <= maxChunk.X && minChunk.Y <= maxChunk.Y);

        int widthTiles = (Math.Abs(minChunk.X - maxChunk.X) + 1) * ChunkSize;
        int heightTiles = (Math.Abs(minChunk.Y - maxChunk.Y) + 1) * ChunkSize;
        Position bottomLeftCorner = new(minChunk, Vector2i.Zero, Vector2i.Zero);

        TileFlags[,] map = new TileFlags[widthTiles, heightTiles];

        this.validPositions.Clear();
        List<Vector2i> validPositions = this.MapGenerator.GenerateMap(map);
        this.validPositions.AddRange(validPositions.Select(p =>
            bottomLeftCorner + new Position(Vector2i.Zero, p, Vector2i.Zero)));

        for (int cy = minChunk.Y, my = 0; cy <= maxChunk.Y; cy += 1, my += 1)
        for (int cx = minChunk.X, mx = 0; cx <= maxChunk.X; cx += 1, mx += 1)
        {
            Vector2i chunkPos = new(cx, cy);
            Entity chunk = this.GetOrCreateChunk(chunkPos);
            ref TileGrid grid = ref this.world.Get<TileGrid>(chunk);

            for (int ty = 0; ty < ChunkSize; ty += 1)
            for (int tx = 0; tx < ChunkSize; tx += 1)
            {
                int x = (mx * ChunkSize) + tx;
                int y = (my * ChunkSize) + ty;
                TileFlags flags = map[x, y];

                (StaticSprite sprite, StaticSprite? overlay, ushort layer) = this.SpriteMapper.MapToSprite(x, y, map);

                grid.Tiles[tx + (ty * ChunkSize)] = new Tile
                {
                    Flags = flags, Sprite = sprite, OverlaySprite = overlay, layer = layer
                };
            }
        }
    }

    /// <summary>
    ///     Retrieves the next available valid position, if one exists.
    /// </summary>
    /// <param name="validPosition">
    ///     When the method returns, contains the next valid position if available; otherwise the default value.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a valid position was retrieved; otherwise <c>false</c>.
    /// </returns>
    public bool NextValidPosition(out Position validPosition)
    {
        try
        {
            validPosition = this.validPositions[this.validPositionsIndex];
            this.validPositionsIndex += 1;
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            validPosition = default;
            return false;
        }
    }
}