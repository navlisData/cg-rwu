using System.Diagnostics;
using System.Drawing;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.UI;

namespace unnamed.Rendering.RenderContext;

public struct DrawBuilder(RenderContext renderContext, Matrix4 projectionMatrix) : IDrawBuilder
{
    private DrawContext drawContext;

    public IDrawStep WithColoration(in Color4 color, float blendFactor)
    {
        Debug.Assert(blendFactor is >= 0f and <= 1f);
        GL.Uniform4(renderContext.uOverrideColor, color);
        GL.Uniform1(renderContext.uBlendFactor, blendFactor);
        return this;
    }

    public IDrawStep WithColoration(in Vector3 color, float blendFactor)
    {
        return this.WithColoration(new Color4(color.X, color.Y, color.Z, 1f), blendFactor);
    }

    public IDrawStep WithColoration(in Color4? color, float blendFactor)
    {
        return color is null ? this.WithoutColoration() : this.WithColoration(color.Value, blendFactor);
    }

    public IDrawStep WithAlpha(float alpha)
    {
        return this.WithColoration(new Color4(0f, 0f, 0f, alpha), 0f);
    }

    public IDrawStep WithoutColoration()
    {
        return this.WithColoration(new Color4(0f, 0f, 0f, 1f), 0f);
    }

    public ISpriteStep WithModelViewProjection(ref Matrix4 modelViewProjection)
    {
        GL.UniformMatrix4(renderContext.uModelViewProjection, false, ref modelViewProjection);
        return this;
    }

    public ISpriteStep WithPosition(in Vector2 position, Vector2 size, Vector2 pivot)
    {
        Matrix4 local = Matrix4.CreateTranslation(-pivot.X, -pivot.Y, 0f) * Matrix4.CreateScale(size.X, -size.Y, 1);

        Matrix4 model = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 modelViewProjection = local * model * projectionMatrix;
        return this.WithModelViewProjection(ref modelViewProjection);
    }

    public ISpriteStep WithPosition(in float x, in float y, Vector2 size, Vector2 pivot)
    {
        return this.WithPosition(new Vector2(x, y), size, pivot);
    }

    public ISpriteStep WithPositionAndTransform(in Vector2 position, in Transform transform, Vector2 size,
        Vector2 pivot)
    {
        Matrix4 distortion =
            Matrix4.CreateRotationZ(transform.Rotation) *
            Matrix4.CreateScale(transform.Scale) *
            Matrix4.CreateTranslation(0, transform.Height, 0);

        return this.WithPositionAndDistortion(position, distortion, size, pivot);
    }

    public ISpriteStep WithPositionAndDistortion(in Vector2 position, in Matrix4 distortionMatrix, Vector2 size,
        Vector2 pivot)
    {
        Matrix4 local = Matrix4.CreateTranslation(-pivot.X, -pivot.Y, 0f) * Matrix4.CreateScale(size.X, -size.Y, 1);

        Matrix4 model = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 distortedModel = local * distortionMatrix * model;
        Matrix4 modelViewProjection = distortedModel * projectionMatrix;
        return this.WithModelViewProjection(ref modelViewProjection);
    }

    public ISpriteStep WithPositionAndDistortion(in float x, in float y, in Matrix4 distortionMatrix, Vector2 size,
        Vector2 pivot)
    {
        return this.WithPositionAndDistortion(new Vector2(x, y), distortionMatrix, size, pivot);
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

    public IColorWithTextureStep WithSprite(in StaticTextTexture text)
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

    public void Draw()
    {
        float invW = 1f / this.drawContext.TextureSize.X;
        float invH = 1f / this.drawContext.TextureSize.Y;

        float u0 = this.drawContext.SpriteSize.Left * invW;
        float u1 = this.drawContext.SpriteSize.Right * invW;

        float v0 = 1f - (this.drawContext.SpriteSize.Top * invH);
        float v1 = 1f - (this.drawContext.SpriteSize.Bottom * invH);

        this.FillVertexArray(0f, 1f, 0f, 1f, u0, u1, v0, v1);

        GL.BufferData(BufferTarget.ArrayBuffer, RenderContext.VerticesLength, renderContext.vertices,
            BufferUsageHint.StaticDraw);
        GL.DrawElements(PrimitiveType.Triangles, RenderContext.QuadIndices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public ISpriteStep WithAbsolutePosition(
        in AbsolutePosition position,
        in AbsoluteSize size,
        in Vector2 pivot)
    {
        AbsolutePosition resolvedPosition = position;
        if (resolvedPosition.AllowWrapping)
        {
            resolvedPosition = resolvedPosition.WrapToScreen(renderContext.camera.Viewport);
        }

        Vector2 topLeft = resolvedPosition - (size * pivot);

        Matrix4 model =
            Matrix4.CreateScale(size.Width, size.Height, 1f) *
            Matrix4.CreateTranslation(topLeft.X, topLeft.Y, 0f);

        Matrix4 modelViewProjection = model * projectionMatrix;
        return this.WithModelViewProjection(ref modelViewProjection);
    }

    public ISpriteStep WithReferencePosition(in UiReferenceOffset referenceOffset, in UiReferenceSize referenceSize,
        in Vector2 pivot, in UiAnchor anchor, UiScaleMode scaleMode)
    {
        Vector2 scale = renderContext.ResolveUiScale(scaleMode);
        Vector2 finalSize = referenceSize * scale;
        Vector2 finalOffset = referenceOffset * scale;

        Vector2 anchorPosition = renderContext.uiViewportSize * anchor;
        Vector2 topLeft = anchorPosition + finalOffset - (finalSize * pivot);

        Matrix4 model =
            Matrix4.CreateScale(finalSize.X, finalSize.Y, 1f) *
            Matrix4.CreateTranslation(topLeft.X, topLeft.Y, 0f);

        Matrix4 modelViewProjection = model * projectionMatrix;
        return this.WithModelViewProjection(ref modelViewProjection);
    }


    /// <summary>
    ///     Writes an interleaved quad into the internal vertex array.
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