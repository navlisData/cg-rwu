using Engine.Ecs;

using unnamed.Components.Physics;

namespace unnamed.Utils;

public static class EntityOrder
{
    public static int ByPositionY(Entity a, Entity b, World world)
    {
        float aPos = world.Get<Position>(a).ToWorldPosition().Y;
        float bPos = world.Get<Position>(b).ToWorldPosition().Y;

        return bPos.CompareTo(aPos);
    }
}