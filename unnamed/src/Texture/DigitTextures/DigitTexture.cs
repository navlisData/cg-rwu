using engine.TextureProcessing;

namespace unnamed.Texture.DigitTextures;

public readonly struct DigitTexture
{
    /// <summary>
    ///     Initializes digit texture data with fixed-cell layout metrics.
    /// </summary>
    /// <param name="texture">The concrete texture of the digit.</param>
    /// <param name="paddingLeft">Horizontal padding on the left side inside the fixed cell.</param>
    /// <param name="paddingRight">Horizontal padding on the right side inside the fixed cell.</param>
    /// <param name="paddingTop">Vertical padding on the top side inside the fixed cell.</param>
    /// <param name="paddingBottom">Vertical padding on the bottom side inside the fixed cell.</param>
    /// <param name="cellWidth">Fixed cell width shared by all digit textures.</param>
    /// <param name="cellHeight">Fixed cell height shared by all digit textures.</param>
    public DigitTexture(
        Texture2D texture,
        int paddingLeft,
        int paddingRight,
        int paddingTop,
        int paddingBottom,
        int cellWidth,
        int cellHeight)
    {
        if (cellWidth < texture.Width)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cellWidth),
                "Cell width must be greater than or equal to texture width.");
        }

        if (cellHeight < texture.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cellHeight),
                "Cell height must be greater than or equal to texture height.");
        }

        this.Texture = texture;
        this.PaddingLeft = paddingLeft;
        this.PaddingRight = paddingRight;
        this.PaddingTop = paddingTop;
        this.PaddingBottom = paddingBottom;
        this.CellWidth = cellWidth;
        this.CellHeight = cellHeight;
    }

    public Texture2D Texture { get; }

    public int PaddingLeft { get; }

    public int PaddingRight { get; }

    public int PaddingTop { get; }

    public int PaddingBottom { get; }

    public int CellWidth { get; }

    public int CellHeight { get; }
}