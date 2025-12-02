using unnamed.Components.Drops;
using unnamed.Enums;

namespace unnamed.Utils.Loot;

public static class LootTableProvider
{
    public static readonly LootTable SlimeLootTable = new LootTableBuilder()
        .SetDropChance(0.5f)
        .SetDropCount(1)
        .AddLootOption(DropType.MaxHealthDrop, 50)
        .AddLootOption(DropType.UpdateHealthDrop, 50)
        .Build();
}