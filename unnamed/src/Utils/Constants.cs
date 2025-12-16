using OpenTK.Windowing.GraphicsLibraryFramework;

namespace unnamed.Utils;

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

public static class GameData
{
    public const float PlayerAttackCooldown = 1f;
}