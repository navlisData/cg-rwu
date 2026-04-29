using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Resources;

namespace unnamed.systems;

public class MapLoadingSystem() : EntitySetSystem<Camera2D>(
    new QueryBuilder()
        .With<TileGrid>()
        .Build())
{
    private const int LoadingRadius = 1;

    protected override void Update(ref Camera2D camera2D, EntityHandle e)
    {
        ref GridPosition chunkPosition = ref e.Get<GridPosition>();
        Position camera = camera2D.Position;

        if (chunkPosition.X <= camera.Chunk.X + LoadingRadius && chunkPosition.Y <= camera.Chunk.Y + LoadingRadius &&
            chunkPosition.X >= camera.Chunk.X - LoadingRadius && chunkPosition.Y >= camera.Chunk.Y - LoadingRadius)
        {
            e.Add(new Loaded());
        }
        else
        {
            e.Remove<Loaded>();
        }
    }
}