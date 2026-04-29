namespace unnamed.Resources;

public struct PlayerScore(int score)
{
    public int Value = score;

    public static implicit operator int(PlayerScore score)
    {
        return score.Value;
    }
}