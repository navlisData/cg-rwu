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
    private const int VerticesArrayLength = 16;
    protected internal const int VerticesLength = VerticesArrayLength * sizeof(float);
    protected internal static readonly uint[] QuadIndices = [0, 1, 2, 2, 1, 3];
    protected internal readonly IAssetStore assetStore;
    private readonly int elementBuffer = GL.GenBuffer();
    protected internal readonly StaticSprite fallbackSprite;
    private readonly int shader;
    protected internal readonly int uBlendFactor;
    private readonly Vector2 uiReferenceResolution;
    protected internal readonly int uModelViewProjection;
    protected internal readonly int uOverrideColor;
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly int vertexHandle = GL.GenVertexArray();
    protected internal readonly float[] vertices = new float[VerticesArrayLength];

    protected internal Camera2D camera;
    private Matrix4 uiProjection;
    private Vector2 uiStretchScale = Vector2.One;
    private Vector2 uiUniformScale = Vector2.One;
    protected internal Vector2 uiViewportSize;

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

    public IProjectionUiStep BeginUi()
    {
        return new DrawBuilder(this, this.uiProjection);
    }

    public IProjectionGameStep BeginDraw()
    {
        return new DrawBuilder(this, this.camera.ViewProjection);
    }

    public void OnUnload()
    {
        GL.DeleteVertexArray(this.vertexHandle);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
        GL.DeleteProgram(this.shader);
    }


    /// <summary>
    ///     Resolves the scale vector for a UI element based on the configured scale mode.
    /// </summary>
    /// <param name="scaleMode">The requested scale mode.</param>
    /// <returns>The resolved scale vector.</returns>
    protected internal Vector2 ResolveUiScale(UiScaleMode scaleMode)
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