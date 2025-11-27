using engine.Control;

namespace unnamed.Enums;

public enum EnemyAction : byte
{
    Idle = 0,
    Move = 1,
    Attack = 2,
    Damage = 3
}

/// <summary>
///     Priority mapping for <see cref="EnemyAction"/> used by <see cref="ActionControlHandler{TAction}"/>.
///     Higher values mean higher priority.
/// </summary>
public static class EnemyActionExtensions
{
    /// <summary>
    ///     Returns the priority of a enemy action. Higher values indicate higher priority.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <returns>The priority value for <paramref name="action"/>.</returns>
    public static byte Priority(this EnemyAction action) => action switch
    {
        EnemyAction.Idle => 0,
        EnemyAction.Move => 0,
        EnemyAction.Attack => 5,
        EnemyAction.Damage => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}