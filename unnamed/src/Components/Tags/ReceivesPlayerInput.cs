namespace unnamed.Components.Tags;

[Flags]
public enum ReceivesPlayerInput
{
    None = 0,
    MovementControls = 1 << 0,
    CameraControls = 1 << 1,
    MouseControls = 1 << 2,
    PositionByMouse = 1 << 3,
    AlignByMouse = 1 << 4
}