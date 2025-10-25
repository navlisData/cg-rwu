using OpenTK.Windowing.GraphicsLibraryFramework;

namespace unnamed.Utils;

public static class Constants
{
    /// <summary>
    ///     Map tile width
    /// </summary>
    public const float TileSizeX = 4;

    /// <summary>
    ///     Map tile height
    /// </summary>
    public const float TileSizeY = 4;

    /// <summary>
    ///     Amount of vertical cells in a map chunk
    /// </summary>
    public const int GridSizeX = 16;

    /// <summary>
    ///     Amount of horizontal cells in a map chunk
    /// </summary>
    public const int GridSizeY = 16;
}

public static class Controls
{
    public const Keys MoveUp = Keys.W;
    public const Keys MoveRight = Keys.D;
    public const Keys MoveDown = Keys.S;
    public const Keys MoveLeft = Keys.A;
    
    public const Keys RotateCamCW = Keys.Q;
    public const Keys RotateCamCCW = Keys.E;
    
    public const MouseButton PlayerShoot = MouseButton.Left;
}