using Engine.Ecs.Pools;

namespace Engine.Ecs.Querying;

/// <summary>
///     Stack-only enumerator over entities that match a given query (With / Without component constraints).
///     Uses a sparse-set pool with the smallest population as the iteration anchor to minimize work,
///     then validates each candidate against the remaining constraints.
/// </summary>
/// <remarks>
///     - Not thread-safe.
///     - This is a <c>ref struct</c> and therefore stack-only (cannot be boxed, captured, or stored on the heap).
///     - The underlying dense spans become invalid if structural modifications (add/remove/resize) occur
///     in any of the involved pools during enumeration.
/// </remarks>
public ref struct EntityEnumerator
{
    private readonly World world;
    private readonly Type[] with;
    private readonly Type[] without;
    private readonly ReadOnlySpan<int> anchorDense;
    private readonly EntityEnumeratorComparison? compare;

    private Span<int> sortedIds;
    private int sortedCount;
    private bool initialized;

    private int i;

    /// <summary>
    ///     The entity at the current iterator position. Only valid after <see cref="MoveNext" /> returned <c>true</c>.
    /// </summary>
    public Entity Current { get; private set; }

    /// <summary>
    ///     Constructs an enumerator for the specified world and component constraints.
    /// </summary>
    /// <param name="world">The ECS world.</param>
    /// <param name="with">Component types that must be present on the entity.</param>
    /// <param name="without">Component types that must be absent on the entity.</param>
    /// <param name="compare">
    ///     Comparison function to sort entities before enumeration
    ///     <c>Beware that this allocates an extra id list on the heap that is used for sorting</c>
    /// </param>
    /// <exception cref="InvalidOperationException">Thrown if an empty <see cref="with" /> parameter is provided.</exception>
    public EntityEnumerator(World world, Type[] with, Type[] without, EntityEnumeratorComparison? compare)
    {
        this.world = world;
        this.with = with;
        this.without = without;
        this.compare = compare;

        if (with.Length == 0)
        {
            throw new InvalidOperationException("Query requires at least one With<T>().");
        }

        // Choose the smallest 'with' pool as the iteration anchor to reduce candidate checks.
        IComponentPool? best = null;
        int bestCount = int.MaxValue;
        foreach (Type t in with)
        {
            IComponentPool pool = this.world.GetPool(t);
            int cnt = pool.Count;
            if (cnt >= bestCount)
            {
                continue;
            }

            best = pool;
            bestCount = cnt;
        }

        IComponentPool anchorPool = best!;
        this.anchorDense = anchorPool.DenseEntitySpan;
        this.i = -1;
        this.Current = default;
    }

    /// <summary>
    ///     Returns the enumerator itself. Enables <c>foreach</c> over the query.
    /// </summary>
    public EntityEnumerator GetEnumerator()
    {
        return this;
    }

    /// <summary>
    ///     Advances the enumerator to the next entity that satisfies all constraints.
    /// </summary>
    /// <returns><c>true</c> if a matching entity was found; otherwise <c>false</c>.</returns>
    public bool MoveNext()
    {
        // Fast path: no sorting
        if (this.compare == null)
        {
            return this.MoveNextUnsorted();
        }

        // Sorted path (lazy init)
        if (!this.initialized)
        {
            this.BuildAndSort();
            this.initialized = true;
            this.i = -1;
        }

        if (++this.i < this.sortedCount)
        {
            int id = this.sortedIds[this.i];
            this.world.TryGetEntityVersion(id, out int version);
            this.Current = new Entity(id, version);
            return true;
        }

        return false;
    }

    private bool Matches(int id)
    {
        foreach (Type t in this.with)
        {
            if (!this.world.GetPool(t).Has(id))
            {
                return false;
            }
        }

        foreach (Type t in this.without)
        {
            if (this.world.GetPool(t).Has(id))
            {
                return false;
            }
        }

        return true;
    }

    private void BuildAndSort()
    {
        int max = this.anchorDense.Length;

        // Allocates enough space for the largest possible set to fit
        // Stackalloc might be better for smaller sets but not possible here due to lifetime limitations
        Span<int> buffer = new int[max];

        int count = 0;

        foreach (int id in this.anchorDense)
        {
            if (!this.Matches(id))
            {
                continue;
            }

            if (!this.world.TryGetEntityVersion(id, out _))
            {
                continue;
            }

            buffer[count++] = id;
        }

        EntityEnumeratorComparison cmp = this.compare!;
        World? localWorld = this.world;
        buffer[..count].Sort((a, b) =>
        {
            localWorld.TryGetEntityVersion(a, out int va);
            localWorld.TryGetEntityVersion(b, out int vb);
            return cmp(new Entity(a, va), new Entity(b, vb), localWorld);
        });

        this.sortedIds = buffer;
        this.sortedCount = count;
    }

    private bool MoveNextUnsorted()
    {
        while (++this.i < this.anchorDense.Length)
        {
            int id = this.anchorDense[this.i];

            bool ok = true;
            foreach (Type t in this.with)
            {
                if (this.world.GetPool(t).Has(id))
                {
                    continue;
                }

                ok = false;
                break;
            }

            if (!ok)
            {
                continue;
            }

            foreach (Type t in this.without)
            {
                if (!this.world.GetPool(t).Has(id))
                {
                    continue;
                }

                ok = false;
                break;
            }

            if (!ok)
            {
                continue;
            }

            // Ensure the entity handle is valid at enumeration time.
            if (!this.world.TryGetEntityVersion(id, out int version))
            {
                continue;
            }

            this.Current = new Entity(id, version);
            return true;
        }

        return false;
    }
}

public delegate int EntityEnumeratorComparison(Entity x, Entity y, World world);