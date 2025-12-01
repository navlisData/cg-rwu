using Engine.Ecs;

using unnamed.Components.General;
using unnamed.Components.Tags;

namespace unnamed.Utils.Health;

/// <summary>
///     Health mutation helpers for entities addressed via <see cref="EntityHandle"/>.
///     All methods clamp values and mark dependent HUD state as dirty when changes occur.
/// </summary>
public static class EntityHealthExtension
{
    /// <summary>
    ///     Sets the entity's current hitpoints, clamps to [0, MaxHealthUnits],
    ///     and marks the HUD visuals as dirty if the value changed.
    /// </summary>
    /// <param name="handle">Handle providing world context and entity identity.</param>
    /// <param name="newHealthUnits">New hitpoints value (pre-clamp).</param>
    /// <returns><c>true</c> if the value changed; otherwise <c>false</c>.</returns>
    public static bool SetHealthUnits(this in EntityHandle handle, int newHealthUnits)
    {
        ref EntityStats entityStats = ref handle.Get<EntityStats>();

        int clamped = Math.Clamp(newHealthUnits, 0, entityStats.MaxHealthUnits);
        if (entityStats.Hitpoints == clamped)
        {
            return false;
        }

        entityStats.Hitpoints = clamped;
        MarkHudVisualDirty(in handle);
        return true;
    }

    /// <summary>
    ///     Adds a delta to the entity's current hitpoints (negative = damage, positive = heal),
    ///     clamps, and marks the HUD visuals as dirty when the value changes.
    /// </summary>
    /// <param name="handle">Handle providing world context and entity identity.</param>
    /// <param name="deltaHealthUnits">Delta to apply to current hitpoints.</param>
    /// <returns><c>true</c> if the value changed; otherwise <c>false</c>.</returns>
    public static bool AddHealthUnits(this in EntityHandle handle, int deltaHealthUnits)
    {
        ref int healthUnits = ref handle.Get<EntityStats>().Hitpoints;
        return handle.SetHealthUnits(healthUnits + deltaHealthUnits);
    }

    /// <summary>
    ///     Applies damage in health units (always reduces hitpoints).
    /// </summary>
    /// <param name="handle">Handle providing world context and entity identity.</param>
    /// <param name="healthUnits">Damage amount (negative values are treated as 0).</param>
    /// <returns><c>true</c> if the value changed; otherwise <c>false</c>.</returns>
    public static bool AddDamage(this in EntityHandle handle, int healthUnits)
    {
        int dmg = Math.Max(0, healthUnits);
        return handle.AddHealthUnits(-dmg);
    }

    /// <summary>
    ///     Applies healing in health units (always increases hitpoints).
    /// </summary>
    /// <param name="handle">Handle providing world context and entity identity.</param>
    /// <param name="healthUnits">Heal amount (negative values are treated as 0).</param>
    /// <returns><c>true</c> if the value changed; otherwise <c>false</c>.</returns>
    public static bool AddHealth(this in EntityHandle handle, int healthUnits)
    {
        int heal = Math.Max(0, healthUnits);
        return handle.AddHealthUnits(heal);
    }

    /// <summary>
    ///     Sets the maximum hitpoints and optionally clamps current hitpoints to the new maximum.
    ///     Marks both HUD layout and visuals as dirty when relevant values change.
    /// </summary>
    /// <param name="handle">Handle providing world context and entity identity.</param>
    /// <param name="newMaxHealthUnits">New maximum hitpoints value.</param>
    /// <param name="clampCurrent">If <c>true</c>, clamps current hitpoints to the new maximum.</param>
    /// <returns><c>true</c> if max hitpoints (or clamped current hitpoints) changed; otherwise <c>false</c>.</returns>
    public static bool SetMaxHealthUnits(this in EntityHandle handle, int newMaxHealthUnits, bool clampCurrent = true)
    {
        ref EntityStats entityStats = ref handle.Get<EntityStats>();

        int max = Math.Max(0, newMaxHealthUnits);
        bool changed = entityStats.MaxHealthUnits != max;

        entityStats.MaxHealthUnits = max;

        if (clampCurrent)
        {
            int clampedCurrent = Math.Clamp(entityStats.Hitpoints, 0, entityStats.MaxHealthUnits);
            if (clampedCurrent != entityStats.Hitpoints)
            {
                entityStats.Hitpoints = clampedCurrent;
                changed = true;
            }
        }

        if (changed)
        {
            MarkHudLayoutDirty(in handle);
            MarkHudVisualDirty(in handle);
        }

        return changed;
    }

    /// <summary>
    ///     Marks the HUD layout (slot count / bindings) as dirty for the entity addressed by this handle.
    /// </summary>
    /// <param name="handle">Handle identifying the entity to mark.</param>
    private static void MarkHudLayoutDirty(in EntityHandle handle)
    {
        handle.Ensure<HealthHudLayoutDirty>();
    }

    /// <summary>
    ///     Marks the HUD visuals (full/half/empty frames) as dirty for the entity addressed by this handle.
    /// </summary>
    /// <param name="handle">Handle identifying the entity to mark.</param>
    private static void MarkHudVisualDirty(in EntityHandle handle)
    {
        handle.Ensure<HealthHudVisualDirty>();
    }
}