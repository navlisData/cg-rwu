using unnamed.Components.Drops;
using unnamed.Enums;

namespace unnamed.Utils.Loot;

public static class LootApi
{
    /// <summary>
    ///     Rolls a loot table and writes up to <see cref="LootTable.DropCount"/> distinct drops into the output buffer.
    /// </summary>
    /// <param name="table">The loot table component.</param>
    /// <param name="rng">A random source.</param>
    /// <param name="output">Destination buffer for selected drops.</param>
    /// <returns>The number of selected drops written to <paramref name="output"/>.</returns>
    public static int Roll(in LootTable table, Random rng, Span<DropType> output)
    {
        if (table.DropChance <= 0f)
            return 0;

        if (table.DropChance < 1f && rng.NextSingle() > table.DropChance)
            return 0;

        var loot = table.Loot;
        if (loot.Length == 0)
            return 0;

        var maxDrops = table.DropCount;
        if (maxDrops == 0)
            return 0;

        var dropsToPick = Math.Min(Math.Min(maxDrops, (byte)loot.Length), (byte)output.Length);
        if (dropsToPick <= 0)
            return 0;

        Span<int> pickedIndices = dropsToPick <= 64
            ? stackalloc int[dropsToPick]
            : new int[dropsToPick];

        var picked = 0;

        for (var pick = 0; pick < dropsToPick; pick++)
        {
            var totalWeight = 0;

            for (var i = 0; i < loot.Length; i++)
            {
                if (IsPicked(i, pickedIndices, picked))
                    continue;

                totalWeight += loot[i].Weight;
            }

            if (totalWeight <= 0)
                break;

            var roll = rng.Next(totalWeight);

            for (var i = 0; i < loot.Length; i++)
            {
                if (IsPicked(i, pickedIndices, picked))
                    continue;

                roll -= loot[i].Weight;
                if (roll < 0)
                {
                    pickedIndices[picked] = i;
                    output[picked] = loot[i].Type;
                    picked++;
                    break;
                }
            }
        }

        return picked;
    }

    /// <summary>
    ///     Checks whether an index has already been selected.
    /// </summary>
    private static bool IsPicked(int index, ReadOnlySpan<int> pickedIndices, int pickedCount)
    {
        for (var i = 0; i < pickedCount; i++)
        {
            if (pickedIndices[i] == index)
                return true;
        }

        return false;
    }
}