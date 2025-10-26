using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace unnamed.Utils;

public static class MouseExtensions
{
    /// <summary>
    /// Computes an 8-way (cardinal + diagonal) direction from the absolute mouse
    /// position relative to the window center.
    /// </summary>
    /// <param name="mouse">
    /// The current <see cref="MouseState"/> associated with the window.
    /// </param>
    /// <param name="windowSize">
    /// Client area size in pixels (the same coordinate space as <see cref="MouseState.Position"/>).
    /// </param>
    /// <param name="currentDirection">
    /// The direction to keep if the mouse is inside the dead zone (prevents jitter).
    /// </param>
    /// <param name="deadZonePx">
    /// Radius of the dead zone in pixels around the window center. Defaults to 50.
    /// </param>
    /// <returns>
    /// One of the eight directions based on the mouse position; returns
    /// <paramref name="currentDirection"/> while inside the dead zone.
    /// </returns>
    public static CharacterDirection Get8WayDirectionFromPosition(
        this MouseState mouse,
        Vector2 windowSize,
        CharacterDirection currentDirection,
        float deadZonePx = 50f)
    {
        // Window center in client coordinates.
        Vector2 center = windowSize * 0.5f;

        // Vector from center to the mouse position.
        Vector2 centerToMouse = mouse.Position - center;

        // Dead zone check
        float deadZoneSq = deadZonePx * deadZonePx;
        if (centerToMouse.LengthSquared <= deadZoneSq)
            return currentDirection;

        // Screen Y grows downward; negate Y to compute a mathematical angle (counter-clockwise).
        // Angle definition: 0 = right (+X), π/2 = up (-screen Y), range normalized to [0, 2π).
        float angle = MathF.Atan2(-centerToMouse.Y, centerToMouse.X);
        if (angle < 0) angle += 2f * MathF.PI;

        // Map angle to one of 8 sectors (each 45°). Round to nearest sector.
        int sector = (int)MathF.Round(angle / (MathF.PI / 4f)) & 7;

        return sector switch
        {
            0 => CharacterDirection.Right,
            1 => CharacterDirection.UpRight,
            2 => CharacterDirection.Up,
            3 => CharacterDirection.UpLeft,
            4 => CharacterDirection.Left,
            5 => CharacterDirection.DownLeft,
            6 => CharacterDirection.Down,
            _ => CharacterDirection.DownRight
        };
    }
}
