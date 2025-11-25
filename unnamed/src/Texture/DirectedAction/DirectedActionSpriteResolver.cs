using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.Texture.DirectedAction;

public sealed class DirectedActionSpriteResolver<TAction> : ISpriteResolver
    where TAction : struct, Enum
{
    private readonly DirectedActionMapper<TAction> spriteByDirectedAction = new();
    public DirectedActionMapper<TAction> SpriteByDirectedAction => this.spriteByDirectedAction;

    private VisualType Get(TAction action, CharacterDirection dir)
        => this.spriteByDirectedAction[action, dir];

    private static TAction FromIndex(byte index)
    {
        if (!Enum.IsDefined(typeof(TAction), index))
            throw new ArgumentOutOfRangeException(nameof(index));

        return (TAction)Enum.ToObject(typeof(TAction), index);
    }

    public VisualType Get(byte actionIndex, CharacterDirection dir)
        => this.Get(FromIndex(actionIndex), dir);
}