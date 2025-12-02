using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;

namespace unnamed.Rendering;

public sealed class PulseAnimationSystem(World world) : EntitySetSystem<float>(world, world.Query()
    .With<PulseAnimation>()
    .With<Sprite>()
    .With<Transform>()
    .Build()
)
{
    /// <summary>
    ///     Applies a sine-based pulse to Transform.Size while keeping Transform.Scale unchanged.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    /// <param name="e">Entity to update.</param>
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Transform transform = ref handle.Get<Transform>();
        ref PulseAnimation pulse = ref handle.Get<PulseAnimation>();

        pulse.TimeSeconds += dt;

        float multiplier = ComputeMultiplier(
            pulse.TimeSeconds,
            pulse.FrequencyHz,
            pulse.PhaseRadians,
            pulse.MultiplierMean,
            pulse.MultiplierAmplitude
        );

        transform.Size = pulse.BaseSize * multiplier;
    }

    /// <summary>
    ///     Computes a multiplier using mean + amplitude * sin(...) and clamps it to remain positive.
    /// </summary>
    /// <param name="timeSeconds">Elapsed time in seconds.</param>
    /// <param name="frequencyHz">Pulse frequency in Hertz.</param>
    /// <param name="phaseRadians">Phase offset in radians.</param>
    /// <param name="meanMultiplier">Mean multiplier value.</param>
    /// <param name="amplitudeMultiplier">Amplitude around the mean.</param>
    /// <returns>A strictly positive multiplier suitable for scaling a size.</returns>
    private static float ComputeMultiplier(
        float timeSeconds,
        float frequencyHz,
        float phaseRadians,
        float meanMultiplier,
        float amplitudeMultiplier)
    {
        const float twoPi = 2f * MathF.PI;

        float clampedAmplitude = Clamp(amplitudeMultiplier, 0f, 0.95f);
        float safeMean = meanMultiplier <= 0f ? 1f : meanMultiplier;

        float angularFrequency = twoPi * MathF.Max(0f, frequencyHz);
        float sine = MathF.Sin(timeSeconds * angularFrequency + phaseRadians);

        return Clamp(safeMean + clampedAmplitude * sine, 0.001f, 1000f);
    }

    /// <summary>
    ///     Clamps a float value between the given bounds.
    /// </summary>
    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}