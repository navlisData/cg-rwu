using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Tags;
using unnamed.GameMap;

namespace unnamed.Rendering;

public class MapRenderSystem(World world)
    : EntitySetSystem<(RenderContext.RenderContext ctx, int layer)>(world,
        new QueryBuilder()
            .With<TileGrid>()
            .With<Loaded>()
            .Build())
{
    protected override void Update((RenderContext.RenderContext ctx, int layer) context, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        (RenderContext.RenderContext ctx, int layer) = context;

        Vector2i chunkPosition = handle.Get<GridPosition>();
        ref Tile[] tiles = ref handle.Get<TileGrid>().Tiles;

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