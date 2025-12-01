using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.GameMap;
using unnamed.Prefabs;

namespace unnamed.systems;

public sealed class MoveSystem(World world, Map map, IAssetStore assetStore) : EntitySetSystem<float>(world,
    world.Query()
        .With<Position>()
        .With<Transform>()
        .With<Velocity>()
        .Without<Sleeping>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Position position = ref handle.Get<Position>();
        ref Velocity velocity = ref handle.Get<Velocity>();
        ref Transform transform = ref handle.Get<Transform>();

        Vector2 halfWidth = new(transform.Size.X / 2, 0);
        Vector2 delta = velocity.Direction * velocity.Speed * dt;

        Position newPosition = position + delta;

        if (CanMoveTo(newPosition))
        {
            position = newPosition;
            return;
        }

        if (this.HandleWallCollision(handle, newPosition, halfWidth, velocity.Direction, transform.Height))
        {
            return;
        }

        Position newPositionAlongX = position + new Vector2(delta.X, 0);
        if (CanMoveTo(newPositionAlongX))
        {
            position = newPositionAlongX;
            return;
        }

        Position newPositionAlongY = position + new Vector2(0, delta.Y);
        if (CanMoveTo(newPositionAlongY))
        {
            position = newPositionAlongY;
        }

        return;

        bool CanMoveTo(Position pos)
        {
            return !map.IsWallAt(pos + halfWidth) &&
                   !map.IsWallAt(pos - halfWidth);
        }
    }

    private bool HandleWallCollision(EntityHandle handle, Position position, Vector2 halfWidth, Vector2 direction,
        float height)
    {
        if (handle.Has<Projectile>())
        {
            PrefabFactory.CreateExplosion(this.world, assetStore, position + (halfWidth * direction),
                height,
                handle.Get<Projectile>().ExplosionAnimation);
            handle.Add(new MarkedToDestroy());
            return true;
        }

        return false;
    }
}