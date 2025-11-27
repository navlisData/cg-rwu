using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.Texture.DirectedAction;

public interface IDirectedSpriteResolver
{
    VisualType Get(byte actionIndex, CharacterDirection dir);
}