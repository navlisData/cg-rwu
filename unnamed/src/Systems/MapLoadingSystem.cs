using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;

namespace unnamed.systems;

public class MapLoadingSystem(World world) : EntitySetSystem<Position>(world,
    world.Query()
        .With<TileGrid>()
        .Build())
{
    private const int LoadingRadius = 1;

    protected override void Update(Position camera, in Entity e)
    {
        ref GridPosition chunkPosition = ref e.Get<GridPosition>();

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