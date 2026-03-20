namespace unnamed.Components.Rendering;

public struct FadeAnimation(bool repeating, float interval, FadeAnimationType startType)
{
    public readonly bool Repeating = repeating;
    public readonly float Interval = interval;
    public FadeAnimationType Type = startType;
    public float Time = 0f;
}

public enum FadeAnimationType
{
    FadeIn,
    FadeOut
}