namespace engine.Ecs.State;

/// <summary>
///     Allows resolving of state changes
///     Currently unused due to runtime polymorphism constraints
/// </summary>
/// <typeparam name="TEnum">The underlying enum</typeparam>
public interface IState<TEnum> : INextState<TEnum> where TEnum : Enum
{
    bool HasChanged(out TEnum currentState, out TEnum nextState);

    void DoChange();
}