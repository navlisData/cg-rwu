namespace unnamed.Resources;

public struct DeltaTime(float value)
{
    public float Value = value;

    public static implicit operator float(DeltaTime deltaTime)
    {
        return deltaTime.Value;
    }
}