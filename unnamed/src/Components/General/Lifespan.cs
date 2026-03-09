namespace unnamed.Components.General;

public struct Lifespan(float max)
{
    public readonly float Max = max;
    public float Current = 0f;
}