using Engine.Ecs.Pools;

namespace Engine.Ecs.Querying;

/// <summary>
/// Stack-only enumerator over entities that match a given query (With / Without component constraints).
/// Uses a sparse-set pool with the smallest population as the iteration anchor to minimize work,
/// then validates each candidate against the remaining constraints.
/// </summary>
/// <remarks>
/// - Not thread-safe.
/// - This is a <c>ref struct</c> and therefore stack-only (cannot be boxed, captured, or stored on the heap).
/// - The underlying dense spans become invalid if structural modifications (add/remove/resize) occur
///   in any of the involved pools during enumeration.
/// </remarks>
public ref struct EntityEnumerator
{
    private readonly World world;
    private readonly Type[] with;
    private readonly Type[] without;
    private readonly IComponentPool anchorPool;
    private readonly ReadOnlySpan<int> anchorDense;

    private int i;

    /// <summary>
    /// The entity at the current iterator position. Only valid after <see cref="MoveNext"/> returned <c>true</c>.
    /// </summary>
    public Entity Current { get; private set; }

    /// <summary>
    /// Constructs an enumerator for the specified world and component constraints.
    /// </summary>
    /// <param name="world">The ECS world.</param>
    /// <param name="with">Component types that must be present on the entity.</param>
    /// <param name="without">Component types that must be absent on the entity.</param>
    /// <exception cref="InvalidOperationException">Thrown if no required component types are provided.</exception>
    public EntityEnumerator(World world, Type[] with, Type[] without)
    {
        this.world   = world   ?? throw new ArgumentNullException(nameof(world));
        this.with    = with    ?? throw new ArgumentNullException(nameof(with));
        this.without = without ?? throw new ArgumentNullException(nameof(without));

        if (with.Length == 0)
            throw new InvalidOperationException("Query requires at least one With<T>().");

        // Choose the smallest 'with' pool as the iteration anchor to reduce candidate checks.
        IComponentPool? best = null;
        int bestCount = int.MaxValue;
        foreach (var t in with)
        {
            var pool = this.world.GetPool(t);
            int cnt = pool.Count;
            if (cnt < bestCount) { best = pool; bestCount = cnt; }
        }

        IComponentPool anchorPool = best!;
        anchorDense = anchorPool.DenseEntitySpan;
        i = -1;
        Current = default;
    }

    /// <summary>
    /// Returns the enumerator itself. Enables <c>foreach</c> over the query.
    /// </summary>
    public EntityEnumerator GetEnumerator() => this;

    /// <summary>
    /// Advances the enumerator to the next entity that satisfies all constraints.
    /// </summary>
    /// <returns><c>true</c> if a matching entity was found; otherwise <c>false</c>.</returns>
    public bool MoveNext()
    {
        while (++i < anchorDense.Length)
        {
            int id = anchorDense[i];

            bool ok = true;
            foreach (var t in with)
                if (!world.GetPool(t).Has(id)) { ok = false; break; }

            if (!ok) continue;

            foreach (var t in without)
                if (world.GetPool(t).Has(id)) { ok = false; break; }

            if (!ok) continue;

            // Ensure the entity handle is valid at enumeration time.
            if (world.TryGetEntityVersion(id, out int version))
            {
                Current = new Entity(world, id, version);
                return true;
            }
        }
        return false;
    }
}
