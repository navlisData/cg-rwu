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

    public IVerticesStep WithModelViewProjection(ref Matrix4 modelViewProjection)
    {
        GL.UniformMatrix4(renderContext.uModelViewProjection, false, ref modelViewProjection);
        return this;
    }

    public IVerticesStep WithPosition(in Vector2 position, Vector2 size, Vector2 pivot)
    {
        Matrix4 local = Matrix4.CreateTranslation(-pivot.X, -pivot.Y, 0f) * Matrix4.CreateScale(size.X, -size.Y, 1);

        Matrix4 model = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 modelViewProjection = local * model * renderContext.worldProjection;
        return this.WithModelViewProjection(ref modelViewProjection);
    }

    public IVerticesStep WithPosition(in float x, in float y, Vector2 size, Vector2 pivot)
    {
        return this.WithPosition(new Vector2(x, y), size, pivot);
    }

    public IVerticesStep WithPositionAndTransform(in Vector2 position, in Transform transform, Vector2 size,
        Vector2 pivot)
    {
        Matrix4 distortion =
            Matrix4.CreateRotationZ(transform.Rotation) *
            Matrix4.CreateScale(transform.Scale) *
            Matrix4.CreateTranslation(0, transform.Height, 0);

        return this.WithPositionAndDistortion(new Vector2(position.X, position.Y), distortion, size, pivot);
    }

    public IVerticesStep WithPositionAndDistortion(in Vector2 position, in Matrix4 distortionMatrix, Vector2 size,
        Vector2 pivot)
    {
        Matrix4 local = Matrix4.CreateTranslation(-pivot.X, -pivot.Y, 0f) * Matrix4.CreateScale(size.X, -size.Y, 1);

        Matrix4 model = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 distortedModel = local * distortionMatrix * model;
        Matrix4 modelViewProjection = distortedModel * renderContext.worldProjection;
        return this.WithModelViewProjection(ref modelViewProjection);
    }

    public IVerticesStep WithPositionAndDistortion(in float x, in float y, in Matrix4 distortionMatrix, Vector2 size,
        Vector2 pivot)
    {
        return this.WithPositionAndDistortion(new Vector2(x, y), distortionMatrix, size, pivot);
    }

    /// <summary>
    ///     Applies a direct screen-space UI transform.
    /// </summary>
    /// <param name="position">The screen-space position in pixels.</param>
    /// <param name="size">The screen-space size in pixels.</param>
    /// <param name="pivot">The normalized pivot inside the element relative to the given position.</param>
    /// <returns>The next draw step.</returns>
    public IVerticesStep WithAbsoluteUiTransform(
        in AbsolutePosition position,
        in AbsoluteSize size,
        in Vector2 pivot)
    {
        Matrix4 modelViewProjection = renderContext.CreateAbsoluteUiModelViewProjection(position, size, pivot);
        this.WithModelViewProjection(ref modelViewProjection);
        return this;
    }

    /// <summary>
    ///     Applies a reference-space UI transform using anchor, pivot and scaling rules.
    /// </summary>
    /// <param name="referenceOffset">The authored offset in reference-space units.</param>
    /// <param name="referenceSize">The authored size in reference-space units.</param>
    /// <param name="anchor">The normalized screen anchor.</param>
    /// <param name="pivot">The normalized local pivot.</param>
    /// <param name="scaleMode">The scaling mode relative to the viewport.</param>
    /// <returns>The next draw step.</returns>
    public IVerticesStep WithReferenceUiTransform(
        in UiReferenceOffset referenceOffset,
        in UiReferenceSize referenceSize,
        in UiAnchor anchor,
        in Vector2 pivot,
        UiScaleMode scaleMode)
    {
        Matrix4 modelViewProjection = renderContext.CreateReferenceUiModelViewProjection(
            referenceSize,
            referenceOffset,
            anchor,
            pivot,
            scaleMode);

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

    /// <summary>
    ///     Writes a unit quad for UI rendering.
    ///     Final placement and size are provided by the UI transform matrix.
    /// </summary>
    /// <returns>The next draw step.</returns>
    public IDrawStep WithUnitQuad()
    {
        return this.WithTexturedQuad(0f, 1f, 1f, 0f);
    }

    private IDrawStep WithTexturedQuad(float x0, float x1, float y0, float y1)
    {
        (float u0, float u1, float vBottom, float vTop) = this.ComputeSpriteUvBounds();

        this.FillVertexArray(x0, x1, y0, y1, u0, u1, vBottom, vTop);
        return this.WithVertices(in renderContext.vertices);
    }

    private (float u0, float u1, float vBottom, float vTop) ComputeSpriteUvBounds()
    {
        float invW = 1f / this.drawContext.TextureSize.X;
        float invH = 1f / this.drawContext.TextureSize.Y;

        float u0 = this.drawContext.SpriteSize.Left * invW;
        float u1 = this.drawContext.SpriteSize.Right * invW;

        float vTop = 1f - (this.drawContext.SpriteSize.Top * invH);
        float vBottom = 1f - (this.drawContext.SpriteSize.Bottom * invH);

        return (u0, u1, vBottom, vTop);
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
}