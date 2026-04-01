using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.UI;
using unnamed.Systems.SystemScheduler;
using unnamed.Texture;

namespace unnamed.Rendering.RenderContext;

public class RenderContext : IRenderContext
{
    protected internal const int VerticesArrayLength = 16;
    protected internal const int VerticesLength = VerticesArrayLength * sizeof(float);

    protected internal static readonly uint[] QuadIndices = [0, 1, 2, 2, 1, 3];

    protected internal readonly IAssetStore assetStore;
    protected internal readonly StaticSprite fallbackSprite;
    protected internal readonly float[] vertices = new float[VerticesArrayLength];

    private readonly int shader;
    private readonly int vertexHandle = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly int elementBuffer = GL.GenBuffer();

    protected internal readonly int uBlendFactor;
    protected internal readonly int uModelViewProjection;
    protected internal readonly int uOverrideColor;

    private readonly Vector2 uiReferenceResolution;

    private Camera2D camera;
    protected internal Matrix4 worldProjection;
    private Matrix4 uiProjection;
    private Vector2 uiViewportSize;

    private Vector2 uiStretchScale = Vector2.One;
    private Vector2 uiUniformScale = Vector2.One;

    public RenderContext(IAssetStore assetStore, int shader, Vector2i uiReferenceResolution)
    {
        this.assetStore = assetStore;
        this.shader = shader;
        this.uiReferenceResolution = new Vector2(uiReferenceResolution.X, uiReferenceResolution.Y);

        int aPosition = GL.GetAttribLocation(shader, "aPosition");
        int aTexCoord = GL.GetAttribLocation(shader, "aTexCoord");

        this.uModelViewProjection = GL.GetUniformLocation(shader, "uMVP");
        this.uOverrideColor = GL.GetUniformLocation(shader, "uOverrideColor");
        this.uBlendFactor = GL.GetUniformLocation(shader, "uBlendFactor");

        GL.UseProgram(this.shader);

        GL.BindVertexArray(this.vertexHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);

        GL.EnableVertexAttribArray(aPosition);
        GL.EnableVertexAttribArray(aTexCoord);

        GL.VertexAttribPointer(aPosition, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.VertexAttribPointer(aTexCoord, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
            2 * sizeof(float));

        GL.BufferData(BufferTarget.ElementArrayBuffer, QuadIndices.Length * sizeof(uint), QuadIndices,
            BufferUsageHint.StaticDraw);

        this.fallbackSprite = this.assetStore.Get(GameAssets.FallBack.Default);
    }

    public void UpdateState(Camera2D newCamera)
    {
        this.camera = newCamera;
        this.UpdateUiState();
    }

    public ISpriteStep BeginDraw()
    {
        return new DrawBuilder(this);
    }


    public void OnUnload()
    {
        GL.DeleteVertexArray(this.vertexHandle);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
        GL.DeleteProgram(this.shader);
    }

    /// <summary>
    ///     Creates the final model-view-projection matrix for a direct screen-space UI element.
    /// </summary>
    /// <param name="position">The screen-space position in pixels.</param>
    /// <param name="size">The screen-space size in pixels.</param>
    /// <param name="pivot">The normalized pivot inside the element relative to the given position.</param>
    /// <returns>The final model-view-projection matrix.</returns>
    protected internal Matrix4 CreateAbsoluteUiModelViewProjection(
        in AbsolutePosition position,
        in AbsoluteSize size,
        in UiPivot pivot)
    {
        AbsolutePosition resolvedPosition = position;
        if (resolvedPosition.AllowWrapping)
        {
            resolvedPosition = resolvedPosition.WrapToScreen(this.camera.Viewport);
        }

        Vector2 finalSize = size;
        Vector2 topLeft = new(
            resolvedPosition.X - (finalSize.X * pivot.X),
            resolvedPosition.Y - (finalSize.Y * pivot.Y));

        Matrix4 model =
            Matrix4.CreateScale(finalSize.X, finalSize.Y, 1f) *
            Matrix4.CreateTranslation(topLeft.X, topLeft.Y, 0f);

        return model * this.uiProjection;
    }

    /// <summary>
    ///     Creates the final model-view-projection matrix for a reference-space UI element.
    /// </summary>
    /// <param name="referenceSize">The authored size in reference-space units.</param>
    /// <param name="referenceOffset">The authored offset in reference-space units.</param>
    /// <param name="anchor">The normalized screen anchor.</param>
    /// <param name="pivot">The normalized local pivot.</param>
    /// <param name="scaleMode">The scaling mode relative to the current viewport.</param>
    /// <returns>The final model-view-projection matrix.</returns>
    protected internal Matrix4 CreateReferenceUiModelViewProjection(
        in UiReferenceSize referenceSize,
        in UiReferenceOffset referenceOffset,
        in UiAnchor anchor,
        in UiPivot pivot,
        UiScaleMode scaleMode)
    {
        Vector2 scale = this.ResolveUiScale(scaleMode);
        Vector2 finalSize = referenceSize.ToVector2() * scale;
        Vector2 finalOffset = referenceOffset.ToVector2() * scale;

        Vector2 anchorPosition = new(
            this.uiViewportSize.X * anchor.X,
            this.uiViewportSize.Y * anchor.Y);

        Vector2 topLeft = anchorPosition + finalOffset - (finalSize * pivot.ToVector2());

        Matrix4 model =
            Matrix4.CreateScale(finalSize.X, finalSize.Y, 1f) *
            Matrix4.CreateTranslation(topLeft.X, topLeft.Y, 0f);

        return model * this.uiProjection;
    }

    /// <summary>
    ///     Updates cached UI projection and scale values for the current framebuffer size.
    /// </summary>
    private void UpdateUiState()
    {
        this.worldProjection = this.camera.ViewProjection;

        this.uiViewportSize = new Vector2(this.camera.Viewport.X, this.camera.Viewport.Y);
        this.uiProjection = Matrix4.CreateOrthographicOffCenter(
            0f, this.uiViewportSize.X,
            this.uiViewportSize.Y, 0f,
            -1f, 1f);

        this.uiStretchScale = new Vector2(
            this.uiViewportSize.X / this.uiReferenceResolution.X,
            this.uiViewportSize.Y / this.uiReferenceResolution.Y);

        float uniformScale = MathF.Min(this.uiStretchScale.X, this.uiStretchScale.Y);
        this.uiUniformScale = new Vector2(uniformScale, uniformScale);
    }

    /// <summary>
    ///     Resolves the scale vector for a UI element based on the configured scale mode.
    /// </summary>
    /// <param name="scaleMode">The requested scale mode.</param>
    /// <returns>The resolved scale vector.</returns>
    private Vector2 ResolveUiScale(UiScaleMode scaleMode)
    {
        return scaleMode switch
        {
            UiScaleMode.None => Vector2.One,
            UiScaleMode.Stretch => this.uiStretchScale,
            UiScaleMode.Uniform => this.uiUniformScale,
            _ => Vector2.One
        };
    }
}