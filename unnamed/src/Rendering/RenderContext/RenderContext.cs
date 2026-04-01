using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Systems.SystemScheduler;
using unnamed.Texture;

namespace unnamed.Rendering.RenderContext;

public class RenderContext : IRenderContext
{
    protected internal const int VerticesArrayLength = 16;
    protected internal const int VerticesLength = VerticesArrayLength * sizeof(float);

    protected internal static readonly uint[] QuadIndices = [0, 1, 2, 2, 1, 3];

    protected internal readonly IAssetStore assetStore;
    private readonly int elementBuffer = GL.GenBuffer();
    protected internal readonly StaticSprite fallbackSprite;
    private readonly int shader;
    protected internal readonly int uBlendFactor;
    private readonly Vector2i uiReferenceSize;
    protected internal readonly int uModelViewProjection;
    protected internal readonly int uOverrideColor;
    private readonly int vertexBuffer = GL.GenBuffer();

    private readonly int vertexHandle = GL.GenVertexArray();
    protected internal readonly float[] vertices = new float[VerticesArrayLength];

    protected internal Camera2D camera;
    private Matrix4 uiProjection;
    private Matrix4 uiView;
    protected internal Matrix4 uiViewProjection;

    public RenderContext(IAssetStore assetStore, int shader)
    {
        this.assetStore = assetStore;
        this.shader = shader;

        int aPosition1 = GL.GetAttribLocation(shader, "aPosition");
        int aTexCoord1 = GL.GetAttribLocation(shader, "aTexCoord");

        this.uModelViewProjection = GL.GetUniformLocation(shader, "uMVP");
        this.uOverrideColor = GL.GetUniformLocation(shader, "uOverrideColor");
        this.uBlendFactor = GL.GetUniformLocation(shader, "uBlendFactor");

        GL.UseProgram(this.shader);

        GL.BindVertexArray(this.vertexHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);

        GL.EnableVertexAttribArray(aPosition1);
        GL.EnableVertexAttribArray(aTexCoord1);

        GL.VertexAttribPointer(aPosition1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.VertexAttribPointer(aTexCoord1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
            2 * sizeof(float));

        GL.BufferData(BufferTarget.ElementArrayBuffer, QuadIndices.Length * sizeof(uint), QuadIndices,
            BufferUsageHint.StaticDraw);

        this.fallbackSprite = this.assetStore.Get(GameAssets.FallBack.Default);
        this.uiReferenceSize = new Vector2i(500, 500);
    }


    public void UpdateState(Camera2D newCamera)
    {
        this.camera = newCamera;

        float scale = (float)this.camera.Viewport.Y / this.uiReferenceSize.Y;

        float offsetX = ((this.camera.Viewport.X / scale) - this.uiReferenceSize.X) * 0.5f;
        float offsetY = ((this.camera.Viewport.Y / scale) - this.uiReferenceSize.Y) * 0.5f;

        // With this scaling is more or less free
        this.uiView = Matrix4.CreateScale(scale, scale, 1f) * Matrix4.CreateTranslation(offsetX, offsetY, 0f);

        // This is the coordinate system that is used to render the elements according to their model matrix
        // Compare also the cameraProjection
        // Instead of a reference size one could also use a reference coord system of f.e. 0..1 or -1..1 to be able to position elements easily
        // To be better it would need to be combined with some sort of anchor layout system
        this.uiProjection = Matrix4.CreateOrthographicOffCenter(
            0, this.uiReferenceSize.X,
            this.uiReferenceSize.Y, 0,
            -1f, 1f);

        // The problem is there are currently still issues with stretching and positioning, if the size != referenceSize
        this.uiViewProjection = this.uiView * this.uiProjection;
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
}