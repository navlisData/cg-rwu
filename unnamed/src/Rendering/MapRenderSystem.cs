using Engine.Ecs;
using Engine.Ecs.Systems;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Rendering;
using unnamed.Utils;

namespace unnamed.Rendering;

public class MapRenderSystem : EntitySetSystem<(int shader, Camera2D camera)>
{
    private readonly int elementBuffer;
    private readonly uint[] quadIndices = [0, 1, 2, 2, 1, 3];

    private readonly float[] quadVertices =
        [0f, 0f, Constants.TileSizeX, 0f, 0f, Constants.TileSizeY, Constants.TileSizeX, Constants.TileSizeY];

    private readonly int vertexArray;
    private readonly int vertexBuffer;

    public MapRenderSystem(World world) : base(world, world.Query()
        .With<TileType>()
        .Build())
    {
        this.vertexArray = GL.GenVertexArray();
        this.vertexBuffer = GL.GenBuffer();
        this.elementBuffer = GL.GenBuffer();
    }

    protected override void Update((int shader, Camera2D camera) param, in Entity e)
    {
        ref Entity chunk = ref e.Get<ChunkRef>().Chunk;
        Vector2i chunkPosition = chunk.Get<GridPosition>().ToVector2I();
        Vector2i inChunkPosition = e.Get<GridPosition>().ToVector2I();
        ref TileType type = ref e.Get<TileType>();

        Vector4 color = type switch
        {
            TileType.Floor1 => new Vector4(0.9f, 0.9f, 0.9f, 1f),
            TileType.Floor2 => new Vector4(0.8f, 0.8f, 0.8f, 1f),
            TileType.Wall => new Vector4(0.1f, 0.1f, 0.1f, 1f),
            _ => new Vector4(1f, 1f, 1f, 1f)
        };

        GL.BindVertexArray(this.vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, this.quadVertices.Length * sizeof(float), this.quadVertices,
            BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, this.quadIndices.Length * sizeof(uint), this.quadIndices,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        int mvpUniformLocation = GL.GetUniformLocation(param.shader, "uMVP");
        int colorUniformLocation = GL.GetUniformLocation(param.shader, "uColor");


        Matrix4 modelSquare = Matrix4.CreateTranslation(
            ((chunkPosition.X * Constants.GridSizeX) + inChunkPosition.X) * Constants.TileSizeX,
            ((chunkPosition.Y * Constants.GridSizeY) + inChunkPosition.Y) * Constants.TileSizeY, 0f);
        Matrix4 mvpSquare = modelSquare * param.camera.ViewProjection;
        GL.UniformMatrix4(mvpUniformLocation, false, ref mvpSquare);
        GL.Uniform4(colorUniformLocation, color);

        GL.BindVertexArray(this.vertexArray);
        GL.DrawElements(PrimitiveType.Triangles, this.quadIndices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void onUnload()
    {
        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
    }
}