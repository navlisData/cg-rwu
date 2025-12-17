using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.systems;

public class MapLoadingSystem(World world) : EntitySetSystem<Position>(world,
    new QueryBuilder()
        .With<TileGrid>()
        .Build())
{
    private const int LoadingRadius = 1;

    protected override void Update(Position camera, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref GridPosition chunkPosition = ref handle.Get<GridPosition>();

        if (chunkPosition.X <= camera.Chunk.X + LoadingRadius && chunkPosition.Y <= camera.Chunk.Y + LoadingRadius &&
            chunkPosition.X >= camera.Chunk.X - LoadingRadius && chunkPosition.Y >= camera.Chunk.Y - LoadingRadius)
        {
            handle.Add(new Loaded());
        }
        else
        {
            handle.Remove<Loaded>();
        }
    }
}