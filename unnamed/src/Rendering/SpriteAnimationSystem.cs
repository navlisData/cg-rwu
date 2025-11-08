using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public sealed class SpriteAnimationSystem(World world, IAssetStore assetStore) : EntitySetSystem<float>(world,
    world.Query()
        .With<AnimatedSprite>()
        .Without<Sleeping>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref AnimatedSprite animatedSprite = ref e.Get<AnimatedSprite>();

        animatedSprite.TimeInFrame += dt;
        float secondsPerFrame = 1.0f / animatedSprite.AnimationClip.Fps;

        if (animatedSprite.TimeInFrame >= secondsPerFrame)
        {
            animatedSprite.TimeInFrame -= secondsPerFrame;
            animatedSprite.CurrentFrameIndex =
                (animatedSprite.CurrentFrameIndex + 1) % animatedSprite.AnimationClip.Frames.Count;
        }

        StaticSprite currentFrame = animatedSprite.AnimationClip.Frames[animatedSprite.CurrentFrameIndex];
        if (!e.Has<Sprite>())
            e.Add(new Sprite { Tint = new Vector4(0f, 0f, 0f, 1f), Layer = 0 });
        e.Get<Sprite>().Frame = currentFrame;
    }
}