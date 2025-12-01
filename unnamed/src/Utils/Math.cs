namespace unnamed.Utils;

/// <summary>
///     Provides math-related helper functions.
/// </summary>
public static class MathUtils
{
    /// <summary>
    ///     Wraps a numeric value into the range <c>[0, max)</c> using positive modulo.
    ///     Unlike the standard remainder operator, this guarantees a non-negative result
    ///     even when <paramref name="value" /> is negative.
    /// </summary>
    /// <param name="value">The input value to wrap.</param>
    /// <param name="max">The upper bound of the wrapping range.</param>
    /// <returns>
    ///     A value in the interval <c>[0, max)</c> representing <paramref name="value" /> modulo <paramref name="max" />,
    ///     always non-negative.
    /// </returns>
    public static float Wrap(float value, float max)
    {
        return ((value % max) + max) % max;
    }
}