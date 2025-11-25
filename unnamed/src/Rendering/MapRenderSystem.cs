using System.Drawing;

using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using unnamed.Components.Map;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.GameMap;
using unnamed.Utils;

namespace unnamed.Rendering;

public class MapRenderSystem(World world, IAssetStore assets)
    : ExtendedEntitySetSystem<int, (Camera2D camera, int layer)>(world,
        world.Query()
            .With<TileGrid>()
            .With<Loaded>()
            .Build())
{
    private readonly int elementBuffer = GL.GenBuffer();
    private readonly uint[] quadIndices = GraphicsUtils.QuadIndices;

    private readonly int vertexArray = GL.GenVertexArray();
    private readonly int vertexBuffer = GL.GenBuffer();
    private readonly float[] vertexScratch = new float[16];
    private int mvpUniformLocation;
    private int texCoordLocation;
    private int vertexLocation;

    protected override bool BeforeUpdate(int shader)
    {
        GL.UseProgram(shader);

        GL.BindVertexArray(this.vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBuffer);

        this.vertexLocation = GL.GetAttribLocation(shader, "aPosition");
        this.texCoordLocation = GL.GetAttribLocation(shader, "aTexCoord");
        this.mvpUniformLocation = GL.GetUniformLocation(shader, "uMVP");

        GL.EnableVertexAttribArray(this.vertexLocation);
        GL.EnableVertexAttribArray(this.texCoordLocation);

        GL.VertexAttribPointer(this.vertexLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.VertexAttribPointer(this.texCoordLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
            2 * sizeof(float));

        GL.BufferData(BufferTarget.ElementArrayBuffer, this.quadIndices.Length * sizeof(uint), this.quadIndices,
            BufferUsageHint.StaticDraw);

        return false;
    }

    protected override void Update((Camera2D camera, int layer) context, in Entity e)
    {
        (Camera2D camera, int layer) = context;
        Vector2i chunkPosition = e.Get<GridPosition>().ToVector2I();
        ref Tile[] tiles = ref e.Get<TileGrid>().Tiles;

        for (int y = 0; y < Map.ChunkSize; y++)
        {
            for (int x = 0; x < Map.ChunkSize; x++)
            {
                Tile tile = tiles[x + (y * Map.ChunkSize)];
                if (tile.layer != layer) { continue; }

                Matrix4 modelSquare = Matrix4.CreateTranslation(
                    ((chunkPosition.X * Map.ChunkSize) + x) * Map.TileSize,
                    ((chunkPosition.Y * Map.ChunkSize) + y) * Map.TileSize, 0f);
                Matrix4 mvpSquare = modelSquare * camera.ViewProjection;

                StaticSprite sprite = tile.Sprite;

                Texture2D texture = assets.GetTextureById(sprite.SpriteSheetId);
                RectangleF rect = sprite.RectPx;

                GraphicsUtils.FillSpriteQuadGeometry(
                    new Vector2(Map.TileSize, Map.TileSize),
                    in rect, in texture,
                    in this.vertexScratch, false, false);

                GraphicsUtils.RenderSpriteQuad(texture.Handle, this.mvpUniformLocation, in this.vertexScratch,
                    ref mvpSquare);

                StaticSprite? overlay = tile.OverlaySprite;

                if (overlay != null)
                {
                    Texture2D overlayTexture = assets.GetTextureById(overlay.SpriteSheetId);
                    RectangleF overlayRect = overlay.RectPx;

                    GraphicsUtils.FillSpriteQuadGeometry(
                        new Vector2(Map.TileSize, Map.TileSize),
                        in overlayRect, in overlayTexture,
                        in this.vertexScratch, false, false);

                    GraphicsUtils.RenderSpriteQuad(overlayTexture.Handle, this.mvpUniformLocation,
                        in this.vertexScratch,
                        ref mvpSquare);
                }
            }
        }
    }

    public void OnUnload()
    {
        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.vertexBuffer);
        GL.DeleteBuffer(this.elementBuffer);
    }
}