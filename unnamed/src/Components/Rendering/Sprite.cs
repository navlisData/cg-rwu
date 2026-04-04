using engine.TextureProcessing;

using OpenTK.Mathematics;

namespace unnamed.Components.Rendering;

public struct Sprite(StaticSprite frame, Color4? tint = null)
{
    public StaticSprite Frame = frame;
    public Color4? Tint = tint;
}