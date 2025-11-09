using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public sealed class SpriteAnimationSystem(World world) : EntitySetSystem<float>(world,
    world.Query()
        .With<AnimatedSprite>()
        .Without<Sleeping>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref AnimatedSprite animatedSprite = ref e.Get<AnimatedSprite>();

        HandleAnimationRequest(ref animatedSprite);
        if (animatedSprite.AnimationClip is null) return;

        animatedSprite.TimeInFrame += dt;

        var clip = animatedSprite.AnimationClip;
        float secondsPerFrame = 1.0f / clip.Fps;
        if (animatedSprite.TimeInFrame < secondsPerFrame) return;

        animatedSprite.TimeInFrame -= secondsPerFrame;
        int frameCount = clip.Frames.Count;
        bool isLastFrame = animatedSprite.CurrentFrameIndex == frameCount - 1;
        if (isLastFrame)
        {
            if (!clip.Loop)
            {
                e.Remove<AnimatedSprite>();
                return;
            }

            animatedSprite.CurrentFrameIndex = 0;
        }
        else
        {
            animatedSprite.CurrentFrameIndex++;
        }

        StaticSprite currentFrame = clip.Frames[animatedSprite.CurrentFrameIndex];
        if (!e.Has<Sprite>())
            e.Add(new Sprite { Tint = new Vector4(0f, 0f, 0f, 1f), Layer = 0 });
        e.Get<Sprite>().Frame = currentFrame;
    }

    private static void HandleAnimationRequest(ref AnimatedSprite animatedSprite)
    {
        var requested = animatedSprite.RequestedAnimation;
        if (requested is null)
            return;

        byte currentPrio = animatedSprite.AnimationClip?.Priority ?? 0;
        bool sameAnimation = ReferenceEquals(requested, animatedSprite.AnimationClip);

        if (!sameAnimation && requested.Priority >= currentPrio)
            SetRequested(requested, ref animatedSprite);
    }

    private static void SetRequested(AnimationClip requestedClip, ref AnimatedSprite targetSprite)
    {
        targetSprite.AnimationClip = requestedClip;
        targetSprite.CurrentFrameIndex = 0;
        targetSprite.TimeInFrame = 0f;
        targetSprite.RequestedAnimation = null;
    }
}