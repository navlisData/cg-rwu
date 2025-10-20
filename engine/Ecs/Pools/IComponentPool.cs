namespace Engine.Ecs.Pools;

/// <summary>
///     Non-generic contract for sparse-set component pools used by the ECS <c>World</c>.
///     Implementations must provide O(1) operations for add/remove/has and expose a
///     dense view of entity IDs for allocation-free iteration.
/// </summary>
/// <remarks>
///     Not thread-safe. Structural changes (add/remove/resize) invalidate previously
///     retrieved spans from <see cref="DenseEntitySpan" />.
/// </remarks>
internal interface IComponentPool
{
    /// <summary>
    ///     Number of active components stored in the pool (current dense length).
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Read-only span over the dense array of entity IDs for indices
    ///     <c>[0, <see cref="Count" />)</c>. Intended as an iteration anchor to
    ///     minimize lookups and allocations.
    /// </summary>
    /// <remarks>
    ///     The span becomes invalid if the pool performs a structural modification
    ///     (e.g., add/remove/resize). Do not cache beyond the immediate use.
    /// </remarks>
    ReadOnlySpan<int> DenseEntitySpan { get; }

    /// <summary>
    ///     Ensures the sparse address space can index entity IDs in the range
    ///     <c>[0, <paramref name="entityCapacity" />)</c>.
    ///     This may grow internal arrays but must not shrink them.
    /// </summary>
    /// <param name="entityCapacity">Required capacity in entity ID space.</param>
    void EnsureEntityCapacity(int entityCapacity);

    /// <summary>
    ///     Returns <c>true</c> if the specified entity currently has a component in this pool.
    ///     Expected complexity: O(1).
    /// </summary>
    /// <param name="entityId">Entity identifier.</param>
    bool Has(int entityId);

    /// <summary>
    ///     Removes the component for the specified entity if present.
    ///     Must be O(1) using a swap-and-pop strategy on the dense storage.
    /// </summary>
    /// <param name="entityId">Entity identifier.</param>
    void Remove(int entityId);
}