using OpenTK.Mathematics;

namespace unnamed.Utils;

public static class Projection
{
    /// <summary>
    ///     Converts a screen-space coordinate (in pixels) to a world-space position
    ///     using the given View-projection matrix.
    /// </summary>
    /// <param name="screenCoords">The position in screen coordinates (pixels).</param>
    /// <param name="viewport">The width and height of the screen or viewport in pixels.</param>
    /// <param name="viewProjection">The current View-projection matrix (must be orthographic).</param>
    /// <returns>The corresponding position in world space.</returns>
    /// <remarks>
    ///     This method assumes an orthographic projection. For perspective cameras, use
    ///     <see cref="Vector3.Unproject" /> instead, which handles depth and perspective
    ///     transformations automatically.
    /// </remarks>
    public static Vector2 ScreenToWorldCoordinates(Vector2 screenCoords, Vector2i viewport, Matrix4 viewProjection)
    {
        float x = (2.0f * screenCoords.X / viewport.X) - 1.0f;
        float y = 1.0f - (2.0f * screenCoords.Y / viewport.Y);
        Vector4 normalizedDeviceCoords = new(x, y, 0f, 1f);

        Vector4 world4 = normalizedDeviceCoords * Matrix4.Invert(viewProjection);

        return new Vector2(world4.X, world4.Y);
    }
}