using System.Runtime.CompilerServices;

using OpenTK.Mathematics;

namespace unnamed.Components.Map;

public struct GridPosition(int x, int y)
{
    public int X = x;

    public int Y = y;

    public GridPosition(int value) : this(value, value) { }

    public GridPosition(Vector2i position) : this(position.X, position.Y) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2i ToVector2I()
    {
        return new Vector2i(this.X, this.Y);
    }
}