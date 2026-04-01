using System.Diagnostics;
using System.Drawing;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.UI;

namespace unnamed.Rendering.RenderContext;

public sealed class DrawBuilder(RenderContext renderContext) : IDrawBuilder
{
    private DrawContext drawContext;

    public IProjectionStep WithColoration(in Color4 color, float blendFactor)
    {
        Debug.Assert(blendFactor is >= 0f and <= 1f);
        GL.Uniform4(renderContext.uOverrideColor, color);
        GL.Uniform1(renderContext.uBlendFactor, blendFactor);
        return this;
    }

    public IProjectionStep WithColoration(in Vector3 color, float blendFactor)
    {
        return this.WithColoration(new Color4(color.X, color.Y, color.Z, 1f), blendFactor);
    }

    public IProjectionStep WithColoration(in Color4? color, float blendFactor)
    {
        return color is null ? this.WithoutColoration() : this.WithColoration(color.Value, blendFactor);
    }

    public IProjectionStep WithAlpha(float alpha)
    {
        return this.WithColoration(new Color4(0f, 0f, 0f, alpha), 0f);
    }

    public IProjectionStep WithoutColoration()
    {
        return this.WithColoration(new Color4(0f, 0f, 0f, 1f), 0f);
    }

    public void Draw()
    {
        GL.DrawElements(PrimitiveType.Triangles, RenderContext.QuadIndices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public IVerticesRelativeStep WithModelViewProjection(ref Matrix4 modelViewProjection)
    {
        GL.UniformMatrix4(renderContext.uModelViewProjection, false, ref modelViewProjection);
        return this;
    }

    public IVerticesRelativeStep WithPosition(in Vector2 position)
    {
        Matrix4 model = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 modelViewProjection = model * renderContext.camera.ViewProjection;
        return this.WithModelViewProjection(ref modelViewProjection);
    }

    public IVerticesRelativeStep WithPosition(in float x, in float y)
    {
        return this.WithPosition(new Vector2(x, y));
    }

    public IVerticesRelativeStep WithPositionAndDistortion(in Vector2 position, in Matrix4 distortionMatrix)
    {
        Matrix4 model = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 distortedModel = distortionMatrix * model;
        Matrix4 modelViewProjection = distortedModel * renderContext.camera.ViewProjection;
        return this.WithModelViewProjection(ref modelViewProjection);
    }

    public IVerticesRelativeStep WithPositionAndDistortion(in float x, in float y, in Matrix4 distortionMatrix)
    {
        return this.WithPositionAndDistortion(new Vector2(x, y), distortionMatrix);
    }

    public IVerticesRelativeStep WithPositionAndTransform(in Vector2 position, in Transform transform)
    {
        Matrix4 distortion =
            Matrix4.CreateRotationZ(transform.Rotation) *
            Matrix4.CreateScale(transform.Scale);

        return this.WithPositionAndDistortion(new Vector2(position.X, position.Y + transform.Height), distortion);
    }

    public IVerticesAbsoluteStep WithAbsolutePosition(in AbsolutePosition position, in AbsoluteSize size)
    {
        Matrix4 model = Matrix4.CreateScale(size.Width, size.Height, 1f) *
                        Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 modelViewProjection = model * renderContext.uiViewProjection;
        this.WithModelViewProjection(ref modelViewProjection);
        return this;
    }

    public IColorWithTextureStep WithSprite(in StaticSprite sprite)
    {
        Texture2D texture = renderContext.assetStore.GetTextureById(sprite.SpriteSheetId);
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
        GL.ActiveTexture(TextureUnit.Texture0);
        this.drawContext.SpriteSize = sprite.RectPx;
        this.drawContext.TextureSize = texture.Size;
        return this;
    }

    public IColorWithTextureStep WithText(in StaticTextTexture text)
    {
        Texture2D texture = text.Texture;
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
        GL.ActiveTexture(TextureUnit.Texture0);
        this.drawContext.SpriteSize = new Rectangle(0, 0, texture.Width, texture.Height);
        this.drawContext.TextureSize = texture.Size;
        return this;
    }

    public IColorWithoutTextureStep WithoutSprite()
    {
        this.WithSprite(renderContext.fallbackSprite);
        return this;
    }

    public IDrawStep WithVertices(in float[] vertexArray)
    {
        Debug.Assert(vertexArray.Length == RenderContext.VerticesArrayLength);
        GL.BufferData(BufferTarget.ArrayBuffer, RenderContext.VerticesLength, vertexArray,
            BufferUsageHint.StaticDraw);
        return this;
    }

    public IDrawStep WithAbsoluteSize(in Vector2 size, bool horizontallyCentered, bool verticallyCentered)
    {
        return this.WithSize(size, horizontallyCentered, verticallyCentered);
    }

    public IDrawStep WithAbsoluteSize()
    {
        // Unlike the WithSize this now uses a unit quad and changes the size with the model matrix
        // This is arguably better and the other should ideally use this aswell
        // This needs a pivot point or similar to match the centering functionality the other one has
        float invW = 1f / this.drawContext.TextureSize.X;
        float invH = 1f / this.drawContext.TextureSize.Y;

        float u0 = this.drawContext.SpriteSize.Left * invW;
        float u1 = this.drawContext.SpriteSize.Right * invW;

        float vTop = this.drawContext.SpriteSize.Top * invH;
        float vBottom = this.drawContext.SpriteSize.Bottom * invH;

        this.FillVertexArray(0, 1, 0, 1, u0, u1, vBottom, vTop);
        return this.WithVertices(in renderContext.vertices);
    }

    public IDrawStep WithSize(in Vector2 size, bool horizontallyCentered, bool verticallyCentered)
    {
        (float x0, float x1, float y0, float y1) =
            ComputeQuadBounds(size, horizontallyCentered, verticallyCentered);

        float invW = 1f / this.drawContext.TextureSize.X;
        float invH = 1f / this.drawContext.TextureSize.Y;

        float u0 = this.drawContext.SpriteSize.Left * invW;
        float u1 = this.drawContext.SpriteSize.Right * invW;

        float vTop = 1f - (this.drawContext.SpriteSize.Top * invH);
        float vBottom = 1f - (this.drawContext.SpriteSize.Bottom * invH);

        this.FillVertexArray(x0, x1, y0, y1, u0, u1, vBottom, vTop);
        return this.WithVertices(in renderContext.vertices);
    }

    /// <summary>
    ///     Writes an interleaved quad into the provided vertex array.
    ///     Vertex layout: (x, y, u, v) for 4 vertices (total 16 floats).
    /// </summary>
    /// <param name="x0">Left x in object/world space.</param>
    /// <param name="x1">Right x in object/world space.</param>
    /// <param name="y0">Bottom y in object/world space.</param>
    /// <param name="y1">Top y in object/world space.</param>
    /// <param name="u0">Left u texture coordinate.</param>
    /// <param name="u1">Right u texture coordinate.</param>
    /// <param name="v0">Bottom v texture coordinate.</param>
    /// <param name="v1">Top v texture coordinate.</param>
    private void FillVertexArray(
        float x0, float x1,
        float y0, float y1,
        float u0, float u1,
        float v0, float v1)
    {
        // Bottom-Left
        renderContext.vertices[0] = x0;
        renderContext.vertices[1] = y0;
        renderContext.vertices[2] = u0;
        renderContext.vertices[3] = v0;
        // Bottom-Right
        renderContext.vertices[4] = x1;
        renderContext.vertices[5] = y0;
        renderContext.vertices[6] = u1;
        renderContext.vertices[7] = v0;
        // Top-Left
        renderContext.vertices[8] = x0;
        renderContext.vertices[9] = y1;
        renderContext.vertices[10] = u0;
        renderContext.vertices[11] = v1;
        // Top-Right
        renderContext.vertices[12] = x1;
        renderContext.vertices[13] = y1;
        renderContext.vertices[14] = u1;
        renderContext.vertices[15] = v1;
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
}