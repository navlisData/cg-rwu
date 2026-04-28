using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Tags;
using unnamed.GameMap;

namespace unnamed.Rendering;

public class MapRenderSystem(int layer)
    : EntitySetSystem<RenderContext.RenderContext>(
        new QueryBuilder()
            .With<TileGrid>()
            .With<Loaded>()
            .Build())
{
    protected override void Update(ref RenderContext.RenderContext ctx, EntityHandle e)
    {
        Vector2i chunkPosition = e.Get<GridPosition>();
        ref Tile[] tiles = ref e.Get<TileGrid>().Tiles;

        for (int y = 0; y < Map.ChunkSize; y++)
        {
            for (int x = 0; x < Map.ChunkSize; x++)
            {
                Tile tile = tiles[x + (y * Map.ChunkSize)];
                if (tile.SpriteLayer != layer || tile.Kind == TileKind.Empty)
                {
                    continue;
                }

                Vector2 size = new(Map.TileSize, Map.TileSize);

                ctx.BeginDraw()
                    .WithPosition(
                        ((chunkPosition.X * Map.ChunkSize) + x) * Map.TileSize,
                        ((chunkPosition.Y * Map.ChunkSize) + y) * Map.TileSize, size, tile.Sprite.Pivot)
                    .WithSprite(in tile.Sprite).WithoutColoration().Draw();

                if (tile.OverlaySprite == null)
                {
                    continue;
                }

                ctx.BeginDraw().WithPosition(
                        ((chunkPosition.X * Map.ChunkSize) + x) * Map.TileSize,
                        ((chunkPosition.Y * Map.ChunkSize) + y) * Map.TileSize, size, tile.Sprite.Pivot)
                    .WithSprite(in tile.OverlaySprite).WithoutColoration().Draw();
            }
        }
    }
}