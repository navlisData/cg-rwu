namespace engine.Ecs.State;

/// <summary>
///     Allows queuing of state changes
///     Currently unused due to runtime polymorphism constraints
/// </summary>
/// <typeparam name="TEnum">The underlying enum</typeparam>
public interface INextState<TEnum>
{
    void QueueChange(TEnum nextState);

    TEnum Current();
}