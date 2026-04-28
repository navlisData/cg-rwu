using OpenTK.Mathematics;

namespace unnamed.Resources;

public struct WindowSize(Vector2i value)
{
    public Vector2i Value = value;

    public static implicit operator Vector2(WindowSize size)
    {
        return size.Value;
    }
}