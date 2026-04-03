using engine.TextureProcessing;

using unnamed.Components.UI;

namespace unnamed.Components.Rendering;

public struct AnimatedSprite(int currentFrameIndex, AnimationClip?  animationClip, AnimationClip? requestedAnimation, float timeInFrame, UiPivot pivot)
{
    public int CurrentFrameIndex = currentFrameIndex; 
    public AnimationClip? AnimationClip = animationClip;
    public AnimationClip? RequestedAnimation = requestedAnimation;
    public float TimeInFrame = timeInFrame;
    public UiPivot Pivot = pivot;
}