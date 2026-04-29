namespace engine.Ecs.State;

/// <summary>
///     Enum wrapper which keeps track of state changes and allows mutable access
/// </summary>
/// <param name="initial">Starting state</param>
/// <typeparam name="TEnum">Underlying enum</typeparam>
public struct State<TEnum>(TEnum initial) : IState<TEnum> where TEnum : Enum
{
    private TEnum next = initial;
    private TEnum current = initial;

    /// <summary>
    ///     Queue a state change that will be executed after all systems
    ///     Might be overwritten by a later system
    /// </summary>
    /// <param name="nextState"></param>
    public void QueueChange(TEnum nextState)
    {
        this.next = nextState;
    }

    /// <summary>
    ///     Returns the current state
    /// </summary>
    /// <returns></returns>
    public TEnum Current()
    {
        return this.current;
    }

    /// <summary>
    ///     Check if a state change is queued
    /// </summary>
    /// <param name="currentState"></param>
    /// <param name="nextState"></param>
    /// <returns></returns>
    public bool HasChanged(out TEnum currentState, out TEnum nextState)
    {
        currentState = this.current;
        nextState = this.next;

        return !this.next.Equals(this.current);
    }

    /// <summary>
    ///     Resolves the queued state change
    ///     USING IN A NORMAL SYSTEM MIGHT BRAKE STATE BEHAVIOR
    ///     I originally wanted to enforce this but passing
    ///     by ref somewhat breaks runtime polymorphism
    /// </summary>
    public void DoChange()
    {
        this.current = this.next;
    }
}