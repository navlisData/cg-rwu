using System.Drawing;

namespace engine.TextureProcessing;

public readonly record struct TextureId(int Value);
public readonly record struct SpriteSheetId(int Value);
public readonly record struct SpriteFrameId(SpriteSheetId Sheet, int Index);

public sealed class SpriteSheet
{
    public SpriteSheetId Id { get; internal set; }
    public TextureId Texture { get; init; }
    public List<RectangleF> Frames { get; } = new();
    public Dictionary<string, int>? NameToIndex { get; init; }
    public int IndexOf(string name)
        => this.NameToIndex is { } map && map.TryGetValue(name, out var index)
            ? index 
            : throw new KeyNotFoundException($"Frame-Name '{name}' not found.");
}