namespace Engine.Ecs;

public readonly struct Entity : IEquatable<Entity>
{
    /// <summary>
    ///     Numeric identifier within the world.
    /// </summary>
    public readonly int Id;

    /// <summary>
    ///     Version counter used to invalidate stale handles when entities are destroyed and IDs recycled.
    /// </summary>
    public readonly int Version;

    /// <summary>
    ///     Creates a new entity identity. Internal: use <see cref="World.CreateEntity" /> to obtain valid values.
    /// </summary>
    internal Entity(int id, int version)
    {
        this.Id = id;
        this.Version = version;
    }

    /// <summary>
    ///     Returns whether this entity identity is equal to another.
    /// </summary>
    /// <param name="other">Other entity.</param>
    /// <returns><c>true</c> if both Id and Version match; otherwise <c>false</c>.</returns>
    public bool Equals(Entity other) => this.Id == other.Id && this.Version == other.Version;

    /// <summary>
    ///     Returns whether this entity identity is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare.</param>
    /// <returns><c>true</c> if the object is an <see cref="Entity" /> with same Id and Version.</returns>
    public override bool Equals(object? obj) => obj is Entity other && this.Equals(other);

    /// <summary>
    ///     Returns a hash code combining Id and Version.
    /// </summary>
    /// <returns>Hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(this.Id, this.Version);

    /// <summary>
    ///     Returns a readable representation of the entity.
    /// </summary>
    /// <returns>String representation.</returns>
    public override string ToString() => $"Entity(Id={this.Id}, Version={this.Version})";
}