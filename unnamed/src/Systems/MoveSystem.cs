using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.GameMap;
using unnamed.Prefabs;
using unnamed.Resources;

namespace unnamed.systems;

public sealed class MoveSystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<Position>()
        .With<Transform>()
        .With<Velocity>()
        .Without<Sleeping>()
        .Build();

    public override void Run(World world)
    {
        ref DeltaTime dt = ref world.GetResource<DeltaTime>();
        ref Map map = ref world.GetResource<Map>();

        foreach (Entity e in Query.AsEnumerator(world))
        {
            Update(world, ref dt, ref map, world.Handle(e));
        }
    }

    private static void Update(World world, ref DeltaTime dt, ref Map map, EntityHandle e)
    {
        ref Position position = ref e.Get<Position>();
        ref Velocity velocity = ref e.Get<Velocity>();
        ref Transform transform = ref e.Get<Transform>();

        Vector2 halfWidth = new(transform.Size.X / 2, 0);
        Vector2 delta = velocity.Direction * velocity.Speed * dt;

        Position newPosition = position + delta;

        if (CanMoveTo(ref map, newPosition))
        {
            position = newPosition;
            return;
        }

        if (HandleWallCollision(e, newPosition, halfWidth, velocity.Direction, transform.Height, world))
        {
            return;
        }

        Position newPositionAlongX = position + new Vector2(delta.X, 0);
        if (CanMoveTo(ref map, newPositionAlongX))
        {
            position = newPositionAlongX;
            return;
        }

        Position newPositionAlongY = position + new Vector2(0, delta.Y);
        if (CanMoveTo(ref map, newPositionAlongY))
        {
            position = newPositionAlongY;
        }

        return;

        bool CanMoveTo(ref Map map, Position pos)
        {
            return !map.IsWallAt(world, pos + halfWidth) &&
                   !map.IsWallAt(world, pos - halfWidth);
        }
    }

    private static bool HandleWallCollision(EntityHandle handle, Position position, Vector2 halfWidth,
        Vector2 direction,
        float height, World world)
    {
        if (!handle.Has<Projectile>())
        {
            return false;
        }

        PrefabFactory.CreateExplosion(world, position + (halfWidth * direction),
            height,
            handle.Get<Projectile>().ExplosionAnimation);
        handle.Add(new MarkedToDestroy());
        return true;
    }
}