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

    /// <summary>
    ///     Returns the single entity matching this query in the specified world.
    /// </summary>
    /// <remarks>
    ///     This method enforces that exactly one entity matches the query.
    /// </remarks>
    /// <param name="world">The ECS world to query.</param>
    /// <returns>
    ///     The single matching entity.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no entities match the query, or when more than one entity matches.
    /// </exception>
    public Entity Single(World world)
    {
        EntityEnumerator e = this.AsEnumerator(world);

        if (!e.MoveNext())
        {
            throw new InvalidOperationException("Query returned no entities.");
        }

        Entity result = e.Current;

        if (e.MoveNext())
        {
            throw new InvalidOperationException("Query returned more than one entity.");
        }

        return result;
    }

    /// <summary>
    ///     Attempts to retrieve the single entity matching this query in the specified world.
    /// </summary>
    /// <remarks>
    ///     This method succeeds only if exactly one entity matches the query.
    ///     It does not throw on failure conditions.
    /// </remarks>
    /// <param name="world">The ECS world to query.</param>
    /// <param name="entity">
    ///     When this method returns, contains the matching entity if exactly one was found;
    ///     otherwise, the default <see cref="Entity" /> value.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if exactly one entity matches the query; otherwise <see langword="false" />.
    /// </returns>
    public bool TrySingle(World world, out Entity entity)
    {
        EntityEnumerator e = this.AsEnumerator(world);

        if (!e.MoveNext())
        {
            entity = default;
            return false;
        }

        Entity first = e.Current;

        if (e.MoveNext())
        {
            entity = default;
            return false;
        }

        entity = first;
        return true;
    }
}