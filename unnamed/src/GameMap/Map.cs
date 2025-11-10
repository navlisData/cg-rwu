using System.Diagnostics;

using Engine.Ecs;

using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Physics;
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

    public IMapGenerator? MapGenerator = null;

    public Map(World world)
    {
        this.world = world;
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
    public void SetTile(Vector2i worldTile, Tile tile)
    {
        Vector2i chunkPos = new(worldTile.X >> 4, worldTile.Y >> 4);
        Vector2i local = new(worldTile.X & 15, worldTile.Y & 15);

        Entity chunk = this.GetOrCreateChunk(chunkPos);
        ref TileGrid grid = ref chunk.Get<TileGrid>();

        grid.Tiles[local.X + (local.Y * ChunkSize)] = tile;
    }

    /// <summary>
    ///     Generates a rectangular region of chunks using the current <see cref="IMapGenerator" />.
    /// </summary>
    public void GenerateArea(Vector2i minChunk, Vector2i maxChunk)
    {
        Debug.Assert(this.MapGenerator != null, "MapGenerator not set");
        for (int cy = minChunk.Y; cy <= maxChunk.Y; cy++)
        {
            for (int cx = minChunk.X; cx <= maxChunk.X; cx++)
            {
                Vector2i chunkPos = new(cx, cy);
                Entity chunk = this.GetOrCreateChunk(chunkPos);
                ref TileGrid grid = ref chunk.Get<TileGrid>();

                for (int ty = 0; ty < ChunkSize; ty += 1)
                {
                    for (int tx = 0; tx < ChunkSize; tx += 1)
                    {
                        Vector2i worldTile = new((cx * ChunkSize) + tx, (cy * ChunkSize) + ty);
                        grid.Tiles[tx + (ty * ChunkSize)] = this.MapGenerator.GenerateTile(worldTile);
                    }
                }
            }
        }
    }
}