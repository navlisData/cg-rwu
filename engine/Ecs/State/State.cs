namespace engine.Ecs.State;

public struct State<TEnum>(TEnum initial) : IState<TEnum> where TEnum : Enum
{
    private TEnum next = initial;
    private TEnum current = initial;

    public void QueueChange(TEnum nextState)
    {
        this.next = nextState;
    }

    public TEnum Current()
    {
        return this.current;
    }

    public bool HasChanged(out TEnum nextState)
    {
        if (this.next.Equals(this.current))
        {
            nextState = this.current;
            return false;
        }

        nextState = this.next;
        return true;
    }

    public void DoChange()
    {
        this.current = this.next;
    }
}