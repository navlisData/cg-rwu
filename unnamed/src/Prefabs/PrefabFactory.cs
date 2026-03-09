using Engine.Ecs;

using engine.TextureProcessing;
using engine.TextureProcessing.Text;

using OpenTK.Mathematics;

using SixLabors.Fonts;
using SixLabors.ImageSharp;

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
            .Add(new VisibleEntity())
            .Add(new Character())
            .Add(new Player())
            .Add(new HasShadow())
            .Add(new PlayerActionState())
            .Add(new EntityStats(10, 16) { MaxAttackCooldown = 1f })
            .Add(new HudHearts { hearts = [] })
            .Add(new HealthHudLayoutDirty())
            .ToEntity();
    }

    public static Entity CreateHealthIndicator(World world, IAssetStore assetStore, HeartStatus heartStatus)
    {
        StaticSprite frame = assetStore.Get(heartStatus.GetAsset());

        return world.Create()
            .Add(new AbsoluteSize(frame.RectPx.Width, frame.RectPx.Height))
            .Add(new UiAlignment())
            .Add(new Transform { Size = new Vector2(1f, 1f), Scale = 1f })
            .Add(new Sprite { Frame = frame, Tint = new Vector4(0f, 0f, 0f, 1f) })
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
            .Add(new VisibleEntity())
            .Add(new Character())
            .Add(new CanCollideWithPlayer { Range = 2f })
            .Add(new Enemy())
            .Add(new Follows { Target = target, Type = FollowType.Linear, FollowRadius = 15, Speed = 2f })
            .Add(new HasShadow())
            .Add(new EnemyActionState())
            .Add(stats)
            .ToEntity();
    }

    public static Entity CreateText(World world, String text, Color color, StaticTextTextureFactory textFactory,
        Vector2i windowSize, TextAlignment textAlignment = TextAlignment.Start)
    {
        Texture2D titleTexture = textFactory.CreateTexture(text, color, textAlignment);

        return world.Create()
            .Add(new StaticTextTexture(titleTexture))
            .Add(new AbsoluteSize(titleTexture.Width, titleTexture.Height))
            .Add(new AbsolutePosition(windowSize.X * 0.5f, windowSize.Y * 0.5f))
            .Add(new UiAlignment { VerticallyCentered = true, HorizontallyCentered = true })
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
                Damage = 8,
                Lifetime = Lifetime.DestroyOnSleep,
                ExplosionAnimation = GameAssets.Explosion.BulletExplosion,
                ExplosionRadius = 1
            })
            .Add(new HasShadow())
            .Add(EntityCollisionBehavior.DestroySelf | EntityCollisionBehavior.Explode)
            .ToEntity();
    }

    public static Entity CreateCrossHairSpawner(World world, Func<World, AbsolutePosition, Entity> spawnCrosshair)
    {
        return world.Create()
            .Add(new SetPositionToMouse())
            .Add(new AbsolutePosition())
            .Add(new UiAlignment(true, true))
            .Add(new Spawner(0.02f, 1, 12, 7.5f, null, spawnCrosshair, null))
            .ToEntity();
    }

    public static Entity CreateCrossHair2(World world, AbsolutePosition position, IAssetStore assetStore)
    {
        return world.Create()
            .Add(position)
            .Add(new AbsoluteSize(16, 16))
            .Add(new UiAlignment(true, true))
            .Add(new Sprite { Frame = assetStore.Get(GameAssets.Crosshair.Simple), Tint = new Vector4(0f, 0f, 0f, 1f) })
            .Add(new Lifespan(0.5f))
            .Add(new InfluencedByWind(10))
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

    public static Entity CreateDrop(World world, DropType dropType, IAssetStore assetStore, Position position,
        Entity player)
    {
        StaticSprite frame = assetStore.Get(dropType.GetAsset());
        Transform transform = new() { Size = new Vector2(1f, 1f), Scale = 1.5f, Height = frame.RectPx.Height };

        EntityHandle dropHandle = world.Create()
            .Add(new VisibleEntity())
            .Add(position)
            .Add(dropType)
            .Add(new PulseAnimation(transform, 1.45f, 1.65f, 0.8f))
            .Add(new CanCollideWithPlayer { Range = 0.5f })
            .Add(new Follows { Target = player, Speed = 8f, FollowRadius = 8, Type = FollowType.Linear })
            .Add(transform)
            .Add(new Sprite { Frame = frame, Tint = new Vector4(0f, 0f, 0f, 1f) });

        dropHandle.AddDefaultDropComponent(dropType);
        return dropHandle.ToEntity();
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

    public static Entity CreatePortal(World world, Position pos, IAssetStore assetStore)
    {
        return world.Create()
            .Add(pos)
            .Add(new Transform { Size = new Vector2(1.3f, 1f), Scale = 8 })
            .Add(new Sprite
            {
                Frame = assetStore.Get(GameAssets.Props.Portal), Tint = new Vector4(0f, 0f, 0f, 1f), Layer = 0
            })
            .Add(new Prop())
            .Add(new CanCollideWithPlayer { Range = 2f })
            .Add(new TriggerStageEnd())
            .ToEntity();
    }

    public static Entity CreateMapDeco(World world, Position pos, Vector2 size, StaticSprite asset)
    {
        return world.Create()
            .Add(pos)
            .Add(new Transform { Size = size, Scale = 1 })
            .Add(new Sprite { Frame = asset, Tint = new Vector4(0f, 0f, 0f, 1f), Layer = 0 })
            .Add(new Prop())
            .ToEntity();
    }
}