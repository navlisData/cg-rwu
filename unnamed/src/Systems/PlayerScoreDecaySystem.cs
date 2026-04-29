using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Tags;
using unnamed.Components.UI;
using unnamed.Resources;
using unnamed.Utils;

namespace unnamed.systems;

public sealed class PlayerScoreDecaySystem()
    : BaseSystem
{
    private static readonly Query PlayerQuery = new QueryBuilder().With<Player>().With<ScoreDigits>().Build();

    private const float BaseDecayTickIntervalSeconds = 3f;
    private const float MinimumDecayTickIntervalSeconds = 1.2f;
    private const float DecayTickIntervalFactor = 0.80f;

    public override void Run(World world)
    {
        int level = world.GetResource<Level>().Value;
        float tickIntervalSeconds = CalculateDecayTickIntervalSeconds(level);

        if (!PlayerQuery.TrySingle(world, out Entity player))
        {
            return;
        }

        ref DeltaTime dt = ref world.GetResource<DeltaTime>();
        ref PlayerScoreDecayTimer decayTimer = ref world.GetResource<PlayerScoreDecayTimer>();

        decayTimer.ElapsedSeconds += dt;

        if (decayTimer.ElapsedSeconds < tickIntervalSeconds)
        {
            return;
        }

        int elapsedTicks = (int)(decayTimer.ElapsedSeconds / tickIntervalSeconds);
        decayTimer.ElapsedSeconds -= elapsedTicks * tickIntervalSeconds;
        world.DecreasePlayerScore(player, (elapsedTicks * PlayerScoreConstants.ScoreLossPerTick));
    }

    private static float CalculateDecayTickIntervalSeconds(int level)
    {
        int normalizedLevel = Math.Max(1, level);
        int levelSteps = normalizedLevel - 1;

        float scaledInterval = MinimumDecayTickIntervalSeconds
                               + ((BaseDecayTickIntervalSeconds - MinimumDecayTickIntervalSeconds)
                                  * MathF.Pow(DecayTickIntervalFactor, levelSteps));

        return MathF.Max(MinimumDecayTickIntervalSeconds, scaledInterval);
    }
}