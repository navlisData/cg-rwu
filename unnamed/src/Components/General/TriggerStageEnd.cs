namespace unnamed.Components.Tags;

public struct TriggerStageEnd(float timeRequired)
{
    public float TimeRequired = timeRequired;
    public float TimeRemaining = timeRequired;
}