using engine.TextureProcessing;

namespace unnamed.Components.Rendering;

public struct AnimatedSprite
{
    public int CurrentFrameIndex;
    public AnimationClip? AnimationClip;
    public AnimationClip? RequestedAnimation;
    public float TimeInFrame;
}