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

        bool updated = HandleAnimationRequest(ref animatedSprite);
        if (animatedSprite.AnimationClip is null)
        {
            return;
        }

        animatedSprite.TimeInFrame += dt;

        AnimationClip? clip = animatedSprite.AnimationClip;
        float secondsPerFrame = 1.0f / clip.Fps;
        if (!updated && animatedSprite.TimeInFrame < secondsPerFrame)
        {
            return;
        }

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
        {
            e.Add(new Sprite { Tint = new Vector4(0f, 0f, 0f, 1f), Layer = 0 });
        }

        e.Get<Sprite>().Frame = currentFrame;
    }

    private static bool HandleAnimationRequest(ref AnimatedSprite animatedSprite)
    {
        AnimationClip? requested = animatedSprite.RequestedAnimation;
        if (requested is null)
        {
            return false;
        }

        byte currentPrio = animatedSprite.AnimationClip?.Priority ?? 0;
        bool sameAnimation = ReferenceEquals(requested, animatedSprite.AnimationClip);

        if (sameAnimation || requested.Priority < currentPrio)
        {
            return false;
        }

        SetRequested(requested, ref animatedSprite);
        return true;
    }

    private static void SetRequested(AnimationClip requestedClip, ref AnimatedSprite targetSprite)
    {
        targetSprite.AnimationClip = requestedClip;
        targetSprite.CurrentFrameIndex = 0;
        targetSprite.TimeInFrame = 0f;
        targetSprite.RequestedAnimation = null;
    }
}