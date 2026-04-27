using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Resources;
using unnamed.Systems.SystemScheduler;

namespace unnamed.systems;

public class MapLoadingSystem(World world) : EntitySetSystem<UpdateContext>(world,
    new QueryBuilder()
        .With<TileGrid>()
        .Build())
{
    private const int LoadingRadius = 1;

    protected override void Update(UpdateContext unused, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);
        ref Position camera = ref this.world.GetResource<Camera2D>().Position;

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