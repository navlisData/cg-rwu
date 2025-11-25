using OpenTK.Mathematics;

namespace unnamed.Components.Physics;

public struct Velocity(Vector2 direction, float speed)
{
    public Vector2 Direction = direction;
    public float Speed = speed;
}