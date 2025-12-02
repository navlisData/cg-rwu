using System.Diagnostics;
using System.Drawing;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace unnamed.Utils;

public static class GraphicsUtils
{
    public static readonly uint[] QuadIndices = [0, 1, 2, 2, 1, 3];

    /// <summary>
    ///     Builds an interleaved quad for a sprite on a texture atlas.
    ///     Assumes the texture was loaded with vertical flip enabled (v=0 at bottom).
    /// </summary>
    /// <param name="objectSize">Requested quad size in object/world units.</param>
    /// <param name="spriteRect">Sprite bounds in pixels (top-left origin).</param>
    /// <param name="texture">Atlas texture (for normalization).</param>
    /// <param name="vertices">Target array; must have length >= 16.</param>
    /// <param name="horizontallyCentered">If true, x is centered around 0.</param>
    /// <param name="verticallyCentered">If true, y is centered around 0.</param>
    public static void FillSpriteQuadGeometry(
        in Vector2 objectSize,
        in RectangleF spriteRect,
        in Texture2D texture,
        in float[] vertices,
        bool horizontallyCentered,
        bool verticallyCentered)
    {
        Vector2 size = ComputeSpriteWorldSize(objectSize, spriteRect);
        (float x0, float x1, float y0, float y1) =
            ComputeQuadBounds(size, horizontallyCentered, verticallyCentered);

        float invW = 1f / texture.Width;
        float invH = 1f / texture.Height;

        float u0 = spriteRect.Left * invW;
        float u1 = spriteRect.Right * invW;

        float vTop = 1f - (spriteRect.Top * invH);
        float vBottom = 1f - (spriteRect.Bottom * invH);

        FillQuadInterleavedXYUV(vertices, x0, x1, y0, y1, u0, u1, vBottom, vTop);
    }

    /// <summary>
    ///     Fills an interleaved position/uv quad for solid-color rendering.
    ///     UVs are set to the full [0..1] range.
    /// </summary>
    /// <param name="size">Quad size in object/world units.</param>
    /// <param name="vertices">Target array; must have length >= 16.</param>
    /// <param name="horizontallyCentered">If true, x is centered around 0.</param>
    /// <param name="verticallyCentered">If true, y is centered around 0.</param>
    public static void FillSolidQuadGeometry(
        in Vector2 size,
        float[] vertices,
        bool horizontallyCentered,
        bool verticallyCentered)
    {
        (float x0, float x1, float y0, float y1) = ComputeQuadBounds(size, horizontallyCentered, verticallyCentered);
        FillQuadInterleavedXYUV(vertices, x0, x1, y0, y1, u0: 0f, u1: 1f, v0: 0f, v1: 1f);
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

    /// <summary>
    ///     Writes an interleaved quad into the provided vertex array.
    ///     Vertex layout: (x, y, u, v) for 4 vertices (total 16 floats).
    /// </summary>
    /// <param name="vertices">Target array; must have length >= 16.</param>
    /// <param name="x0">Left x in object/world space.</param>
    /// <param name="x1">Right x in object/world space.</param>
    /// <param name="y0">Bottom y in object/world space.</param>
    /// <param name="y1">Top y in object/world space.</param>
    /// <param name="u0">Left u texture coordinate.</param>
    /// <param name="u1">Right u texture coordinate.</param>
    /// <param name="v0">Bottom v texture coordinate.</param>
    /// <param name="v1">Top v texture coordinate.</param>
    private static void FillQuadInterleavedXYUV(
        float[] vertices,
        float x0, float x1,
        float y0, float y1,
        float u0, float u1,
        float v0, float v1)
    {
        if (vertices.Length < 16)
        {
            throw new ArgumentException("vertices length must be >= 16");
        }

        // Bottom-Left
        vertices[0] = x0;
        vertices[1] = y0;
        vertices[2] = u0;
        vertices[3] = v0;
        // Bottom-Right
        vertices[4] = x1;
        vertices[5] = y0;
        vertices[6] = u1;
        vertices[7] = v0;
        // Top-Left
        vertices[8] = x0;
        vertices[9] = y1;
        vertices[10] = u0;
        vertices[11] = v1;
        // Top-Right
        vertices[12] = x1;
        vertices[13] = y1;
        vertices[14] = u1;
        vertices[15] = v1;
    }

    /// <summary>
    ///     Computes quad bounds in object/world space based on size and centering flags.
    /// </summary>
    /// <param name="size">Quad size in object/world units.</param>
    /// <param name="horizontallyCentered">If true, x is centered around 0.</param>
    /// <param name="verticallyCentered">If true, y is centered around 0.</param>
    /// <returns>Tuple of (x0, x1, y0, y1).</returns>
    private static (float x0, float x1, float y0, float y1) ComputeQuadBounds(
        in Vector2 size,
        bool horizontallyCentered,
        bool verticallyCentered)
    {
        float x0 = horizontallyCentered ? -0.5f * size.X : 0f;
        float x1 = horizontallyCentered ? 0.5f * size.X : size.X;
        float y0 = verticallyCentered ? -0.5f * size.Y : 0f;
        float y1 = verticallyCentered ? 0.5f * size.Y : size.Y;

        return (x0, x1, y0, y1);
    }

    /// <summary>
    ///     Uploads interleaved quad vertex data into the given VBO.
    ///     Expects VAO with vertex attributes already configured.
    /// </summary>
    /// <param name="vertexScratch">Interleaved vertex data (length >= 16).</param>
    public static void UploadQuadVertices(in float[] vertexScratch)
    {
        int sizeInBytes = vertexScratch.Length * sizeof(float);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeInBytes, vertexScratch, BufferUsageHint.StreamDraw);
    }

    /// <summary>
    ///     Sets the uniform color and draws the currently bound quad (VAO/EBO must be bound).
    /// </summary>
    /// <param name="colorLocation">Uniform location for uColor.</param>
    /// <param name="color">RGBA color.</param>
    public static void DrawColoredQuad(int colorLocation, in Vector4 color)
    {
        GL.Uniform4(colorLocation, color);
        GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, 0);
    }

    /// <summary>
    ///     Computes the final world-space size of a sprite quad using the same aspect logic as FillSpriteQuadGeometry.
    /// </summary>
    /// <param name="objectSize">Requested object size in world units.</param>
    /// <param name="spriteRect">Sprite bounds in pixels.</param>
    /// <returns>World-space quad size with aspect correction applied.</returns>
    public static Vector2 ComputeSpriteWorldSize(in Vector2 objectSize, in RectangleF spriteRect)
    {
        Debug.Assert(spriteRect.Height > 0f);

        float aspect = spriteRect.Width / spriteRect.Height;
        return new Vector2(objectSize.Y * aspect, objectSize.Y);
    }
}