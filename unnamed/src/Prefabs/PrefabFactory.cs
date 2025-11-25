using Engine.Ecs;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.General;
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
        entity.Add(new Velocity { Value = startVel });
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
        return entity;
    }

    public static Entity CreateFollowingCamera(World world, in Entity target, Vector2i viewport, Position startPos)
    {
        Entity entity = world.CreateEntity();
        entity.Add(new Camera2D { Zoom = 1f, OrthographicSize = 20f, Viewport = viewport });
        entity.Add(new Follows { Target = target, LerpSpeed = 10f });
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

    public static Entity CreateBullet(World world, Position startPos, Vector2 velocity, float rotation, float height,
        IAssetStore assetStore)
    {
        Entity entity = world.CreateEntity();
        entity.Add(startPos);
        entity.Add(new Transform { Size = new Vector2(2f, 2f), Scale = 1.2f, Rotation = rotation, Height = height });
        entity.Add(new AnimatedSprite
        {
            CurrentFrameIndex = 0, AnimationClip = assetStore.Get(GameAssets.Projectile.Fireball), TimeInFrame = 0
        });
        entity.Add(new Velocity { Value = velocity });
        entity.Add(new Projectile { Damage = 10, Lifetime = Lifetime.DestroyOnSleep });
        entity.Add(new HasShadow());
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
}