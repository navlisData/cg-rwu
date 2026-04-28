namespace engine.Ecs.State;

public interface IState<TEnum> : INextState<TEnum> where TEnum : Enum
{
    bool HasChanged(out TEnum nextState);

    void DoChange();
}