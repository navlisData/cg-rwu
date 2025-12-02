using unnamed.Components.Drops;
using unnamed.Enums;

namespace unnamed.Utils.Loot;

public sealed class LootTableBuilder
{
    private float dropChance = 1f;
    private byte dropCount = 1;
    private readonly List<LootEntry> entries = new();

    /// <summary>
    ///     Sets the probability that any loot will drop at all.
    /// </summary>
    /// <param name="chance">Chance in range [0..1]. Values are clamped.</param>
    /// <returns>The builder instance.</returns>
    public LootTableBuilder SetDropChance(float chance)
    {
        this.dropChance = Math.Clamp(chance, 0f, 1f);
        return this;
    }

    /// <summary>
    ///     Sets the maximum number of drops to produce from this table.
    /// </summary>
    /// <param name="count">Maximum number of drops.</param>
    /// <returns>The builder instance.</returns>
    public LootTableBuilder SetDropCount(byte count)
    {
        this.dropCount = count;
        return this;
    }

    /// <summary>
    ///     Adds a loot option to the table.
    /// </summary>
    /// <param name="type">The drop type.</param>
    /// <param name="weight">The relative selection weight. Must be &gt; 0.</param>
    /// <returns>The builder instance.</returns>
    public LootTableBuilder AddLootOption(DropType type, ushort weight)
    {
        if (weight == 0)
            throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be greater than 0.");

        this.entries.Add(new LootEntry(type, weight));
        return this;
    }

    /// <summary>
    /// Builds the loot table instance.
    /// </summary>
    /// <returns>A loot table containing the configured entries.</returns>
    public LootTable Build()
    {
        var loot = this.entries.Count == 0 ? [] : this.entries.ToArray();

        if (dropCount > loot.Length)
            dropCount = (byte)loot.Length;

        return new LootTable { DropChance = this.dropChance, DropCount = dropCount, Loot = loot };
    }
}