namespace unnamed.Resources;

public struct Level(int value)
{
    public int Value = value;

    public static implicit operator int(Level l)
    {
        return l.Value;
    }
}