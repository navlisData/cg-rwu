using System.Drawing;

using engine.TextureProcessing;

namespace unnamed.Utils;

public static class GraphicsUtils
{
    public static readonly uint[] QuadIndices = [0, 1, 2, 2, 1, 3];

    /// <summary>
    /// Builds quad vertices and indices for a sprite on a texture atlas.
    /// Assumes the texture was loaded with vertical flip enabled (v=0 at bottom).
    /// </summary>
    /// <param name="objectX">Quad width in object space.</param>
    /// <param name="objectY">Quad height in object space.</param>
    /// <param name="spriteRect">Sprite bounds in pixels (top-left origin).</param>
    /// <param name="texture">Atlas texture (for normalization).</param>
    /// <returns>Quad geometry with interleaved vertices and indices.</returns>
    public static void FillSpriteQuadGeometry(
        float objectX, float objectY,
        RectangleF spriteRect, Texture2D texture,
        Span<float> vertices)
    {
        if (vertices.Length < 16) throw new ArgumentException("vertices length must be >= 16");
        
        // Normalize helpers
        float invW = 1f / texture.Width;
        float invH = 1f / texture.Height;

        // Horizontal UVs (left/right)
        float u0 = spriteRect.Left * invW;
        float u1 = spriteRect.Right * invW;

        // With stb flip ON (stbi_set_flip_vertically_on_load(1)):
        // RectangleF uses a top-left origin, but OpenGL UVs are bottom-left.
        // Conversion required: pixel Y to UV by v = 1 - (y / height).
        float vTop = 1f - (spriteRect.Top * invH);
        float vBottom = 1f - (spriteRect.Bottom * invH);
        
        // Interleaved vertex buffer: position(x,y), texcoord(u,v)
        // Bottom-Left
        vertices[0] = 0f;
        vertices[1] = 0f;
        vertices[2] = u0;
        vertices[3] = vBottom;
  
        // Bottom-Right
        vertices[4] = objectX;
        vertices[5] = 0f;
        vertices[6] = u1;
        vertices[7] = vBottom;

        // Top-Left
        vertices[8] = 0f;
        vertices[9] = objectY;
        vertices[10] = u0;
        vertices[11] = vTop;

        // Top-Right
        vertices[12] = objectX;
        vertices[13] = objectY;
        vertices[14] = u1;
        vertices[15] = vTop;
    }
}