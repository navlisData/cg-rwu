using engine.Control;

namespace unnamed.Enums;

public enum PlayerAction : byte
{
    Idle = 0,
    Move = 1,
    Shoot = 2
}

/// <summary>
///     Priority mapping for <see cref="PlayerAction"/> used by <see cref="ActionControlHandler{TAction}"/>.
///     Higher values mean higher priority.
/// </summary>
public static class PlayerActionExtensions
{
    /// <summary>
    ///     Returns the priority of a player action. Higher values indicate higher priority.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <returns>The priority value for <paramref name="action"/>.</returns>
    public static byte Priority(this PlayerAction action) => action switch
    {
        PlayerAction.Idle => 0,
        PlayerAction.Move => 0,
        PlayerAction.Shoot => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}