using Engine.Ecs;

namespace unnamed.Components.Physics;

public struct Follows
{
    /// The target entity. The target is required to have a
    /// <see cref="Position" />
    /// component
    public Entity Target;

    /// The speed factor controlling how quickly the entity moves toward the target position each frame
    public float LerpSpeed;
}