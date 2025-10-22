using OpenTK.Mathematics;

namespace unnamed.Components.Rendering;

public struct Camera2D
{
    public float Rotation;
    public float Zoom;

    /// Visible world units vertically at Zoom = 1
    public float OrthographicSize;

    // Viewport
    public Vector2i Viewport;

    /// Translation
    public Matrix4 View;

    /// Orthographic Projection
    public Matrix4 Projection;

    /// View * Projection
    public Matrix4 ViewProjection;
}