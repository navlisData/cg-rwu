using Engine.Ecs;

namespace unnamed.Components.General;

/// <summary>
///     Indicates that an entity is owned by another entity (e.g., UI elements owned by a player).
///     Used for lifecycle cleanup when the owner is destroyed.
/// </summary>
public struct OwnedBy
{
    public Entity Owner;
}