using OpenTK.Mathematics;

namespace unnamed.Utils;

public static class ColorUtils
{
    /// <summary>
    ///     Creates a slightly varied color based on the provided base color.
    ///     The variation combines a shared offset for overall brightness and
    ///     per-channel offsets for subtle hue changes.
    /// </summary>
    /// <param name="baseColor">The source color.</param>
    /// <param name="strength">The maximum total variation applied to the color channels.</param>
    /// <param name="centerBias">
    ///     Controls how strongly random values cluster around the original color.
    ///     A value of 1.0f is uniform, values greater than 1.0f produce subtler variations,
    ///     and values below 1.0f create more extreme variation.
    /// </param>
    /// <param name="sharedAmount">
    ///     Defines how much of the variation is shared across all RGB channels.
    ///     A value of 0.0f produces only hue variation, while 1.0f produces only brightness variation.
    /// </param>
    /// <returns>A new color with slight randomized brightness and hue variation.</returns>
    public static Color4 CreateSlightColorVariation(
        Color4 baseColor,
        float strength = 0.08f,
        float centerBias = 2.0f,
        float sharedAmount = 0.35f)
    {
        strength = Math.Clamp(strength, 0f, 1f);
        centerBias = Math.Max(0.01f, centerBias);
        sharedAmount = Math.Clamp(sharedAmount, 0f, 1f);

        var sharedOffset = NextSignedBiasedValue(centerBias) * strength * sharedAmount;
        var channelStrength = strength * (1f - sharedAmount);

        return new Color4(
            Math.Clamp(baseColor.R + sharedOffset + NextSignedBiasedValue(centerBias) * channelStrength, 0f, 1f),
            Math.Clamp(baseColor.G + sharedOffset + NextSignedBiasedValue(centerBias) * channelStrength, 0f, 1f),
            Math.Clamp(baseColor.B + sharedOffset + NextSignedBiasedValue(centerBias) * channelStrength, 0f, 1f),
            baseColor.A);
    }

    /// <summary>
    ///     Returns a random value in the range [-1, 1] with a controllable bias toward zero.
    /// </summary>
    /// <param name="centerBias">
    ///     The bias factor. A value of 1.0f is uniform, values greater than 1.0f
    ///     cluster more strongly around zero.
    /// </param>
    /// <returns>A signed biased random value between -1.0f and 1.0f.</returns>
    private static float NextSignedBiasedValue(float centerBias)
    {
        var magnitude = MathF.Pow((float)Random.Shared.NextDouble(), centerBias);
        var sign = Random.Shared.Next(2) == 0 ? -1f : 1f;

        return magnitude * sign;
    }
}