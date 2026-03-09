using System.Diagnostics.Contracts;

using OpenTK.Mathematics;

using unnamed.Utils;

namespace unnamed.Components.UI;

/// <summary>
///     Represents a fixed screen position measured in pixels.
///     Coordinates may be negative, allowing positions that are relative to the bottom or right edges.
/// </summary>
/// <param name="x">Horizontal offset from the top-left corner.</param>
/// <param name="y">Vertical offset from the top-left corner.</param>
public struct AbsolutePosition(float x, float y, bool allowWrapping = true)
{
    public float X = x;
    public float Y = y;
    public readonly bool AllowWrapping = allowWrapping;

    public static explicit operator AbsolutePosition(Vector2 pos)
    {
        return new AbsolutePosition(pos.X, pos.Y);
    }

    public static explicit operator Vector2(AbsolutePosition pos)
    {
        return new Vector2(pos.X, pos.Y);
    }

    public AbsolutePosition WrapToScreen(Vector2i windowSize)
    {
        return new AbsolutePosition(
            MathUtils.Wrap(this.X, windowSize.X),
            MathUtils.Wrap(this.Y, windowSize.Y));
    }

    [Pure]
    public static AbsolutePosition operator +(AbsolutePosition left, in Vector2 right)
    {
        left.X += right.X;
        left.Y += right.Y;
        return left;
    }

    [Pure]
    public static AbsolutePosition operator -(AbsolutePosition left, in Vector2 right)
    {
        left.X -= right.X;
        left.Y -= right.Y;
        return left;
    }
}