using OpenTK.Mathematics;

namespace unnamed.Components.Map;

public readonly struct GridPosition(int x, int y)
{
    public readonly int X = x;

    public readonly int Y = y;

    public GridPosition(Vector2i position) : this(position.X, position.Y) { }

    public static implicit operator GridPosition(Vector2i other)
    {
        return new GridPosition(other);
    }

    public static implicit operator Vector2i(GridPosition self)
    {
        return new Vector2i(self.X, self.Y);
    }
}