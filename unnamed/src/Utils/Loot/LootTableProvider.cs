using unnamed.Components.Drops;
using unnamed.Enums;

namespace unnamed.Utils.Loot;

public static class LootTableProvider
{
    public static readonly LootTable SlimeLootTable = new LootTableBuilder()
        .SetDropChance(0.25f)
        .SetDropCount(1)
        .AddLootOption(DropType.MaxHealthDrop, 20)
        .AddLootOption(DropType.UpdateHealthDrop, 80)
        .Build();
}