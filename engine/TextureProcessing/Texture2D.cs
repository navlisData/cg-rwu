using OpenTK.Graphics.OpenGL4;

using StbImageSharp;

namespace engine.TextureProcessing;

public sealed class Texture2D : IDisposable
{
    public int Handle { get; }
    public int Width { get; }
    public int Height { get; }

    public Texture2D(String imagePath, bool generateMipmaps = true)
    {
        this.Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, this.Handle);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)(generateMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Nearest));
        
        StbImage.stbi_set_flip_vertically_on_load(1);
        using var stream = File.OpenRead(imagePath);
        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        
        this.Width = image.Width;
        this.Height = image.Height;
        
        GL.TexImage2D(
            TextureTarget.Texture2D, 0,  PixelInternalFormat.Rgba,
            this.Width, this.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data
        );
        
        if (generateMipmaps) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void Dispose()
    {
        GL.DeleteTexture(this.Handle);
    }
}