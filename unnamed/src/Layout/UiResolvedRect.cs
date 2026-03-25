using OpenTK.Mathematics;

namespace unnamed.Layout;

/// <summary>
///     Represents a resolved UI rectangle in absolute screen pixels.
/// </summary>
/// <param name="Position">The top-left position in screen pixels.</param>
/// <param name="Size">The final size in screen pixels.</param>
public readonly record struct UiResolvedRect(Vector2 Position, Vector2 Size);