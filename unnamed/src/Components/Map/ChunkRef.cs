using Engine.Ecs;

namespace unnamed.Components.Map;

public struct ChunkRef(Entity chunk)
{
    public Entity Chunk = chunk;
}