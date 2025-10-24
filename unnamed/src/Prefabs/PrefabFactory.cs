using Engine.Ecs;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Prefabs;

public static class PrefabFactory
{
    // public static Entity CreateSpriteTest(World world, SpriteFrameId spriteFrame, Position startPos)
    // {
    //     Entity entity = world.CreateEntity();
    //     entity.Add(new Sprite { Frame = spriteFrame, Tint = new (1,1,1,1), Layer = 0 });
    //     entity.Add(startPos);
    //     return entity;
    // }
    
    public static Entity CreatePlayer(World world, Position startPos, Vector2 startVel, Vector2 size)
    {
        Entity entity = world.CreateEntity();
        entity.Add(startPos);
        entity.Add(new Velocity { Value = startVel });
        entity.Add(new Transform { Size = size, Scale = 1 });
        entity.Add(new ReceivesPlayerInput());
        entity.Add(new Player());

        entity.Add(new Circle());
        entity.Add(new ObjectColor { Rgba = new Vector4(0.3f, 0.5f, 0.8f, 1f) });

        return entity;
    }

    public static Entity CreateFollowingCamera(World world, in Entity target, Vector2i viewport)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new Camera2D { Zoom = 1f, OrthographicSize = 20f, Viewport = viewport });
        entity.Add(new Follows { Target = target, LerpSpeed = 10f });
        entity.Add(new Position());
        entity.Add(new ReceivesPlayerInput());
        entity.Add(new Hidden());
        return entity;
    }

    public static Entity CreateEllipsis(World world, Position startPos, Vector2 size, Vector4 color)
    {
        Entity entity = world.CreateEntity();
        entity.Add(startPos);
        entity.Add(new Transform { Size = size, Scale = 1 });
        entity.Add(new ObjectColor { Rgba = color });
        entity.Add(new Circle());
        return entity;
    }

    public static Entity CreateBullet(World world, Position startPos, Vector2 velocity)
    {
        Entity entity = world.CreateEntity();
        entity.Add(startPos);
        entity.Add(new Transform { Size = new Vector2(0.1f, 0.1f), Scale = 1 });
        entity.Add(new ObjectColor { Rgba = (1, 0, 0, 1) });
        entity.Add(new Velocity { Value = velocity });
        entity.Add(new Circle());
        return entity;
    }

    public static Entity CreateMapChunk(World world, Vector2i gridPos)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new GridPosition(gridPos));
        entity.Add(new Loaded());
        return entity;
    }

    public static Entity CreateMapTile(World world, TileType type, SpriteFrameId frameId, Entity chunk, Vector2i position)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new ChunkRef(chunk));
        entity.Add(new GridPosition(position));
        entity.Add(type);
        entity.Add(new Sprite { Frame = frameId, Tint = new (1,1,1,1), Layer = 0 });
        return entity;
    }
}