namespace engine.Control;

/// <summary>
///     Handles selection and timing of actions based on a priority function.
///     Intended for gameplay scenarios (e.g., OpenTK) where an entity can perform
///     one action at a time for a limited duration, and higher-priority actions may
///     preempt lower-priority ones.
/// </summary>
/// <typeparam name="TAction">
///     An enum type representing available actions (e.g., <c>PlayerAction</c>).
///     Must be an enum backed by a value type.
/// </typeparam>
/// <param name="getPriority">
///     A delegate that maps an action to its priority. Lower numbers indicate
///     lower priority; higher numbers indicate higher priority.
/// </param>
public readonly struct ActionControlHandler<TAction>(Func<TAction, byte> getPriority)
    where TAction : struct, Enum
{
    private readonly Func<TAction, byte> getPriority =
        getPriority ?? throw new ArgumentNullException(nameof(getPriority));


    /// <summary>
    ///     Advances the internal action timer and clears the current action once its remaining time elapses.
    /// </summary>
    /// <param name="currentAction">
    ///     The currently active action. Set to <see langword="null" /> when no action is active or when the
    ///     previously active action has finished.
    /// </param>
    /// <param name="remainingTime">
    ///     Remaining time (in seconds) for the current action. Set to <see langword="null" /> when there is
    ///     no active action. Decrements by <paramref name="deltaTime" /> each call.
    /// </param>
    /// <param name="deltaTime">
    ///     Time step (in seconds) since the last update.
    /// </param>
    /// <remarks>
    ///     If <paramref name="remainingTime" /> is <see langword="null" />, this method does nothing.
    ///     When the remaining time reaches zero or below, both <paramref name="currentAction" /> and
    ///     <paramref name="remainingTime" /> are cleared (set to <see langword="null" />).
    /// </remarks>
    public void Sync(ref TAction? currentAction, ref float? remainingTime, float deltaTime)
    {
        if (remainingTime is null)
        {
            return;
        }

        remainingTime -= deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = null;
            currentAction = null;
        }
    }


    /// <summary>
    ///     Attempts to start or replace the current action with a desired action for a given duration.
    /// </summary>
    /// <param name="currentAction">
    ///     The currently active action; may be replaced depending on priorities and remaining time.
    /// </param>
    /// <param name="remainingTime">
    ///     Remaining time (in seconds) for the current action; will be reset to <paramref name="duration" />
    ///     on a successful update.
    /// </param>
    /// <param name="desiredAction">
    ///     The action the caller wants to activate.
    /// </param>
    /// <param name="duration">
    ///     The duration (in seconds) to assign to <paramref name="desiredAction" /> if the update succeeds.
    /// </param>
    /// <param name="success">
    ///     <see langword="true" /> if the desired action was applied; otherwise <see langword="false" />.
    /// </param>
    /// <returns>
    ///     The action that remains active after the call (either the previous <paramref name="currentAction" />
    ///     or the new <paramref name="desiredAction" />).
    /// </returns>
    public TAction TryUpdateAction(
        ref TAction? currentAction,
        ref float? remainingTime,
        TAction desiredAction,
        float duration,
        out bool success)
    {
        if (!currentAction.HasValue)
        {
            currentAction = desiredAction;
            remainingTime = duration;
            success = true;
            return desiredAction;
        }

        byte currentPriority = this.getPriority(currentAction.Value);
        byte newPriority = this.getPriority(desiredAction);

        if (newPriority < currentPriority)
        {
            if (remainingTime is <= 0f)
            {
                currentAction = desiredAction;
                remainingTime = duration;
                success = true;
                return desiredAction;
            }

            success = false;
            return currentAction.Value;
        }

        currentAction = desiredAction;
        remainingTime = duration;
        success = true;
        return currentAction.Value;
    }

    /// <summary>
    ///     Attempts to start or replace the current action with a desired action, using a duration of <c>0</c> seconds.
    /// </summary>
    /// <param name="currentAction">
    ///     The currently active action; may be replaced depending on priorities and remaining time.
    /// </param>
    /// <param name="remainingTime">
    ///     Remaining time (in seconds) for the current action; will be reset to <c>0</c> on success.
    /// </param>
    /// <param name="desiredAction">
    ///     The action the caller wants to activate.
    /// </param>
    /// <param name="success">
    ///     <see langword="true" /> if the desired action was applied; otherwise <see langword="false" />.
    /// </param>
    /// <returns>
    ///     The action that remains active after the call (either the previous <paramref name="currentAction" />
    ///     or the new <paramref name="desiredAction" />).
    /// </returns>
    public TAction TryUpdateAction(
        ref TAction? currentAction,
        ref float? remainingTime,
        TAction desiredAction,
        out bool success)
    {
        return this.TryUpdateAction(ref currentAction, ref remainingTime, desiredAction, 0f, out success);
    }
}