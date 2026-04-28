using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.UI;
using unnamed.Resources;
using unnamed.Systems.SystemScheduler;
using unnamed.Texture;

namespace unnamed.Rendering.RenderContext;

public struct RenderContext : IRenderContext
{
    private const int VerticesArrayLength = 16;
    internal const int VerticesLength = VerticesArrayLength * sizeof(float);
    internal static readonly uint[] QuadIndices = [0, 1, 2, 2, 1, 3];
    internal readonly AssetStore AssetStore;
    private readonly int elementBuffer = GL.GenBuffer();
    internal readonly StaticSprite FallbackSprite;
    private readonly int shader;
    internal readonly int UBlendFactor;
    private readonly Vector2 uiReferenceResolution;
    internal readonly int UModelViewProjection;
    internal readonly int UOverrideColor;
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly int vertexHandle = GL.GenVertexArray();
    internal readonly float[] Vertices = new float[VerticesArrayLength];

    internal Camera2D Camera;
    private Matrix4 uiProjection;
    private Vector2 uiStretchScale = Vector2.One;
    private Vector2 uiUniformScale = Vector2.One;
    internal Vector2 UiViewportSize;

    public RenderContext(AssetStore assetStore, int shader, Vector2i uiReferenceResolution)
    {
        this.AssetStore = assetStore;
        this.shader = shader;
        this.uiReferenceResolution = new Vector2(uiReferenceResolution.X, uiReferenceResolution.Y);

        int aPosition = GL.GetAttribLocation(shader, "aPosition");
        int aTexCoord = GL.GetAttribLocation(shader, "aTexCoord");

        this.UModelViewProjection = GL.GetUniformLocation(shader, "uMVP");
        this.UOverrideColor = GL.GetUniformLocation(shader, "uOverrideColor");
        this.UBlendFactor = GL.GetUniformLocation(shader, "uBlendFactor");

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

        this.FallbackSprite = this.AssetStore.Get(GameAssets.FallBack.Default);
    }

    public void UpdateState(Camera2D newCamera)
    {
        this.Camera = newCamera;

        this.UiViewportSize = new Vector2(this.Camera.Viewport.X, this.Camera.Viewport.Y);
        this.uiProjection = Matrix4.CreateOrthographicOffCenter(
            0f, this.UiViewportSize.X,
            this.UiViewportSize.Y, 0f,
            -1f, 1f);

        this.uiStretchScale = new Vector2(
            this.UiViewportSize.X / this.uiReferenceResolution.X,
            this.UiViewportSize.Y / this.uiReferenceResolution.Y);

        float uniformScale = MathF.Min(this.uiStretchScale.X, this.uiStretchScale.Y);
        this.uiUniformScale = new Vector2(uniformScale, uniformScale);
    }

    public IProjectionUiStep BeginUi()
    {
        return new DrawBuilder(this, this.uiProjection);
    }

    public IProjectionGameStep BeginDraw()
    {
        return new DrawBuilder(this, this.Camera.ViewProjection);
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
    internal Vector2 ResolveUiScale(UiScaleMode scaleMode)
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