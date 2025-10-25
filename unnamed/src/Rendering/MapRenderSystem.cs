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
    private readonly uint[] quadIndices = GraphicsUtils.QuadIndices;
    private readonly float[] vertexScratch = new float[16];

    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();

    private readonly IAssetStore assets = assets;

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
   
            GraphicsUtils.FillSpriteQuadGeometry(
                Constants.TileSizeX, Constants.TileSizeY,
                rect, texture,
                vertexScratch
            );

            GL.BindVertexArray(this.vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexScratch.Length * sizeof(float), vertexScratch,
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