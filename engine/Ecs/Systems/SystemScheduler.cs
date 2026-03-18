namespace Engine.Ecs.Systems;

/// <summary>
/// Executes registered system callbacks in order and only when their state condition matches.
/// </summary>
/// <typeparam name="TState">The enum type representing the current execution state.</typeparam>
/// <typeparam name="TContext">The frame context passed to each system.</typeparam>
public sealed class SystemScheduler<TState, TContext>
    where TState : struct, Enum
{
    private static readonly Func<TState, bool> Always = static _ => true;
    private readonly List<Entry> entries = new();

    /// <summary>
    /// Registers a system callback that always runs, regardless of the current state.
    /// </summary>
    /// <param name="run">The system callback to execute.</param>
    /// <returns>The current scheduler instance.</returns>
    public SystemScheduler<TState, TContext> Add(Action<TContext> run)
    {
        ArgumentNullException.ThrowIfNull(run);

        this.entries.Add(new Entry(run, Always));
        return this;
    }

    /// <summary>
    /// Registers a system callback with a state condition.
    /// </summary>
    /// <param name="run">The system callback to execute.</param>
    /// <param name="canRun">The state predicate that decides whether the callback should run.</param>
    /// <returns>The current scheduler instance.</returns>
    public SystemScheduler<TState, TContext> Add(Action<TContext> run, Func<TState, bool> canRun)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(canRun);

        this.entries.Add(new Entry(run, canRun));
        return this;
    }

    /// <summary>
    /// Registers a system callback that runs only for a single state value.
    /// </summary>
    /// <param name="run">The system callback to execute.</param>
    /// <param name="requiredState">The state in which the callback is allowed to run.</param>
    /// <returns>The current scheduler instance.</returns>
    public SystemScheduler<TState, TContext> Add(Action<TContext> run, TState requiredState)
    {
        ArgumentNullException.ThrowIfNull(run);

        this.entries.Add(new Entry(
            run,
            state => EqualityComparer<TState>.Default.Equals(state, requiredState)));

        return this;
    }

    /// <summary>
    /// Executes all registered systems whose condition matches the current state.
    /// </summary>
    /// <param name="state">The current execution state.</param>
    /// <param name="context">The frame context.</param>
    public void Run(TState state, TContext context)
    {
        foreach (Entry entry in this.entries)
        {
            if (entry.CanRun(state))
            {
                entry.Run(context);
            }
        }
    }

    private sealed record Entry(Action<TContext> Run, Func<TState, bool> CanRun);
}