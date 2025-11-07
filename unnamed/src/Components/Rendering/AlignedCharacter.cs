using engine.TextureProcessing;

using unnamed.Utils;

namespace unnamed.Components.Rendering;

public struct AlignedCharacter
{
    public CharacterDirection CharacterDirection;
    public Dictionary<CharacterDirection, VisualType> GraphicByDirection;
}