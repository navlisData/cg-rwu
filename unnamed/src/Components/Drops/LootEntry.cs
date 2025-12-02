using unnamed.Enums;

namespace unnamed.Components.Drops;

public struct LootEntry
{
    public DropType Type { get; }
    public ushort Weight { get; }

    /// <summary>
    ///     Creates a loot entry.
    /// </summary>
    /// <param name="type">The drop type.</param>
    /// <param name="weight">The selection weight (must be &gt; 0).</param>
    public LootEntry(DropType type, ushort weight)
    {
        Type = type;
        Weight = weight;
    }
}