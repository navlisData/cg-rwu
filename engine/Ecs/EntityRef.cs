namespace Engine.Ecs;

/// <summary>
///     Lightweight entity reference that can be stored inside components
/// </summary>
public readonly struct EntityRef
{
    /// <summary>
    ///     Sentinel value that represents an unbound / invalid reference.
    /// </summary>
    public static readonly EntityRef Invalid = new(-1, -1);

    public readonly int Id;
    public readonly int Version;

    /// <summary>
    ///     Creates a reference from a concrete entity id/version pair.
    /// </summary>
    /// <param name="id">Entity identifier.</param>
    /// <param name="version">Entity version.</param>
    public EntityRef(int id, int version)
    {
        this.Id = id;
        this.Version = version;
    }

    /// <summary>
    ///     Returns <c>true</c> if this reference is not the invalid sentinel.
    /// </summary>
    public bool IsValid => Id >= 0 && Version >= 0;
}