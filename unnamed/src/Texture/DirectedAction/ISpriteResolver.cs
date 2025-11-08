using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.Texture.DirectedAction;

public interface ISpriteResolver
{
    VisualType Get(int actionIndex, CharacterDirection dir);
}