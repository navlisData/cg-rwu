using engine.TextureProcessing;

namespace unnamed.Texture.NonDirectionalAction;

/// <summary>
/// Resolves a visual representation for a given action index without any directional context.
/// </summary>
public interface INonDirectionalSpriteResolver
{
    /// <summary>
    /// Resolves the visual type for the given action index.
    /// </summary>
    /// <param name="actionIndex">The numeric action identifier (enum underlying value).</param>
    /// <returns>The resolved <see cref="VisualType"/>.</returns>
    VisualType Get(byte actionIndex);
}