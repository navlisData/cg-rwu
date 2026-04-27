using Engine.Ecs;

namespace unnamed.Components.Physics;

public struct Follows
{
    /// The target entity. The target is required to have a
    /// <see cref="Position" />
    /// component
    public Entity Target;

    /// <summary>
    ///     The radius around the target to start following behavior
    /// </summary>
    public float FollowRadius;

    /// The speed factor controlling how quickly the entity moves toward the target position each frame
    public float Speed;
}