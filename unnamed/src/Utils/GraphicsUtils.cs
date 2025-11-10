using System.Drawing;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace unnamed.Utils;

public static class GraphicsUtils
{
    public static readonly uint[] QuadIndices = [0, 1, 2, 2, 1, 3];
    public static readonly uint[] QuadIndices2 = [0, 1, 2, 2, 3, 0];

    /// <summary>
    ///     Builds quad vertices and indices for a sprite on a texture atlas.
    ///     Assumes the texture was loaded with vertical flip enabled (v=0 at bottom).
    /// </summary>
    /// <param name="objectSize">Quad dimensions in object space.</param>
    /// <param name="spriteRect">Sprite bounds in pixels (top-left origin).</param>
    /// <param name="texture">Atlas texture (for normalization).</param>
    /// <param name="vertices">Vertex array: required to have a length >= 16</param>
    /// <param name="horizontallyCentered">The x-axis is centered around 0</param>
    /// <param name="verticallyCentered">The y-axis is centered around 0</param>
    /// <returns>Quad geometry with interleaved vertices and indices.</returns>
    public static void FillSpriteQuadGeometry(
        in Vector2 objectSize,
        in RectangleF spriteRect, in Texture2D texture,
        in float[] vertices, bool horizontallyCentered, bool verticallyCentered)
    {
        if (vertices.Length < 16)
        {
            throw new ArgumentException("vertices length must be >= 16");
        }

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

        float width = objectSize.X;
        float height = objectSize.Y;

        if (spriteRect.Height > 0f)
        {
            float spriteAspect = spriteRect.Width / spriteRect.Height;
            width = height * spriteAspect;
        }

        float x0 = horizontallyCentered ? -0.5f * width : 0f;
        float x1 = horizontallyCentered ? 0.5f * width : width;
        float y0 = verticallyCentered ? -0.5f * height : 0f;
        float y1 = verticallyCentered ? 0.5f * height : height;

        // Interleaved vertex buffer: position(x,y), texcoord(u,v)
        // Bottom-Left
        vertices[0] = x0;
        vertices[1] = y0;
        vertices[2] = u0;
        vertices[3] = vBottom;

        // Bottom-Right
        vertices[4] = x1;
        vertices[5] = y0;
        vertices[6] = u1;
        vertices[7] = vBottom;

        // Top-Left
        vertices[8] = x0;
        vertices[9] = y1;
        vertices[10] = u0;
        vertices[11] = vTop;

        // Top-Right
        vertices[12] = x1;
        vertices[13] = y1;
        vertices[14] = u1;
        vertices[15] = vTop;
    }

    public static void RenderSpriteQuad(int textureHandle, int mvpLocation, in float[] vertexScratch, ref Matrix4 mvp)
    {
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);
        GL.ActiveTexture(TextureUnit.Texture0);

        GL.BufferData(BufferTarget.ArrayBuffer, vertexScratch.Length * sizeof(float), vertexScratch,
            BufferUsageHint.StaticDraw);

        GL.BufferData(BufferTarget.ElementArrayBuffer, QuadIndices.Length * sizeof(uint), QuadIndices,
            BufferUsageHint.StaticDraw);

        GL.UniformMatrix4(mvpLocation, false, ref mvp);
        GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, 0);
    }
}