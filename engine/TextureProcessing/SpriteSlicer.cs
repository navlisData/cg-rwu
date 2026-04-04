using System.Drawing;

using OpenTK.Mathematics;

namespace engine.TextureProcessing;

/// <summary>
/// Describes a regular grid layout over a texture to slice sprites from.
/// </summary>
/// <remarks>
/// If <paramref name="Columns"/> and/or <paramref name="Rows"/> are zero, they are derived from the available texture space,
/// starting at (<paramref name="OffsetX"/>, <paramref name="OffsetY"/>) with cell size (<paramref name="CellW"/>, <paramref name="CellH"/>) and gaps (<paramref name="GapX"/>, <paramref name="GapY"/>).
/// </remarks>
/// <param name="CellW">Cell width in pixels.</param>
/// <param name="CellH">Cell height in pixels.</param>
/// <param name="OffsetX">Horizontal pixel offset of the grid origin.</param>
/// <param name="OffsetY">Vertical pixel offset of the grid origin.</param>
/// <param name="GapX">Horizontal gap between cells in pixels.</param>
/// <param name="GapY">Vertical gap between cells in pixels.</param>
/// <param name="Columns">Number of columns; 0 to auto-compute from texture width.</param>
/// <param name="Rows">Number of rows; 0 to auto-compute from texture height.</param>
public readonly record struct TextureGrid(
    int CellW,
    int CellH,
    int OffsetX = 0,
    int OffsetY = 0,
    int GapX = 0,
    int GapY = 0,
    int Columns = 0,
    int Rows = 0)
{
    /// <summary>
    /// Resolves the effective grid dimensions against a given texture size.
    /// </summary>
    /// <param name="texW">Texture width in pixels.</param>
    /// <param name="texH">Texture height in pixels.</param>
    /// <returns>The computed number of columns and rows.</returns>
    public (int cols, int rows) Resolve(int texW, int texH)
    {
        if (Columns > 0 && Rows > 0) return (Columns, Rows);

        int cols = Columns > 0
            ? Columns
            : (texW - OffsetX >= CellW
                ? 1 + (texW - OffsetX - CellW) / (CellW + GapX)
                : 0);

        int rows = Rows > 0
            ? Rows
            : (texH - OffsetY >= CellH
                ? 1 + (texH - OffsetY - CellH) / (CellH + GapY)
                : 0);

        return (Math.Max(cols, 0), Math.Max(rows, 0));
    }
}

/// <summary>
/// Utility methods to create sprites, sprite sets, and animation clips from rectangular regions in a sprite sheet.
/// </summary>
public static class SpriteSlicer
{
    /// <summary>
    /// Creates a single static sprite from an explicit rectangle in a sprite sheet.
    /// </summary>
    /// <param name="spriteSheet">Source sprite sheet descriptor.</param>
    /// <param name="spriteRectangle">Rectangle in pixels within the sprite sheet.</param>
    /// <returns>A <see cref="StaticSprite"/> referencing the given region.</returns>
    public static StaticSprite FromRect(SpriteSheet spriteSheet, Rectangle spriteRectangle, Vector2 pivot)
    {
        return new() { SpriteSheetId = spriteSheet.Id, RectPx = spriteRectangle, Pivot = pivot};
    }

    /// <summary>
    /// Creates a sprite set by slicing a regular grid from a sprite sheet.
    /// </summary>
    /// <param name="spriteSheet">Source sprite sheet descriptor.</param>
    /// <param name="textureGrid">Grid definition describing cell size, origin, gaps, and dimensions.</param>
    /// <param name="indices">
    /// Optional zero-based linear indices (row-major) selecting specific cells; when <c>null</c>, all cells are included.
    /// </param>
    /// <returns>A <see cref="SpriteSet"/> consisting of the selected grid cells; empty if resolved cols/rows are zero.</returns>
    public static SpriteSet FromGrid(Vector2 pivot, SpriteSheet spriteSheet, TextureGrid textureGrid, IEnumerable<int>? indices = null)
    {
        var (cols, rows) = textureGrid.Resolve(spriteSheet.Size.X, spriteSheet.Size.Y);
        if (cols == 0 || rows == 0) return new SpriteSet();

        IEnumerable<int> all = indices ?? Enumerable.Range(0, cols * rows);

        IEnumerable<StaticSprite> Make()
        {
            foreach (var i in all)
            {
                int cx = i % cols, cy = i / cols;
                int x = textureGrid.OffsetX + cx * (textureGrid.CellW + textureGrid.GapX);
                int y = textureGrid.OffsetY + cy * (textureGrid.CellH + textureGrid.GapY);
                yield return FromRect(spriteSheet, new Rectangle(x, y, textureGrid.CellW, textureGrid.CellH), pivot);
            }
        }

        return new SpriteSet(Make());
    }

    /// <summary>
    /// Creates a looping animation clip by taking a contiguous range of frames from a grid.
    /// </summary>
    /// <param name="spriteSheet">Source sprite sheet descriptor.</param>
    /// <param name="textureGrid">Grid definition used to slice frames.</param>
    /// <param name="frameCount">Number of frames to include starting at <paramref name="startIndex"/>.</param>
    /// <param name="fps">Playback frames per second.</param>
    /// <param name="priority">Can be used to implement overriding logic.</param>
    /// <param name="loop">Should the animation be looped.</param>
    /// <param name="startIndex">Starting zero-based cell index (row-major order).</param>
    /// <returns>An <see cref="AnimationClip"/> with the selected frames.</returns>
    public static AnimationClip ClipFromGrid(SpriteSheet spriteSheet, TextureGrid textureGrid, int frameCount,
        float fps, Vector2 pivot, byte priority = 0, bool loop = true, int startIndex = 0)
    {
        var set = FromGrid(pivot, spriteSheet, textureGrid, Enumerable.Range(startIndex, frameCount));
        return new AnimationClip { Frames = set, Fps = fps, Loop = loop, Priority = priority };
    }
}