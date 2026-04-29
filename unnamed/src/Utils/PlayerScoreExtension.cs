using Engine.Ecs;

using engine.Ecs.State;

using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Resources;

namespace unnamed.Utils;

public static class PlayerScoreExtension
{
    public static void SetPlayerScore(this World world, in Entity player, int scorePoints)
    {
        ref PlayerScore score = ref world.GetResource<PlayerScore>();
        score.Value = scorePoints;
        world.Ensure<ScoreLayoutDirty>(player);

        if (score.Value <= 0)
        {
            ref State<GameState> state = ref world.GetState<GameState>();
            state.QueueChange(GameState.Lost);
        }
    }

    public static void DecreasePlayerScore(this World world, in Entity player, int score)
    {
        int newScore = Math.Max(0, world.GetResource<PlayerScore>().Value - score);
        SetPlayerScore(world, player, newScore);
    }

    public static void IncreasePlayerScore(this World world, in Entity player, int score)
    {
        int newScore = world.GetResource<PlayerScore>().Value + score;
        SetPlayerScore(world, player, newScore);
    }
}