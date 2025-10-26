using engine.TextureProcessing;

using unnamed.Utils;

namespace unnamed.Components.Rendering;

public struct AlignedCharacter
{
    public CharacterDirection CharacterDirection;
    public SpriteFrameId FrameUp;
    public SpriteFrameId FrameUpRight;
    public SpriteFrameId FrameRight;
    public SpriteFrameId FrameDownRight;
    public SpriteFrameId FrameDown;
    public SpriteFrameId FrameDownLeft;
    public SpriteFrameId FrameLeft;
    public SpriteFrameId FrameUpLeft;

    public SpriteFrameId GetFrameIdByDirection() => this.CharacterDirection switch
    {
        CharacterDirection.Up => this.FrameUp,
        CharacterDirection.UpRight => this.FrameUpRight,
        CharacterDirection.Right => this.FrameRight,
        CharacterDirection.DownRight => this.FrameDownRight,
        CharacterDirection.Down => this.FrameDown,
        CharacterDirection.DownLeft => this.FrameDownLeft,
        CharacterDirection.Left => this.FrameLeft,
        CharacterDirection.UpLeft => this.FrameUpLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(CharacterDirection), CharacterDirection, null)
    };
}