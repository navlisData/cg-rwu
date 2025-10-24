using engine.TextureProcessing;

using OpenTK.Mathematics;

namespace unnamed.Components.Rendering;

public struct Sprite
{
    // Points to (Sheet, FrameIndex)
    public SpriteFrameId Frame;
    public Vector4 Tint;
    public float Layer;
}