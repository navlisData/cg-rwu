using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.UI;

namespace unnamed.Components.Rendering;

public struct Sprite(StaticSprite frame, Color4? tint, UiPivot pivot)
{
    public Sprite(StaticSprite frame, UiPivot pivot):this(frame, null, pivot)
    {}
    
    public StaticSprite Frame = frame;
    public Color4? Tint = tint;
    public UiPivot pivot = pivot;
}