using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using StbImageSharp;

namespace engine.TextureProcessing;

public sealed class Texture2D : IDisposable
{
    public int Handle { get; }
    public int Width { get; }
    public int Height { get; }

    public Vector2i Size => new(this.Width, this.Height);

    /// <summary>
    ///     Initializes a texture from an image file.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    /// <param name="generateMipmaps">Whether mipmaps should be generated.</param>
    public Texture2D(string imagePath, bool generateMipmaps = true)
    {
        this.Handle = GL.GenTexture();
        this.BindAndConfigure(generateMipmaps);

        StbImage.stbi_set_flip_vertically_on_load(1);

        using var stream = File.OpenRead(imagePath);
        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        this.Width = image.Width;
        this.Height = image.Height;

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            this.Width,
            this.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            image.Data);

        if (generateMipmaps)
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
    }

    /// <summary>
    ///     Initializes a texture from raw RGBA8 pixel data.
    /// </summary>
    /// <param name="rgbaPixels">The raw RGBA8 pixel buffer.</param>
    /// <param name="width">The texture width in pixels.</param>
    /// <param name="height">The texture height in pixels.</param>
    /// <param name="generateMipmaps">Whether mipmaps should be generated.</param>
    public Texture2D(byte[] rgbaPixels, int width, int height, bool generateMipmaps = false)
    {
        ArgumentNullException.ThrowIfNull(rgbaPixels);

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        }

        int expectedLength = width * height * 4;
        if (rgbaPixels.Length != expectedLength)
        {
            throw new ArgumentException(
                $"Expected {expectedLength} RGBA bytes but received {rgbaPixels.Length}.",
                nameof(rgbaPixels));
        }

        this.Handle = GL.GenTexture();
        this.BindAndConfigure(generateMipmaps);

        this.Width = width;
        this.Height = height;

        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            this.Width,
            this.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            rgbaPixels);

        if (generateMipmaps)
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
    }

    public void Dispose()
    {
        GL.DeleteTexture(this.Handle);
    }

    /// <summary>
    ///     Binds the texture and applies the default texture parameters.
    /// </summary>
    /// <param name="generateMipmaps">Whether mipmaps are expected for minification.</param>
    private void BindAndConfigure(bool generateMipmaps)
    {
        GL.BindTexture(TextureTarget.Texture2D, this.Handle);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)(generateMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Nearest));
    }
}