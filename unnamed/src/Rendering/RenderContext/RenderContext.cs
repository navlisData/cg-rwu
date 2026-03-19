using System.Diagnostics;
using System.Drawing;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.UI;
using unnamed.Systems.SystemScheduler;
using unnamed.Texture;

namespace unnamed.Rendering.RenderContext;

public class RenderContext : IRenderContext, ISpriteStep,
    IColorWithoutTextureStep, IColorWithTextureStep,
    IProjectionStep,
    IVerticesRelativeStep, IVerticesAbsoluteStep, IDrawStep
{
    private const int VerticesArrayLength = 16;
    private const int VerticesLength = VerticesArrayLength * sizeof(float);

    private static readonly uint[] QuadIndices = [0, 1, 2, 2, 1, 3];

    private readonly int aPosition;
    private readonly IAssetStore assetStore;
    private readonly int aTexCoord;
    private readonly int elementBuffer = GL.GenBuffer();
    private readonly StaticSprite fallbackSprite;
    private readonly int shader;
    private readonly int uBlendFactor;
    private readonly int uModelViewProjection;
    private readonly int uOverrideColor;
    private readonly int vertexBuffer = GL.GenBuffer();

    private readonly int vertexHandle = GL.GenVertexArray();
    private readonly float[] vertices = new float[VerticesArrayLength];

    // Render State
    private Camera2D camera;

    // Draw State
    private DrawState drawState;
    private Matrix4 screenProjection;


    public RenderContext(IAssetStore assetStore, int shader)
    {
        this.assetStore = assetStore;
        this.shader = shader;

        this.aPosition = GL.GetAttribLocation(shader, "aPosition");
        this.aTexCoord = GL.GetAttribLocation(shader, "aTexCoord");

        this.uModelViewProjection = GL.GetUniformLocation(shader, "uMVP");
        this.uOverrideColor = GL.GetUniformLocation(shader, "uOverrideColor");
        this.uBlendFactor = GL.GetUniformLocation(shader, "uBlendFactor");

        GL.BindVertexArray(this.vertexHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);

        this.fallbackSprite = this.assetStore.Get(GameAssets.FallBack.Default);
    }

    public IProjectionStep WithColoration(in Color4 color, float blendFactor)
    {
        Debug.Assert(blendFactor is >= 0f and <= 1f);
        GL.Uniform4(this.uOverrideColor, color);
        GL.Uniform1(this.uBlendFactor, blendFactor);
        return this;
    }

    public IProjectionStep WithColoration(in Vector3 color, float blendFactor)
    {
        return this.WithColoration(new Color4(color.X, color.Y, color.Z, 1f), blendFactor);
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
        GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public IVerticesRelativeStep WithModelViewProjection(ref Matrix4 modelViewProjection)
    {
        GL.UniformMatrix4(this.uModelViewProjection, false, ref modelViewProjection);
        return this;
    }

    public IVerticesRelativeStep WithPosition(in Vector2 position)
    {
        Matrix4 model = Matrix4.CreateTranslation(position.X, position.Y, 0f);
        Matrix4 modelViewProjection = model * this.camera.ViewProjection;
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
        Matrix4 modelViewProjection = distortedModel * this.camera.ViewProjection;
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

    public IVerticesAbsoluteStep WithAbsolutePosition(in AbsolutePosition position)
    {
        AbsolutePosition pos = position with { Y = this.camera.Viewport.Y - position.Y };
        if (position.AllowWrapping)
        {
            pos = pos.WrapToScreen(this.camera.Viewport);
        }

        Matrix4 model = Matrix4.CreateTranslation(pos.X, pos.Y, 0f);
        Matrix4 modelViewProjection = model * this.screenProjection;
        this.WithModelViewProjection(ref modelViewProjection);
        return this;
    }

    public IColorWithTextureStep WithSprite(in StaticSprite sprite)
    {
        Texture2D texture = this.assetStore.GetTextureById(sprite.SpriteSheetId);
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
        GL.ActiveTexture(TextureUnit.Texture0);
        this.drawState.SpriteSize = sprite.RectPx;
        this.drawState.TextureSize = texture.Size;
        return this;
    }

    public IColorWithTextureStep WithText(in StaticTextTexture text)
    {
        Texture2D texture = text.Texture;
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
        GL.ActiveTexture(TextureUnit.Texture0);
        this.drawState.SpriteSize = new Rectangle(0, 0, texture.Width, texture.Height);
        this.drawState.TextureSize = texture.Size;
        return this;
    }

    public IColorWithoutTextureStep WithoutSprite()
    {
        this.WithSprite(this.fallbackSprite);
        return this;
    }

    public IDrawStep WithVertices(in float[] vertexArray)
    {
        Debug.Assert(vertexArray.Length == VerticesArrayLength);
        GL.BufferData(BufferTarget.ArrayBuffer, VerticesLength, vertexArray,
            BufferUsageHint.StaticDraw);
        return this;
    }

    public IDrawStep WithAbsoluteSize(in Vector2 size, bool horizontallyCentered, bool verticallyCentered)
    {
        return this.WithSize(size, horizontallyCentered, verticallyCentered);
    }

    public IDrawStep WithAbsoluteSize(in Vector2 size, UiAlignment alignment)
    {
        return this.WithSize(size, alignment.HorizontallyCentered, alignment.VerticallyCentered);
    }

    public IDrawStep WithSize(in Vector2 size, bool horizontallyCentered, bool verticallyCentered)
    {
        (float x0, float x1, float y0, float y1) =
            ComputeQuadBounds(size, horizontallyCentered, verticallyCentered);

        float invW = 1f / this.drawState.TextureSize.X;
        float invH = 1f / this.drawState.TextureSize.Y;

        float u0 = this.drawState.SpriteSize.Left * invW;
        float u1 = this.drawState.SpriteSize.Right * invW;

        float vTop = 1f - (this.drawState.SpriteSize.Top * invH);
        float vBottom = 1f - (this.drawState.SpriteSize.Bottom * invH);

        this.FillVertexArray(x0, x1, y0, y1, u0, u1, vBottom, vTop);
        return this.WithVertices(in this.vertices);
    }

    public void UpdateState(Camera2D newCamera)
    {
        this.camera = newCamera;

        this.screenProjection = Matrix4.CreateOrthographicOffCenter(
            0, this.camera.Viewport.X,
            0, this.camera.Viewport.Y,
            -1f, 1f
        );

        GL.UseProgram(this.shader);

        GL.EnableVertexAttribArray(this.aPosition);
        GL.EnableVertexAttribArray(this.aTexCoord);

        GL.VertexAttribPointer(this.aPosition, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.VertexAttribPointer(this.aTexCoord, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
            2 * sizeof(float));

        GL.BufferData(BufferTarget.ElementArrayBuffer, QuadIndices.Length * sizeof(uint), QuadIndices,
            BufferUsageHint.StaticDraw);
    }

    public ISpriteStep BeginDraw()
    {
        return this;
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
        this.vertices[0] = x0;
        this.vertices[1] = y0;
        this.vertices[2] = u0;
        this.vertices[3] = v0;
        // Bottom-Right
        this.vertices[4] = x1;
        this.vertices[5] = y0;
        this.vertices[6] = u1;
        this.vertices[7] = v0;
        // Top-Left
        this.vertices[8] = x0;
        this.vertices[9] = y1;
        this.vertices[10] = u0;
        this.vertices[11] = v1;
        // Top-Right
        this.vertices[12] = x1;
        this.vertices[13] = y1;
        this.vertices[14] = u1;
        this.vertices[15] = v1;
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

    public void OnUnload()
    {
        GL.DeleteVertexArray(this.vertexHandle);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
        GL.DeleteProgram(this.shader);
    }
}