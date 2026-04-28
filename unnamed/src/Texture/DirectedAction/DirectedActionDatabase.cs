using unnamed.Enums;

namespace unnamed.Texture.DirectedAction;

public readonly struct DirectedActionDatabase
{
    private readonly Dictionary<CharacterType, IDirectedSpriteResolver> sets = new();

    public IDirectedSpriteResolver GetByCharacterType(CharacterType type)
    {
        return this.sets[type];
    }

    private void Register(CharacterType type, IDirectedSpriteResolver resolver)
    {
        this.sets[type] = resolver;
    }

    public DirectedActionDatabase()
    {
        DirectedActionDirectedSpriteResolver<PlayerAction> playerSet = new()
        {
            SpriteByDirectedAction =
            {
                [PlayerAction.Idle, CharacterDirection.North] = GameAssets.Player.Idle.North,
                [PlayerAction.Move, CharacterDirection.North] = GameAssets.Player.Run.North,
                [PlayerAction.Shoot, CharacterDirection.North] = GameAssets.Player.Attack.North,
                [PlayerAction.Idle, CharacterDirection.NorthEast] = GameAssets.Player.Idle.NorthEast,
                [PlayerAction.Move, CharacterDirection.NorthEast] = GameAssets.Player.Run.NorthEast,
                [PlayerAction.Shoot, CharacterDirection.NorthEast] = GameAssets.Player.Attack.NorthEast,
                [PlayerAction.Idle, CharacterDirection.East] = GameAssets.Player.Idle.East,
                [PlayerAction.Move, CharacterDirection.East] = GameAssets.Player.Run.East,
                [PlayerAction.Shoot, CharacterDirection.East] = GameAssets.Player.Attack.East,
                [PlayerAction.Idle, CharacterDirection.SouthEast] = GameAssets.Player.Idle.SouthEast,
                [PlayerAction.Move, CharacterDirection.SouthEast] = GameAssets.Player.Run.SouthEast,
                [PlayerAction.Shoot, CharacterDirection.SouthEast] = GameAssets.Player.Attack.SouthEast,
                [PlayerAction.Idle, CharacterDirection.South] = GameAssets.Player.Idle.South,
                [PlayerAction.Move, CharacterDirection.South] = GameAssets.Player.Run.South,
                [PlayerAction.Shoot, CharacterDirection.South] = GameAssets.Player.Attack.South,
                [PlayerAction.Idle, CharacterDirection.SouthWest] = GameAssets.Player.Idle.SouthWest,
                [PlayerAction.Move, CharacterDirection.SouthWest] = GameAssets.Player.Run.SouthWest,
                [PlayerAction.Shoot, CharacterDirection.SouthWest] = GameAssets.Player.Attack.SouthWest,
                [PlayerAction.Idle, CharacterDirection.West] = GameAssets.Player.Idle.West,
                [PlayerAction.Move, CharacterDirection.West] = GameAssets.Player.Run.West,
                [PlayerAction.Shoot, CharacterDirection.West] = GameAssets.Player.Attack.West,
                [PlayerAction.Idle, CharacterDirection.NorthWest] = GameAssets.Player.Idle.NorthWest,
                [PlayerAction.Move, CharacterDirection.NorthWest] = GameAssets.Player.Run.NorthWest,
                [PlayerAction.Shoot, CharacterDirection.NorthWest] = GameAssets.Player.Attack.NorthWest
            }
        };
        this.Register(CharacterType.Player, playerSet);
    }
}