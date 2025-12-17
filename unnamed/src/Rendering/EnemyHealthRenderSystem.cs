using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Utils;

namespace unnamed.Rendering;

public sealed class EnemyHealthRenderSystem(World world)
    : ExtendedEntitySetSystem<int, Camera2D>(world, new QueryBuilder()
        .With<Enemy>()
        .With<Sprite>()
        .With<Position>()
        .With<Velocity>()
        .With<Transform>()
        .With<EntityStats>()
        .Without<Sleeping>()
        .Build())
{
    private readonly float[] vertexScratch = new float[16];
    private int colorUniformLocation = -1;
    private int elementBuffer;

    private int mvpUniformLocation = -1;
    private int vertexArray;
    private int vertexBuffer;

    /// <summary>
    ///     Binds the correct program/VertexArrayObject and ensures
    ///     the correct VBO is bound for per-entity vertex uploads.
    /// </summary>
    /// <param name="shader">Healthbar shader program handle.</param>
    protected override void BeforeUpdate(int shader)
    {
        GL.UseProgram(shader);
        GL.BindVertexArray(this.vertexArray);

        this.vertexArray = GL.GenVertexArray();
        this.vertexBuffer = GL.GenBuffer();
        this.elementBuffer = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, this.vertexScratch.Length * sizeof(float), IntPtr.Zero,
            BufferUsageHint.DynamicDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, GraphicsUtils.QuadIndices.Length * sizeof(uint),
            GraphicsUtils.QuadIndices, BufferUsageHint.StaticDraw);

        // sprite.vert: 0 = aPosition, 1 = aTexCoord
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

        this.mvpUniformLocation = GL.GetUniformLocation(shader, "uMVP");
        this.colorUniformLocation = GL.GetUniformLocation(shader, "uColor");
    }

    /// <summary>
    ///     Renders background + filled foreground healthbar above the enemy.
    /// </summary>
    protected override void Update(Camera2D camera, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Sprite sprite = ref handle.Get<Sprite>();
        ref Transform transform = ref handle.Get<Transform>();
        Vector2 position = handle.Get<Position>().ToWorldPosition();
        ref EntityStats stats = ref handle.Get<EntityStats>();

        int maxHealth = Math.Max(0, stats.MaxHealthUnits);
        if (maxHealth <= 0)
        {
            return;
        }

        int currentHealth = stats.Hitpoints;
        float ratio = Math.Clamp(currentHealth / (float)maxHealth, 0f, 1f);

        Vector2 spriteWorldSize = GraphicsUtils.ComputeSpriteWorldSize(transform.Size, sprite.Frame.RectPx);

        float barWidth = spriteWorldSize.X * 0.9f;
        float barHeight = MathF.Max(0.04f, spriteWorldSize.Y * 0.08f);

        float yMargin = barHeight * 0.6f;

        float xLeft = position.X - (barWidth * 0.5f);
        float yBottom = position.Y + spriteWorldSize.Y + yMargin;

        Matrix4 model = Matrix4.CreateTranslation(xLeft, yBottom, 0f);
        Matrix4 mvp = model * camera.ViewProjection;
        GL.UniformMatrix4(this.mvpUniformLocation, false, ref mvp);

        // Background
        GraphicsUtils.FillSolidQuadGeometry(new Vector2(barWidth, barHeight), this.vertexScratch, false, false);
        GraphicsUtils.UploadQuadVertices(this.vertexScratch);
        GraphicsUtils.DrawColoredQuad(this.colorUniformLocation, new Vector4(0f, 0f, 0f, 0.55f));

        // Foreground
        float fillWidth = barWidth * ratio;
        if (fillWidth > 0.0001f)
        {
            GraphicsUtils.FillSolidQuadGeometry(new Vector2(fillWidth, barHeight), this.vertexScratch, false, false);
            GraphicsUtils.UploadQuadVertices(this.vertexScratch);

            Vector4 fg = ratio switch
            {
                > 0.6f => new Vector4(0.20f, 0.85f, 0.25f, 0.95f),
                > 0.3f => new Vector4(0.95f, 0.85f, 0.15f, 0.95f),
                _ => new Vector4(0.90f, 0.20f, 0.20f, 0.95f)
            };

            GraphicsUtils.DrawColoredQuad(this.colorUniformLocation, fg);
        }
    }

    public void OnUnload()
    {
        this.vertexArray = 0;
        this.vertexBuffer = 0;
        this.elementBuffer = 0;
    }
}