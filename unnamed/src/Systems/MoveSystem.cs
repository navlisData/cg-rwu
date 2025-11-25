using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;

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
        ref Position position = ref e.Get<Position>();
        ref Velocity velocity = ref e.Get<Velocity>();
        ref Transform transform = ref e.Get<Transform>();

        Position newPosition = position + (velocity.Value * dt);
        Vector2 halfWidth = new(transform.Size.X / 2, 0);

        if (map.IsWallAt(newPosition + halfWidth) ||
            map.IsWallAt(newPosition - halfWidth))
        {
            if (e.Has<Projectile>())
            {
                PrefabFactory.CreateExplosion(this.world, assetStore, position,
                    e.Get<Projectile>().ExplosionAnimation);
                this.world.DestroyEntity(e);
                return;
            }
        }

        position = newPosition;
    }
}