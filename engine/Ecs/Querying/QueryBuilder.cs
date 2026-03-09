namespace Engine.Ecs.Querying;

/// <summary>
///     Fluent builder for composing a component-filter query with required (<c>With</c>)
///     and excluded (<c>Without</c>) component types. Produces an immutable <see cref="Query" />.
/// </summary>
/// <remarks>
///     - Not thread-safe.
///     - Intended to be short-lived. Build once and discard the builder.
/// </remarks>
public sealed class QueryBuilder
{
    private readonly List<Type> with = new();
    private readonly List<Type[]> withAny = new();
    private readonly List<Type> without = new();
    private EntityEnumeratorComparison? compare;

    /// <summary>
    ///     Adds a required component type constraint. Entities must have <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Component type that must be present.</typeparam>
    /// <returns>The same builder instance (for fluent chaining).</returns>
    public QueryBuilder With<T>() where T : struct
    {
        this.with.Add(typeof(T));
        return this;
    }

    /// <summary>
    ///     Adds an excluded component type constraint. Entities must NOT have <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Component type that must be absent.</typeparam>
    /// <returns>The same builder instance (for fluent chaining).</returns>
    public QueryBuilder Without<T>() where T : struct
    {
        this.without.Add(typeof(T));
        return this;
    }

    public QueryBuilder WithAny<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        this.withAny.Add([typeof(T1), typeof(T2)]);
        return this;
    }

    public QueryBuilder OrderWith(EntityEnumeratorComparison order)
    {
        this.compare = order;
        return this;
    }

    /// <summary>
    ///     Builds an immutable <see cref="Query" /> snapshot from the current constraints.
    ///     Subsequent mutations of this builder will not affect the returned query.
    /// </summary>
    /// <returns>A <see cref="Query" /> capturing the current With/Without sets.</returns>
    public Query Build()
    {
        return new Query(this.with.ToArray(), this.without.ToArray(), this.withAny.ToArray(), this.compare);
    }
}