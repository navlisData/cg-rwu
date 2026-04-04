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
using unnamed.Utils;

namespace unnamed.Prefabs;

public static class PrefabFactory
{
    public static Entity CreatePlayer(World world, Position startPos,
        IAssetStore assetStore)
    {
        return world.Create()
            .Add(startPos)
            .Add(new Velocity())
            .Add(new Transform { Size = new Vector2(1f, 1f), Scale = 5f })
            .Add(new ReceivesPlayerInput())
            .Add(new Sprite(assetStore.FirstAnimationFrame(GameAssets.Player.Run.South)))
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
            .Add(new UiReferenceSize(frame.RectPx.Width, frame.RectPx.Height))
            .Add(new UiReferenceOffset())
            .Add(new UiElement())
            .Add(UiAnchor.TopLeft)
            .Add(UiScaleMode.Uniform)
            .Add(new Sprite(frame))
            .ToEntity();
    }

    public static Entity CreateEnemy(World world, Position startPos, EntityStats stats, Entity target,
        IAssetStore assetStore)
    {
        return world.Create()
            .Add(startPos)
            .Add(new Transform { Size = new Vector2(1f, 1f), Scale = 3 })
            .Add(new Sprite(assetStore.FirstAnimationFrame(GameAssets.Enemy.Slime1.Idle)))
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

    public static Entity CreateText(World world, string text, Color color, StaticTextTextureFactory textFactory,
        TextAlignment textAlignment = TextAlignment.Start)
    {
        Texture2D titleTexture = textFactory.CreateTexture(text, color, textAlignment);

        return world.Create()
            .Add(new StaticTextTexture(titleTexture, UiPivot.Center.ToVector2()))
            .Add(new UiReferenceSize(titleTexture.Width, titleTexture.Height))
            .Add(new UiReferenceOffset())
            .Add(new UiElement())
            .Add(UiAnchor.Center)
            .Add(UiScaleMode.Uniform)
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
            .Add(new Transform { Size = new Vector2(2f, 1f), Scale = 2.4f, Rotation = rotation, Height = height })
            .Add(new AnimatedSprite(0, null, assetStore.Get(GameAssets.Projectile.Fireball), 0, UiPivot.Center))
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
            .Add(new Spawner(0.01f, 1, 8, 5.0f, null, spawnCrosshair, null))
            .ToEntity();
    }

    public static Entity CreateCrossHair(World world, AbsolutePosition position, IAssetStore assetStore)
    {
        var baseTint = new Color4(0.95f, 0.61f, 0.07f, 0.25f);
        var tint = ColorUtils.CreateSlightColorVariation(baseTint, strength: 0.15f, centerBias: 0.55f,
            sharedAmount: 1.35f);

        return world.Create()
            .Add(position)
            .Add(new AbsoluteSize(16, 16))
            .Add(new UiElement())
            .Add(new Sprite(assetStore.Get(GameAssets.Crosshair.ParticleCloud), tint))
            .Add(new Lifespan(0.5f))
            .Add(new InfluencedByWind(10))
            .Add(new FadeAnimation(false, 0.5f, FadeAnimationType.FadeOut))
            .ToEntity();
    }

    public static Entity CreateDrop(World world, DropType dropType, IAssetStore assetStore, Position position,
        Entity player)
    {
        StaticSprite frame = assetStore.Get(dropType.GetAsset());
        Transform transform = new() { Size = new Vector2(1f, 1f), Scale = 1.5f };

        EntityHandle dropHandle = world.Create()
            .Add(new VisibleEntity())
            .Add(position)
            .Add(dropType)
            .Add(new PulseAnimation(transform, 1.45f, 1.65f, 0.8f))
            .Add(new CanCollideWithPlayer { Range = 0.5f })
            .Add(new Follows { Target = player, Speed = 8f, FollowRadius = 8, Type = FollowType.Linear })
            .Add(transform)
            .Add(new Sprite(frame));

        dropHandle.AddDefaultDropComponent(dropType);
        return dropHandle.ToEntity();
    }

    public static Entity CreateExplosion(World world, IAssetStore assetStore, Position position, float height,
        AssetRef<AnimationClip> animationClip)
    {
        return world.Create()
            .Add(position)
            .Add(new Transform { Size = new Vector2(1f, 1f), Scale = 5f, Height = height })
            .Add(new Projectile())
            .Add(new MarkedToDestroy { RemainingLifetime = assetStore.Get(animationClip).AnimationDuration() })
            .Add(new AnimatedSprite(0, assetStore.Get(animationClip), null, 0, UiPivot.Center))
            .ToEntity();
    }

    public static Entity CreatePortal(World world, Position pos, IAssetStore assetStore)
    {
        return world.Create()
            .Add(pos)
            .Add(new Transform { Size = new Vector2(1f, 1f), Scale = 16f })
            .Add(new Sprite(assetStore.Get(GameAssets.Props.Portal)))
            .Add(new Prop())
            .Add(new CanCollideWithPlayer { Range = 2f })
            .Add(new TriggerStageEnd())
            .ToEntity();
    }

    public static Entity CreateMapDeco(World world, Position pos, Vector2 size, StaticSprite asset)
    {
        return world.Create()
            .Add(pos)
            .Add(new Transform { Size = size, Scale = 2f })
            .Add(new Sprite(asset))
            .Add(new Prop())
            .ToEntity();
    }
}