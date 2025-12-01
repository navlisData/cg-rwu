using Engine.Ecs;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.General;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;
using unnamed.Enums;
using unnamed.Texture;

namespace unnamed.Prefabs;

public static class PrefabFactory
{
    public static Entity CreatePlayer(World world, Position startPos, Vector2 startVel, Vector2 size,
        IAssetStore assetStore)
    {
        return world.Create()
            .Add(startPos)
            .Add(new Velocity())
            .Add(new Transform { Size = size, Scale = 1 })
            .Add(new ReceivesPlayerInput())
            .Add(new Sprite
            {
                Frame = assetStore.FirstAnimationFrame(GameAssets.Player.Run.South),
                Tint = new Vector4(0f, 0f, 0f, 1f),
                Layer = 0
            })
            .Add(new AlignedCharacter
            {
                CharacterDirection = CharacterDirection.South, CharacterType = CharacterType.Player
            })
            .Add(new Character())
            .Add(new Player())
            .Add(new HasShadow())
            .Add(new PlayerActionState())
            .Add(new EntityStats { Hitpoints = 5 })
            .ToEntity();
    }

    public static Entity CreateEnemy(World world, Position startPos, Vector2 size, EntityStats stats, Entity target,
        IAssetStore assetStore)
    {
        return world.Create()
            .Add(startPos)
            .Add(new Transform { Size = size, Scale = 1 })
            .Add(new Sprite
            {
                Frame = assetStore.FirstAnimationFrame(GameAssets.Enemy.Slime1.Idle),
                Tint = new Vector4(0f, 0f, 0f, 1f),
                Layer = 0
            })
            .Add(new NonDirectionalCharacter { CharacterType = CharacterType.Enemy })
            .Add(new Character())
            .Add(new Enemy())
            .Add(new Follows { Target = target, Type = FollowType.Linear, FollowRadius = 15, Speed = 2f })
            .Add(new HasShadow())
            .Add(new EnemyActionState())
            .Add(stats)
            .ToEntity();
    }

    public static Entity CreateFollowingCamera(World world, in Entity target, Vector2i viewport, Position startPos)
    {
        return world.Create()
            .Add(new Camera2D { Zoom = 1f, OrthographicSize = 20f, Viewport = viewport })
            .Add(new Follows { Target = target, Speed = 5f, FollowRadius = float.MaxValue, Type = FollowType.Lerp })
            .Add(startPos)
            .Add(new ReceivesCameraControl())
            .Add(new Hidden())
            .ToEntity();
    }

    public static Entity CreateBullet(World world, Position startPos, Velocity velocity, float rotation, float height,
        IAssetStore assetStore)
    {
        return world.Create()
            .Add(startPos)
            .Add(new Transform { Size = new Vector2(2f, 2f), Scale = 1.2f, Rotation = rotation, Height = height })
            .Add(new AnimatedSprite
            {
                CurrentFrameIndex = 0,
                AnimationClip = assetStore.Get(GameAssets.Projectile.Fireball),
                TimeInFrame = 0
            })
            .Add(velocity)
            .Add(new Projectile
            {
                Damage = 10,
                Lifetime = Lifetime.DestroyOnSleep,
                ExplosionAnimation = GameAssets.Explosion.BulletExplosion,
                ExplosionRadius = 1
            })
            .Add(new HasShadow())
            .Add(EntityCollisionBehavior.DestroySelf | EntityCollisionBehavior.Explode)
            .ToEntity();
    }

    public static Entity CreateCrossHair(World world, IAssetStore assetStore)
    {
        return world.Create()
            .Add(new SetPositionToMouse())
            .Add(new AbsolutePosition())
            .Add(new AbsoluteSize(64, 64))
            .Add(new UiAlignment(true, true))
            .Add(new Sprite { Frame = assetStore.Get(GameAssets.Crosshair.Simple), Tint = new Vector4(0f, 0f, 0f, 1f) })
            .ToEntity();
    }

    public static Entity CreateExplosion(World world, IAssetStore assetStore, Position position, float height,
        AssetRef<AnimationClip> animationClip)
    {
        return world.Create()
            .Add(position)
            .Add(new Transform { Size = new Vector2(1f, 1f), Scale = 2.5f, Height = height })
            .Add(new Projectile())
            .Add(new MarkedToDestroy { RemainingLifetime = assetStore.Get(animationClip).AnimationDuration() })
            .Add(new AnimatedSprite
            {
                CurrentFrameIndex = 0, AnimationClip = assetStore.Get(animationClip), TimeInFrame = 0
            })
            .ToEntity();
    }
}