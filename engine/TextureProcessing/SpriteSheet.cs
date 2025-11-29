using OpenTK.Mathematics;

namespace engine.TextureProcessing;

public readonly record struct SpriteSheetId(int Value);

public sealed class SpriteSheet
{
    public SpriteSheetId Id { get; init; }
    public Vector2i Size { get; init; }
}