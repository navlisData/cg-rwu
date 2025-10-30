using engine.TextureProcessing;

using OpenTK.Mathematics;

namespace unnamed.Components.Rendering;

public struct Sprite
{
    public StaticSprite Frame;
    public Vector4 Tint;
    public float Layer;
}