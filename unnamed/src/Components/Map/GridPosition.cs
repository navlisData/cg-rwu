using OpenTK.Mathematics;

namespace unnamed.Components.Map;

public readonly struct GridPosition(int x, int y, int z)
{
    public readonly int X = x;

    public readonly int Y = y;

    public readonly int Z = z;

    public GridPosition(Vector3i position) : this(position.X, position.Y, position.Z) { }

    public static implicit operator GridPosition(Vector3i other)
    {
        return new GridPosition(other);
    }

    public static implicit operator Vector3i(GridPosition self)
    {
        return new Vector3i(self.X, self.Y, self.Z);
    }
}