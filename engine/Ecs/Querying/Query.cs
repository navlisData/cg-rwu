namespace Engine.Ecs.Querying;

/// <summary>
///     Immutable description of a component-filter query (required and excluded component types).
///     Use <see cref="AsEnumerator(World)" /> to iterate matching entities allocation-free.
/// </summary>
/// <remarks>
///     Not thread-safe. Structural changes to the world (adds/removes) during enumeration
///     may invalidate the underlying spans used by the enumerator.
/// </remarks>
public sealed class Query
{
    private readonly EntityEnumeratorComparison? compare;
    private readonly Type[] with;
    private readonly Type[][] withAny;
    private readonly Type[] without;

    /// <summary>
    ///     Creates a new query.
    /// </summary>
    /// <param name="with">Component types that must be present on an entity.</param>
    /// <param name="without">Component types that must be absent on an entity.</param>
    /// <param name="compareCallback">Comparison function to sort entities before enumeration</param>
    internal Query(Type[] with, Type[] without, Type[][] withAny, EntityEnumeratorComparison? compareCallback)
    {
        this.with = with;
        this.without = without;
        this.withAny = withAny;
        this.compare = compareCallback;
    }

    /// <summary>
    ///     Returns a stack-only enumerator over entities matching this query in the given world.
    /// </summary>
    /// <param name="world">The ECS world to enumerate.</param>
    public EntityEnumerator AsEnumerator(World world)
    {
        return new EntityEnumerator(world, this.with, this.without, this.withAny, this.compare);
    }
}