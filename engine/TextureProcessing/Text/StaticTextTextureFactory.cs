using System.Numerics;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace engine.TextureProcessing.Text;

public sealed class StaticTextTextureFactory
{
    private readonly Font font;
    private readonly int padding;

    /// <summary>
    ///     Initializes a factory that can rasterize static text into OpenGL textures.
    /// </summary>
    /// <param name="fontPath">The filesystem path to the font file.</param>
    /// <param name="fontSize">The font size in points.</param>
    /// <param name="padding">Extra padding around the rendered glyph bounds.</param>
    public StaticTextTextureFactory(string fontPath, float fontSize, int padding = 2)
    {
        FontCollection fontCollection = new();
        FontFamily fontFamily = fontCollection.Add(fontPath);

        this.font = fontFamily.CreateFont(fontSize, FontStyle.Regular);
        this.padding = padding;
    }

    /// <summary>
    ///     Rasterizes the given text into a texture.
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <param name="color">The text color.</param>
    /// <param name="textAlignment">
    ///     The alignment inside the generated text box.
    ///     Use <see cref="TextAlignment.Center" /> for centered multi-line text.
    /// </param>
    /// <returns>A texture containing the rasterized text.</returns>
    public Texture2D CreateTexture(
        string text,
        Color color,
        TextAlignment textAlignment = TextAlignment.Start)
    {
        ArgumentNullException.ThrowIfNull(text);

        RichTextOptions drawOptions = this.CreateDrawOptions(text, textAlignment);

        FontRectangle measuredSize = TextMeasurer.MeasureSize(text, drawOptions);
        FontRectangle measuredBounds = TextMeasurer.MeasureBounds(text, drawOptions);

        int textureWidth = Math.Max(1, (int)MathF.Ceiling(measuredSize.Width) + (this.padding * 2));
        int textureHeight = Math.Max(1, (int)MathF.Ceiling(measuredSize.Height) + (this.padding * 2));

        drawOptions.Origin = new Vector2(
            this.padding - measuredBounds.X,
            this.padding - measuredBounds.Y);

        using Image<Rgba32> image = new(textureWidth, textureHeight);

        image.Mutate(context =>
        {
            context.Clear(Color.Transparent);
            context.DrawText(drawOptions, text, color);
            context.Flip(FlipMode.Vertical);
        });

        byte[] pixels = new byte[textureWidth * textureHeight * 4];
        image.CopyPixelDataTo(pixels);

        return new Texture2D(pixels, textureWidth, textureHeight);
    }

    /// <summary>
    ///     Creates the final text drawing options used for both measurement and rendering.
    /// </summary>
    /// <param name="text">The text that will be rendered.</param>
    /// <param name="textAlignment">The desired alignment inside the text box.</param>
    /// <returns>The configured text options.</returns>
    private RichTextOptions CreateDrawOptions(string text, TextAlignment textAlignment)
    {
        RichTextOptions options = new(this.font) { TextAlignment = textAlignment };

        if (textAlignment == TextAlignment.Start)
        {
            return options;
        }

        float longestLineWidth = this.MeasureLongestLineWidth(text);
        options.WrappingLength = longestLineWidth;

        return options;
    }

    /// <summary>
    ///     Measures the width of the longest explicit line in the provided text.
    /// </summary>
    /// <param name="text">The source text that may contain line breaks.</param>
    /// <returns>The width in pixels of the longest line.</returns>
    private float MeasureLongestLineWidth(string text)
    {
        string[] lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        TextOptions measureOptions = new(this.font);

        float maxWidth = 1f;
        
        foreach (string line in lines)
        {
            FontRectangle lineSize = TextMeasurer.MeasureSize(line, measureOptions);
            maxWidth = Math.Max(maxWidth, lineSize.Width);
        }

        return MathF.Ceiling(maxWidth);
    }
}