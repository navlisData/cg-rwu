using engine.TextureProcessing;
using engine.TextureProcessing.Text;

using SixLabors.ImageSharp;

namespace unnamed.Texture.DigitTextures;

public readonly struct DigitTextureDatabase
{
    private const int DigitCount = 10;
    private readonly DigitTexture[] texturesByDigit = new DigitTexture[DigitCount];

    /// <summary>
    ///     Creates precomputed digit textures and fixed-cell layout metrics for digits 0 to 9.
    /// </summary>
    /// <param name="textFactory">Factory used to rasterize each digit into a texture.</param>
    public DigitTextureDatabase(StaticTextTextureFactory textFactory)
    {
        Texture2D[] generatedTextures = new Texture2D[DigitCount];

        int maxWidth = 0;
        int maxHeight = 0;

        for (int digit = 0; digit < DigitCount; digit++)
        {
            Texture2D texture = textFactory.CreateTexture(
                digit.ToString(),
                Color.White);

            generatedTextures[digit] = texture;

            maxWidth = Math.Max(maxWidth, texture.Width);
            maxHeight = Math.Max(maxHeight, texture.Height);
        }

        for (int digit = 0; digit < DigitCount; digit++)
        {
            Texture2D texture = generatedTextures[digit];

            int missingWidth = maxWidth - texture.Width;
            int paddingLeft = missingWidth / 2;
            int paddingRight = missingWidth - paddingLeft;

            int missingHeight = maxHeight - texture.Height;
            int paddingTop = missingHeight / 2;
            int paddingBottom = missingHeight - paddingTop;

            this.texturesByDigit[digit] = new DigitTexture(
                texture,
                paddingLeft,
                paddingRight,
                paddingTop,
                paddingBottom,
                maxWidth,
                maxHeight);
        }
    }

    public DigitTexture GetDigitTexture(int digit)
    {
        if (digit < 0 || digit >= DigitCount)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), $"Digit must be between 0 and {DigitCount - 1}.");
        }

        return this.texturesByDigit[digit];
    }
}