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
}