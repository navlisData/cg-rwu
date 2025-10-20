using System;
using System.Collections.Generic;

namespace esc_test.Engine.Ecs.Querying;

/// <summary>
/// Fluent builder for composing a component-filter query with required (<c>With</c>)
/// and excluded (<c>Without</c>) component types. Produces an immutable <see cref="Query"/>.
/// </summary>
/// <remarks>
/// - Not thread-safe.
/// - Intended to be short-lived. Build once and discard the builder.
/// - The provided <see cref="World"/> is kept for potential future world-scoped
///   extensions; it is not used by the current implementation.
/// </remarks>
public sealed class QueryBuilder
{
    private readonly World _world;
    private readonly List<Type> _with = new();
    private readonly List<Type> _without = new();

    /// <summary>
    /// Creates a new builder bound to the given world.
    /// </summary>
    /// <param name="world">ECS world that will be queried.</param>
    internal QueryBuilder(World world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _ = _world; // currently unused; reserved for future world-scoped features
    }

    /// <summary>
    /// Adds a required component type constraint. Entities must have <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Component type that must be present.</typeparam>
    /// <returns>The same builder instance (for fluent chaining).</returns>
    public QueryBuilder With<T>() where T : struct
    {
        _with.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Adds an excluded component type constraint. Entities must NOT have <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Component type that must be absent.</typeparam>
    /// <returns>The same builder instance (for fluent chaining).</returns>
    public QueryBuilder Without<T>() where T : struct
    {
        _without.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Builds an immutable <see cref="Query"/> snapshot from the current constraints.
    /// Subsequent mutations of this builder will not affect the returned query.
    /// </summary>
    /// <returns>A <see cref="Query"/> capturing the current With/Without sets.</returns>
    public Query Build()
        => new(_with.ToArray(), _without.ToArray());
}
