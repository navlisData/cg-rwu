namespace engine.Ecs.State;

public interface INextState<TEnum>
{
    void QueueChange(TEnum nextState);

    TEnum Current();
}