using Engine.Ecs.Systems;

using unnamed.Enums;

namespace unnamed.Systems.SystemScheduler;

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
    /// Registers a system callback that runs while the player is inside the game,
    /// including active gameplay and pause mode.
    /// </summary>
    /// <typeparam name="TContext">The frame context type.</typeparam>
    /// <param name="scheduler">The scheduler to configure.</param>
    /// <param name="run">The system callback to execute.</param>
    /// <returns>The current scheduler instance.</returns>
    public static SystemScheduler<GameState, TContext> DuringGame<TContext>(
        this SystemScheduler<GameState, TContext> scheduler,
        Action<TContext> run)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(run);

        return scheduler.Add(run, state => state is GameState.InGame or GameState.Paused);
    }

    /// <summary>
    /// Registers a system callback that runs only while gameplay is active.
    /// </summary>
    /// <typeparam name="TContext">The frame context type.</typeparam>
    /// <param name="scheduler">The scheduler to configure.</param>
    /// <param name="run">The system callback to execute.</param>
    /// <returns>The current scheduler instance.</returns>
    public static SystemScheduler<GameState, TContext> DuringGameplay<TContext>(
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
    public static SystemScheduler<GameState, TContext> WhilePaused<TContext>(
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
    public static SystemScheduler<GameState, TContext> WhenWon<TContext>(
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
    public static SystemScheduler<GameState, TContext> WhenLost<TContext>(
        this SystemScheduler<GameState, TContext> scheduler,
        Action<TContext> run)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(run);

        return scheduler.Add(run, GameState.Lost);
    }
}