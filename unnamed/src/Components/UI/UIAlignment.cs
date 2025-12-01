namespace unnamed.Components.UI;

/// <summary>
///     Defines how a UI element is aligned along both axes.
/// </summary>
/// <param name="verticallyCentered">
///     When set to <c>true</c>, <see cref="AbsolutePosition.Y" /> refers to the vertical center;
///     otherwise, it refers to the top edge.
/// </param>
/// <param name="horizontallyCentered">
///     When set to <c>true</c>, <see cref="AbsolutePosition.X" /> refers to the horizontal center;
///     otherwise, it refers to the left edge.
/// </param>
public struct UiAlignment(bool verticallyCentered, bool horizontallyCentered)
{
    public bool VerticallyCentered = verticallyCentered;
    public bool HorizontallyCentered = horizontallyCentered;
}