using engine.TextureProcessing;

using OpenTK.Mathematics;

namespace unnamed.Components.Rendering;

public struct Sprite(StaticSprite frame, Color4? tint)
{
    public Sprite(StaticSprite frame):this(frame, null)
    {}
    
    public StaticSprite Frame = frame;
    public Color4? Tint = tint;
}