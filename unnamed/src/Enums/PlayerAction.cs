namespace unnamed.Enums;

public enum PlayerAction
{
    Idle = 0,
    Move = 1,
    Shoot = 2
}

public static class PlayerActionExtensions
{
    public static byte GetPriority(this PlayerAction action) => action switch
    {
        PlayerAction.Idle  => 0,
        PlayerAction.Move  => 0,
        PlayerAction.Shoot => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}