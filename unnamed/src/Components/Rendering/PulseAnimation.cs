using OpenTK.Mathematics;

using unnamed.Components.Physics;

namespace unnamed.Components.Rendering;

public struct PulseAnimation
{
    public float FrequencyHz;
    public float PhaseRadians;
    public float TimeSeconds;

    public float MultiplierMean;
    public float MultiplierAmplitude;

    public Vector2 BaseSize;

    /// <summary>
    ///     Configures the pulse so that the effective visible size oscillates between the requested
    ///     <paramref name="minScale"/> and <paramref name="maxScale"/> factors, while keeping
    ///     <paramref name="transform"/>.Scale unchanged.
    /// </summary>
    /// <param name="transform">The current Transform used as baseline for both Scale and Size.</param>
    /// <param name="minScale">
    ///     Desired minimum visible scale factor relative to the unpulsed baseline. Must be greater than zero.
    /// </param>
    /// <param name="maxScale">
    ///     Desired maximum visible scale factor relative to the unpulsed baseline. Must be greater than <paramref name="minScale"/>.
    /// </param>
    /// <param name="frequency">Pulse frequency in Hertz. Values less than zero are treated as zero by the system.</param>
    public PulseAnimation(Transform transform, float minScale, float maxScale, float frequency)
    {
        float minMultiplier = minScale / transform.Scale;
        float maxMultiplier = maxScale / transform.Scale;

        MultiplierMean = (minMultiplier + maxMultiplier) * 0.5f;
        MultiplierAmplitude = (maxMultiplier - minMultiplier) * 0.5f;
        FrequencyHz = frequency;
        BaseSize = transform.Size;
    }

    /// <summary>
    ///     Configures a symmetric pulse around the current size (mean multiplier = 1).
    ///     Example: amplitude = 0.2 produces a multiplier range of [0.8 .. 1.2].
    /// </summary>
    /// <param name="amplitude">
    ///     Relative multiplier amplitude around the mean. Recommended range: 0..0.5.
    ///     Values are clamped by the system to ensure the multiplier remains positive.
    /// </param>
    /// <param name="transform">The current Transform used as baseline for the pulsed size.</param>
    /// <param name="frequency">Pulse frequency in Hertz. Values less than zero are treated as zero by the system.</param>
    public PulseAnimation(float amplitude, Transform transform, float frequency)
    {
        MultiplierMean = 1f;
        MultiplierAmplitude = amplitude;
        FrequencyHz = frequency;
        BaseSize = transform.Size;
    }
}