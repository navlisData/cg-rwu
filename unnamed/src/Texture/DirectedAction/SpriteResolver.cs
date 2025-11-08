using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.Texture.DirectedAction;

public interface SpriteResolver
{
    VisualType Get(int actionIndex, CharacterDirection dir);
}