namespace unnamed.Components.Drops;

public struct MaxHealthDrop
{
    public int MaxHealthDeltaUnits { get; }

    /// <summary>
    ///     Creates an MaxHealthDrop component.
    /// </summary>
    /// <param name="deltaUnits">The health delta in units.</param>
    public MaxHealthDrop(int deltaUnits)
    {
        MaxHealthDeltaUnits = deltaUnits;
    }

    /// <summary>
    ///     Default delta used when an explicit value is not provided.
    /// </summary>
    public static MaxHealthDrop Default => new(deltaUnits: 2);
}