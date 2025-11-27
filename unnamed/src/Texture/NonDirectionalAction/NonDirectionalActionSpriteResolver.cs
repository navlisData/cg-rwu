using engine.TextureProcessing;

namespace unnamed.Texture.NonDirectionalAction;

/// <summary>
/// Resolves visuals for a non-directional action enum.
/// </summary>
/// <typeparam name="TAction">The action enum type.</typeparam>
public sealed class NonDirectionalActionSpriteResolver<TAction> : INonDirectionalSpriteResolver
    where TAction : struct, Enum
{
    private readonly Dictionary<TAction, VisualType> spriteByAction = new();

    /// <summary>
    /// Gets the mutable mapping used for configuring sprites by action.
    /// </summary>
    public IDictionary<TAction, VisualType> SpriteByAction => this.spriteByAction;

    /// <summary>
    /// Resolves the visual type for the given action index.
    /// </summary>
    /// <param name="actionIndex">The numeric action identifier (enum underlying value).</param>
    /// <returns>The resolved <see cref="VisualType"/>.</returns>
    public VisualType Get(byte actionIndex)
    {
        var action = ActionEnum.FromIndex<TAction>(actionIndex);
        return this.spriteByAction[action];
    }
}