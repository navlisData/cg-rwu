namespace unnamed.Components.UI;

public struct ScoreDigit
{
    public ScoreDigit(int value)
    {
        if (value < 0 || value > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Digit value must be between 0 and 9.");
        }

        this.Value = value;
    }

    public int Value { get; }
}