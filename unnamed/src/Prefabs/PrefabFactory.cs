using Engine.Ecs;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Texture;

namespace unnamed.Prefabs;

public static class PrefabFactory
{
    public static Entity CreatePlayer(World world, Position startPos, Vector2 startVel, Vector2 size,
        IAssetStore assetStore)
    {
        Entity entity = world.CreateEntity();
        entity.Add(startPos);
        entity.Add(new Velocity());
        entity.Add(new Transform { Size = size, Scale = 1 });
        entity.Add(new ReceivesPlayerInput());
        entity.Add(new Sprite
        {
            Frame = assetStore.FirstAnimationFrame(GameAssets.Player.Run.South),
            Tint = new Vector4(0f, 0f, 0f, 1f),
            Layer = 0
        });
        entity.Add(new AlignedCharacter
        {
            CharacterDirection = CharacterDirection.South, CharacterType = CharacterType.Player
        });

        entity.Add(new Character());
        entity.Add(new Player());
        entity.Add(new HasShadow());
        entity.Add(new PlayerActionState());
        entity.Add(new EntityStats { Hitpoints = 5 });
        return entity;
    }

    public static Entity CreateEnemy(World world, Position startPos, Vector2 size, EntityStats stats, Entity target,
        IAssetStore assetStore)
    {
        Entity entity = world.CreateEntity();
        entity.Add(startPos);
        entity.Add(new Transform { Size = size, Scale = 1 });
        entity.Add(new Sprite
        {
            Frame = assetStore.FirstAnimationFrame(GameAssets.Enemy.Slime1.Idle),
            Tint = new Vector4(0f, 0f, 0f, 1f),
            Layer = 0
        });
        entity.Add(new NonDirectionalCharacter { CharacterType = CharacterType.Enemy });

        entity.Add(new Character());
        entity.Add(new Enemy());
        entity.Add(new Follows { Target = target, Type = FollowType.Linear, FollowRadius = 15, Speed = 2f });
        entity.Add(new HasShadow());
        entity.Add(new EnemyActionState());
        entity.Add(stats);
        return entity;
    }

    public static Entity CreateFollowingCamera(World world, in Entity target, Vector2i viewport, Position startPos)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new Camera2D { Zoom = 1f, OrthographicSize = 20f, Viewport = viewport });
        entity.Add(new Follows { Target = target, Speed = 5f, FollowRadius = float.MaxValue, Type = FollowType.Lerp });
        entity.Add(startPos);
        entity.Add(new ReceivesCameraControl());
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

    public static Entity CreateBullet(World world, Position startPos, Velocity velocity, float rotation, float height,
        IAssetStore assetStore)
    {
        Entity entity = world.CreateEntity();
        entity.Add(startPos);
        entity.Add(new Transform { Size = new Vector2(2f, 2f), Scale = 1.2f, Rotation = rotation, Height = height });
        entity.Add(new AnimatedSprite
        {
            CurrentFrameIndex = 0, AnimationClip = assetStore.Get(GameAssets.Projectile.Fireball), TimeInFrame = 0
        });
        entity.Add(velocity);
        entity.Add(new Projectile
        {
            Damage = 10,
            Lifetime = Lifetime.DestroyOnSleep,
            ExplosionAnimation = GameAssets.Explosion.BulletExplosion,
            ExplosionRadius = 1
        });
        entity.Add(new HasShadow());
        entity.Add(EntityCollisionBehavior.DestroySelf | EntityCollisionBehavior.Explode);
        return entity;
    }

    public static Entity CreateCrossHair(World world, IAssetStore assetStore)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new Position());
        entity.Add(new SetPositionToMouse());
        entity.Add(new Transform { Size = new Vector2(1f, 1f), Scale = 1f });
        entity.Add(new Sprite
        {
            Frame = assetStore.Get(GameAssets.Crosshair.Simple), Tint = new Vector4(0f, 0f, 0f, 1f)
        });
        entity.Add(new Ui());
        return entity;
    }

    public static Entity CreateExplosion(World world, IAssetStore assetStore, Position position, float height,
        AssetRef<AnimationClip> animationClip)
    {
        Entity entity = world.CreateEntity();
        entity.Add(position);
        entity.Add(new Transform { Size = new Vector2(1f, 1f), Scale = 2.5f, Height = height });
        entity.Add(new Projectile());
        entity.Add(new MarkedToDestroy { RemainingLifetime = assetStore.Get(animationClip).AnimationDuration() });
        entity.Add(new AnimatedSprite
        {
            CurrentFrameIndex = 0, AnimationClip = assetStore.Get(animationClip), TimeInFrame = 0
        });
        return entity;
    }
}