using Engine.Ecs;

using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Prefabs;

public static class PrefabFactory
{
    public static Entity CreatePlayer(World world, Vector2 startPos, Vector2 startVel, Vector2 size)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new Position { Value = startPos });
        entity.Add(new Velocity { Value = startVel });
        entity.Add(new Transform { Size = size, Scale = 1 });
        entity.Add(new Player());

        entity.Add(new Circle());
        entity.Add(new ObjectColor { Rgba = new Vector4(0.3f, 0.5f, 0.8f, 1f) });

        return entity;
    }

    public static Entity CreateFollowingCamera(World world, in Entity target, int width, int height)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new Camera2D { Zoom = 1f, OrthographicSize = 20f, AspectRatio = width / (float)height });
        entity.Add(new Follows { Target = target, LerpSpeed = 25f });
        entity.Add(new Position { Value = (0f, 0f) });
        entity.Add(new Hidden());
        return entity;
    }

    public static Entity CreateEllipsis(World world, Vector2 startPos, Vector2 size, Vector4 color)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new Position { Value = startPos });
        entity.Add(new Transform { Size = size, Scale = 1 });
        entity.Add(new ObjectColor { Rgba = color });
        entity.Add(new Circle());
        return entity;
    }
}