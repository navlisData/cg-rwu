using unnamed.Enums;

namespace unnamed.Utils.Health;

/// <summary>
///     Converts health units to discrete heart slot status.
/// </summary>
public static class HeartStatusUtil
{
    /// <summary>
    ///     Computes the heart status for a given slot index based on current health units.
    /// </summary>
    /// <param name="currentHealthUnits">Current health units.</param>
    /// <param name="slotIndex">Heart slot index (0-based).</param>
    /// <returns>The computed heart status.</returns>
    public static HeartStatus ComputeStatus(int currentHealthUnits, int slotIndex)
    {
        int units = currentHealthUnits - (slotIndex * 2);
        if (units <= 0) return HeartStatus.Empty;
        if (units == 1) return HeartStatus.Half;
        return HeartStatus.Full;
    }

    /// <summary>
    ///     Computes the number of heart slots required for a maximum health-unit value.
    /// </summary>
    /// <param name="maxHealthUnits">Maximum health units.</param>
    /// <returns>Number of heart slots required.</returns>
    public static int RequiredSlots(int maxHealthUnits)
    {
        return (maxHealthUnits + 1) / 2;
    }
}