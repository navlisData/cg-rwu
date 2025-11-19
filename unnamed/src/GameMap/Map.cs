using System.Diagnostics;

using Engine.Ecs;

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

    private readonly World world;
    public IMapGenerator MapGenerator;
    public SpriteMapper? SpriteMapper;

    public Map(World world, IMapGenerator? mapGenerator = null)
    {
        this.world = world;
        this.MapGenerator = mapGenerator ?? new RandomTileGenerator();
    }

    /// <summary>
    ///     Returns the chunk entity for a position in chunk-space, creating one if necessary.
    /// </summary>
    public Entity GetOrCreateChunk(Vector2i chunkPos)
    {
        if (!this.chunks.TryGetValue(chunkPos, out Entity chunk))
        {
            chunk = this.world.CreateEntity();
            chunk.Add(new GridPosition(chunkPos));
            chunk.Add(new TileGrid { Tiles = new Tile[ChunkSize * ChunkSize] });
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

        ref TileGrid grid = ref chunk.Get<TileGrid>();
        int index = pos.Tile.X + (pos.Tile.Y * ChunkSize);
        return grid.Tiles[index];
    }

    /// <summary>
    ///     Sets a tile type at a world-space tile coordinate, creating the chunk if needed.
    /// </summary>
    public void SetTile(Position position, Tile tile)
    {
        Entity chunk = this.GetOrCreateChunk(position.Chunk);
        ref TileGrid grid = ref chunk.Get<TileGrid>();

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

        TileFlags[,] map = new TileFlags[widthTiles, heightTiles];

        this.MapGenerator.GenerateMap(map);
        this.SpriteMapper.Map = map;

        for (int cy = minChunk.Y, my = 0; cy <= maxChunk.Y; cy += 1, my += 1)
        for (int cx = minChunk.X, mx = 0; cx <= maxChunk.X; cx += 1, mx += 1)
        {
            Vector2i chunkPos = new(cx, cy);
            Entity chunk = this.GetOrCreateChunk(chunkPos);
            ref TileGrid grid = ref chunk.Get<TileGrid>();

            for (int ty = 0; ty < ChunkSize; ty += 1)
            for (int tx = 0; tx < ChunkSize; tx += 1)
            {
                int x = (mx * ChunkSize) + tx;
                int y = (my * ChunkSize) + ty;
                TileFlags flags = map[x, y];

                grid.Tiles[tx + (ty * ChunkSize)] = new Tile
                {
                    Flags = flags, Sprite = this.SpriteMapper.MapToSprite(x, y)
                };
            }
        }
    }
}