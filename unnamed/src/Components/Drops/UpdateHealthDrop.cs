namespace unnamed.Components.Drops;

public struct UpdateHealthDrop
{
    public int HealthDeltaUnits { get; }

    /// <summary>
    ///     Creates an UpdateHealthDrop component.
    /// </summary>
    /// <param name="deltaUnits">The health delta in units.</param>
    public UpdateHealthDrop(int deltaUnits)
    {
        HealthDeltaUnits = deltaUnits;
    }

    /// <summary>
    ///     Default delta used when an explicit value is not provided.
    /// </summary>
    public static UpdateHealthDrop Default => new(deltaUnits: 2);
}