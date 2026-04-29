using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.Tags;
using unnamed.Components.UI;
using unnamed.Prefabs;
using unnamed.Resources;
using unnamed.Texture.DigitTextures;

namespace unnamed.systems;

public sealed class ScoreLayoutSystem : BaseSystem
{
    private const int RightOffset = 10;
    private const int TopOffset = 10;
    private const int DigitGap = -3;

    private static readonly Query PlayerQuery = new QueryBuilder()
        .With<ScoreDigits>()
        .With<ScoreLayoutDirty>()
        .With<Player>()
        .Build();

    public override void Run(World world)
    {
        if (!PlayerQuery.TrySingle(world, out Entity player))
        {
            return;
        }

        ref DigitTextureDatabase digitTextures = ref world.GetResource<DigitTextureDatabase>();
        ref ScoreDigits scoreDigits = ref world.Get<ScoreDigits>(player);

        int playerScore = Math.Max(0, world.GetResource<PlayerScore>().Value);
        int requiredDigitCount = CountDigits(playerScore);

        EnsureExactDigitCount(ref scoreDigits, requiredDigitCount, world, ref digitTextures);
        UpdateDigitValuesAndOffsets(ref scoreDigits, playerScore, requiredDigitCount, world, ref digitTextures);

        world.Remove<ScoreLayoutDirty>(player);
        world.Ensure<ScoreVisualDirty>(player);
    }

    /// <summary>
    ///     Updates digit values, texture-dependent layout offsets, and texture-dependent reference sizes.
    /// </summary>
    /// <param name="scoreDigits">Score digit binding component.</param>
    /// <param name="score">Current non-negative score value.</param>
    /// <param name="digitCount">Amount of digits required to display the score.</param>
    /// <param name="world">World containing the digit entities.</param>
    /// <param name="digitTextures">Precomputed digit texture database.</param>
    private static void UpdateDigitValuesAndOffsets(
        ref ScoreDigits scoreDigits,
        int score,
        int digitCount,
        World world,
        ref DigitTextureDatabase digitTextures)
    {
        int divisor = GetHighestDecimalDivisor(digitCount);

        for (int slotIndex = 0; slotIndex < digitCount; slotIndex++)
        {
            int digitValue = score / divisor;
            score %= divisor;

            if (divisor > 1)
            {
                divisor /= 10;
            }

            Entity digitEntity = ResolveOrCreateDigit(ref scoreDigits, slotIndex, world, ref digitTextures);
            EntityHandle digitHandle = world.Handle(digitEntity);

            DigitTexture digitTexture = digitTextures.GetDigitTexture(digitValue);

            digitHandle.Ensure<UiReferenceOffset>();
            digitHandle.Ensure<UiReferenceSize>();
            digitHandle.Ensure<ScoreDigit>();

            ref UiReferenceOffset referenceOffset = ref digitHandle.Get<UiReferenceOffset>();
            ref UiReferenceSize referenceSize = ref digitHandle.Get<UiReferenceSize>();
            ref ScoreDigit scoreDigit = ref digitHandle.Get<ScoreDigit>();

            int indexFromRight = digitCount - 1 - slotIndex;

            referenceOffset = CalculateDigitOffset(
                indexFromRight,
                digitTexture,
                RightOffset,
                TopOffset,
                DigitGap);

            referenceSize = new UiReferenceSize(
                digitTexture.Texture.Width,
                digitTexture.Texture.Height);

            scoreDigit = new ScoreDigit(digitValue);
        }
    }

    private static UiReferenceOffset CalculateDigitOffset(
        int indexFromRight,
        DigitTexture digitTexture,
        int rightInset,
        int topInset,
        int digitGap)
    {
        float distanceFromRight = rightInset
                                  + (indexFromRight * (digitTexture.CellWidth + digitGap))
                                  + digitTexture.PaddingRight;
        float distanceFromTop = topInset + digitTexture.PaddingTop;

        return new UiReferenceOffset(
            -distanceFromRight,
            distanceFromTop);
    }

    /// <summary>
    ///     Resolves an existing digit entity or creates a replacement if the stored reference is no longer alive.
    /// </summary>
    /// <param name="scoreDigits">Score digit binding component.</param>
    /// <param name="slotIndex">Digit slot index to resolve.</param>
    /// <param name="world">World containing the digit entities.</param>
    /// <param name="digitTextures">Precomputed digit texture database.</param>
    /// <returns>The resolved or newly created digit entity.</returns>
    private static Entity ResolveOrCreateDigit(
        ref ScoreDigits scoreDigits,
        int slotIndex,
        World world,
        ref DigitTextureDatabase digitTextures)
    {
        Entity digitEntity = scoreDigits.digits[slotIndex];
        if (world.IsAlive(digitEntity))
        {
            return digitEntity;
        }

        DigitTexture initialDigitTexture = digitTextures.GetDigitTexture(0);

        Entity replacement = PrefabFactory.CreateDigit(
            world,
            initialDigitTexture.Texture,
            0,
            new UiReferenceOffset());

        scoreDigits.digits[slotIndex] = replacement;

        return replacement;
    }

    /// <summary>
    ///     Ensures the score HUD has exactly the required amount of digit entities.
    /// </summary>
    /// <param name="scoreDigits">Score digit binding component.</param>
    /// <param name="requiredDigitCount">Required digit entity count.</param>
    /// <param name="world">World containing the digit entities.</param>
    /// <param name="digitTextures">Precomputed digit texture database.</param>
    private static void EnsureExactDigitCount(
        ref ScoreDigits scoreDigits,
        int requiredDigitCount,
        World world,
        ref DigitTextureDatabase digitTextures)
    {
        int currentDigitCount = scoreDigits.digits.Length;
        if (currentDigitCount == requiredDigitCount)
        {
            return;
        }

        // Shrink
        if (currentDigitCount > requiredDigitCount)
        {
            for (int i = currentDigitCount - 1; i >= requiredDigitCount; i--)
            {
                world.DestroyEntity(scoreDigits.digits[i]);
            }

            Array.Resize(ref scoreDigits.digits, requiredDigitCount);
            return;
        }

        // Grow
        Array.Resize(ref scoreDigits.digits, requiredDigitCount);

        DigitTexture initialDigitTexture = digitTextures.GetDigitTexture(0);
        for (int i = currentDigitCount; i < requiredDigitCount; i++)
        {
            scoreDigits.digits[i] = PrefabFactory.CreateDigit(
                world,
                initialDigitTexture.Texture,
                0,
                new UiReferenceOffset());
        }
    }

    /// <summary>
    ///     Counts the decimal digits needed to represent a positive integer.
    /// </summary>
    /// <param name="value">The positive integer value.</param>
    /// <returns>The decimal digit count.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="value"/> is negative.
    /// </exception>
    private static int CountDigits(int value)
    {
        return value > 0 ? (int)Math.Log10(value) + 1 : 1;
    }

    /// <summary>
    ///     Calculates the highest decimal divisor for extracting digits from left to right.
    /// </summary>
    /// <param name="digitCount">Amount of decimal digits.</param>
    /// <returns>The highest decimal divisor.</returns>
    private static int GetHighestDecimalDivisor(int digitCount)
    {
        return (int)Math.Pow(10, digitCount - 1);
    }
}