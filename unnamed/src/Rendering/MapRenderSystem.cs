using System.Drawing;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Utils;

namespace unnamed.Rendering;

public class MapRenderSystem(World world, AssetStore assets) : EntitySetSystem<(int shader, Camera2D camera)>(world,
    world.Query()
        .With<TileRef>()
        .With<Loaded>()
        .Build())
{
    private readonly int elementBuffer = GL.GenBuffer();
    private readonly uint[] quadIndices = [0, 1, 2, 2, 1, 3];

    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();

    protected override void Update((int shader, Camera2D camera) param, in Entity e)
    {
        Vector2i chunkPosition = e.Get<GridPosition>().ToVector2I();
        ref Entity[] tiles = ref e.Get<TileRef>().Tiles;

        foreach (Entity tile in tiles)
        {
            Vector2i inChunkPosition = tile.Get<GridPosition>().ToVector2I();
            ref Sprite sprite = ref tile.Get<Sprite>();

            SpriteSheet spriteSheet = assets.GetSpriteSheet(sprite.Frame.Sheet);
            if (!assets.TryGetTexture(spriteSheet.Texture, out Texture2D? texture))
            {
                continue;
            }

            RectangleF rect = spriteSheet.Frames[sprite.Frame.Index];

            float u0 = rect.Left / texture.Width;
            float u1 = rect.Right / texture.Width;
            float v0 = rect.Top / texture.Height;
            float v1 = rect.Bottom / texture.Height;

            float[] quadVertices =
            [
                0f, 0f, u0, v0,
                Constants.TileSizeX, 0f, u1, v0,
                0f, Constants.TileSizeY, u0, v1,
                Constants.TileSizeX, Constants.TileSizeY, u1, v1
            ];

            GL.BindVertexArray(this.vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices,
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this.quadIndices.Length * sizeof(uint), this.quadIndices,
                BufferUsageHint.StaticDraw);

            int vertexLocation = GL.GetAttribLocation(param.shader, "aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            int texCoordLocation = GL.GetAttribLocation(param.shader, "aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
                2 * sizeof(float));


            GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
            GL.ActiveTexture(TextureUnit.Texture0);

            Matrix4 modelSquare = Matrix4.CreateTranslation(
                ((chunkPosition.X * Constants.GridSizeX) + inChunkPosition.X) * Constants.TileSizeX,
                ((chunkPosition.Y * Constants.GridSizeY) + inChunkPosition.Y) * Constants.TileSizeY, 0f);
            Matrix4 mvpSquare = modelSquare * param.camera.ViewProjection;

            int mvpUniformLocation = GL.GetUniformLocation(param.shader, "uMVP");
            GL.UniformMatrix4(mvpUniformLocation, false, ref mvpSquare);

            GL.DrawElements(PrimitiveType.Triangles, this.quadIndices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }

    public void OnUnload()
    {
        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
    }
}