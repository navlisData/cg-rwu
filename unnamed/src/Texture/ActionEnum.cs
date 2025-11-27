namespace unnamed.Texture;

/// <summary>
/// Provides validated conversions between numeric action indices and enum values.
/// </summary>
internal static class ActionEnum
{
    /// <summary>
    /// Converts a byte to an enum value and validates that it is defined.
    /// </summary>
    /// <typeparam name="TAction">The enum type.</typeparam>
    /// <param name="index">The numeric enum value.</param>
    /// <returns>The corresponding enum value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is not defined for the enum.</exception>
    public static TAction FromIndex<TAction>(byte index) where TAction : struct, Enum
    {
        if (!Enum.IsDefined(typeof(TAction), index))
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Undefined {typeof(TAction).Name} value.");

        return (TAction)Enum.ToObject(typeof(TAction), index);
    }
}