using engine.TextureProcessing;

using unnamed.Enums;

namespace unnamed.Texture.DirectedAction;

public sealed class DirectedActionMapper<TAction>
    where TAction : struct, Enum
{
    private readonly VisualType[,] directedActionTable;

    public DirectedActionMapper()
    {
        int actions = Enum.GetValues<TAction>().Length;
        int dirs = Enum.GetValues<CharacterDirection>().Length;
        this.directedActionTable = new VisualType[actions, dirs];
    }

    public VisualType this[TAction action, CharacterDirection dir]
    {
        get => this.directedActionTable[Convert.ToInt32(action), (int)dir];
        set => this.directedActionTable[Convert.ToInt32(action), (int)dir] = value;
    }
}