using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public sealed class SpriteAnimationSystem(World world) : EntitySetSystem<float>(world,
    new QueryBuilder()
        .With<AnimatedSprite>()
        .Without<Sleeping>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref AnimatedSprite animatedSprite = ref handle.Get<AnimatedSprite>();

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
                handle.Remove<AnimatedSprite>();
                return;
            }

            animatedSprite.CurrentFrameIndex = 0;
        }
        else
        {
            animatedSprite.CurrentFrameIndex++;
        }

        StaticSprite currentFrame = clip.Frames[animatedSprite.CurrentFrameIndex];
        if (!handle.Has<Sprite>())
        {
            handle.Add(new Sprite(currentFrame, animatedSprite.Pivot));
        }

        handle.Get<Sprite>().Frame = currentFrame;
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