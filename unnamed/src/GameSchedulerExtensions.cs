using Engine.Ecs.Systems;

using unnamed.Enums;

namespace unnamed;

/// <summary>
/// Provides game-specific scheduler extensions for common game states.
/// </summary>
public static class GameSchedulerExtensions
{
    /// <summary>
    /// Registers a system callback that runs in every game state.
    /// </summary>
    /// <typeparam name="TContext">The frame context type.</typeparam>
    /// <param name="scheduler">The scheduler to configure.</param>
    /// <param name="run">The system callback to execute.</param>
    /// <returns>The current scheduler instance.</returns>
    public static SystemScheduler<GameState, TContext> Always<TContext>(
        this SystemScheduler<GameState, TContext> scheduler,
        Action<TContext> run)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(run);

        return scheduler.Add(run);
    }

    /// <summary>
    /// Registers a system callback that runs only while the game is in the in-game state.
    /// </summary>
    /// <typeparam name="TContext">The frame context type.</typeparam>
    /// <param name="scheduler">The scheduler to configure.</param>
    /// <param name="run">The system callback to execute.</param>
    /// <returns>The current scheduler instance.</returns>
    public static SystemScheduler<GameState, TContext> InGame<TContext>(
        this SystemScheduler<GameState, TContext> scheduler,
        Action<TContext> run)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(run);

        return scheduler.Add(run, GameState.InGame);
    }

    /// <summary>
    /// Registers a system callback that runs only while the game is paused.
    /// </summary>
    /// <typeparam name="TContext">The frame context type.</typeparam>
    /// <param name="scheduler">The scheduler to configure.</param>
    /// <param name="run">The system callback to execute.</param>
    /// <returns>The current scheduler instance.</returns>
    public static SystemScheduler<GameState, TContext> Paused<TContext>(
        this SystemScheduler<GameState, TContext> scheduler,
        Action<TContext> run)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(run);

        return scheduler.Add(run, GameState.Paused);
    }

    /// <summary>
    /// Registers a system callback that runs only when the game is won.
    /// </summary>
    /// <typeparam name="TContext">The frame context type.</typeparam>
    /// <param name="scheduler">The scheduler to configure.</param>
    /// <param name="run">The system callback to execute.</param>
    /// <returns>The current scheduler instance.</returns>
    public static SystemScheduler<GameState, TContext> Won<TContext>(
        this SystemScheduler<GameState, TContext> scheduler,
        Action<TContext> run)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(run);

        return scheduler.Add(run, GameState.Won);
    }

    /// <summary>
    /// Registers a system callback that runs only when the game is lost.
    /// </summary>
    /// <typeparam name="TContext">The frame context type.</typeparam>
    /// <param name="scheduler">The scheduler to configure.</param>
    /// <param name="run">The system callback to execute.</param>
    /// <returns>The current scheduler instance.</returns>
    public static SystemScheduler<GameState, TContext> Lost<TContext>(
        this SystemScheduler<GameState, TContext> scheduler,
        Action<TContext> run)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(run);

        return scheduler.Add(run, GameState.Lost);
    }
}