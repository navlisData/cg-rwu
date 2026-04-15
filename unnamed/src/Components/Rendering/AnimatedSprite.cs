using engine.TextureProcessing;

namespace unnamed.Components.Rendering;

public struct AnimatedSprite(
    int currentFrameIndex,
    AnimationClip? animationClip,
    AnimationClip? requestedAnimation,
    float timeInFrame,
    Pivot pivot)
{
    public int CurrentFrameIndex = currentFrameIndex;
    public AnimationClip? AnimationClip = animationClip;
    public AnimationClip? RequestedAnimation = requestedAnimation;
    public float TimeInFrame = timeInFrame;
    public Pivot Pivot = pivot;
}