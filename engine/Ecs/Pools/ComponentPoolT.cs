using System.Runtime.CompilerServices;

namespace Engine.Ecs.Pools;

/// <summary>
///     Sparse-set based component pool for a specific component type <typeparamref name="T" />.
///     Provides O(1) add/remove/has/lookup operations by mapping entity IDs to dense indices.
/// </summary>
/// <typeparam name="T">Component value type stored in this pool.</typeparam>
internal sealed class ComponentPool<T> : IComponentPool where T : struct
{
    /// dense[denseIndex] -> entityId for the component stored at _values[denseIndex].
    private int[] dense;

    /// sparse[entityId] -> (denseIndex + 1), or 0 if the entity does not have the component.
    private int[] sparse;

    /// - values[denseIndex] -> component value T for that entity.
    private T[] values;

    /// <summary>
    ///     Creates a new component pool.
    /// </summary>
    /// <param name="initialEntityCapacity">Initial capacity for the sparse array (by entity ID space).</param>
    /// <param name="initialComponentCapacity">Initial capacity for the dense/value arrays (by number of components).</param>
    public ComponentPool(int initialEntityCapacity = 128, int initialComponentCapacity = 128)
    {
        this.sparse = new int[initialEntityCapacity];
        this.dense = new int[initialComponentCapacity];
        this.values = new T[initialComponentCapacity];
        this.Count = 0;
    }

    /// <summary>
    ///     Number of active components stored in the pool (i.e., current dense length).
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Read-only view over the dense entity IDs (valid range: [0, <see cref="Count" />)).
    ///     Useful as an anchor for fast iteration.
    /// </summary>
    public ReadOnlySpan<int> DenseEntitySpan => new(this.dense, 0, this.Count);

    /// <summary>
    ///     Ensures the sparse array can address entity IDs up to <paramref name="entityCapacity" /> - 1.
    ///     Does not allocate values; only grows the sparse address space.
    /// </summary>
    /// <param name="entityCapacity">Required capacity in entity ID space.</param>
    public void EnsureEntityCapacity(int entityCapacity)
    {
        if (entityCapacity > this.sparse.Length)
        {
            Array.Resize(ref this.sparse, Math.Max(entityCapacity, this.sparse.Length * 2));
        }
    }

    /// <summary>
    ///     Returns whether an entity currently has this component.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(int entityId)
    {
        return entityId < this.sparse.Length && this.sparse[entityId] != 0;
    }

    /// <summary>
    ///     Removes the component from the entity if present.
    ///     Uses swap-and-pop to keep the dense arrays compact (O(1)).
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int entityId)
    {
        if (!this.Has(entityId))
        {
            return;
        }

        int idx = this.sparse[entityId] - 1;
        int last = this.Count - 1;

        // Swap last element into the removed slot...
        int movedEntity = this.dense[last];
        this.dense[idx] = movedEntity;
        this.values[idx] = this.values[last];

        // ...and update sparse mapping.
        this.sparse[movedEntity] = idx + 1;
        this.sparse[entityId] = 0;
        this.Count--;
    }

    /// <summary>
    ///     Ensures the dense/value arrays can store at least <paramref name="needed" /> components.
    /// </summary>
    /// <param name="needed">Required number of components.</param>
    private void EnsureComponentCapacity(int needed)
    {
        if (needed > this.values.Length)
        {
            int newCap = Math.Max(needed, this.values.Length * 2);
            Array.Resize(ref this.values, newCap);
            Array.Resize(ref this.dense, newCap);
        }
    }

    /// <summary>
    ///     Returns a by-ref reference to the component value for an entity.
    ///     Throws if the entity does not have this component.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <exception cref="InvalidOperationException">Thrown if the component is missing on the entity.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRef(int entityId)
    {
        int indexPlusOne = this.sparse[entityId];
        if (indexPlusOne == 0)
        {
            throw new InvalidOperationException($"Component {typeof(T).Name} is missing on entity {entityId}.");
        }

        return ref this.values[indexPlusOne - 1];
    }

    /// <summary>
    ///     Adds or overwrites the component value for an entity.
    ///     If the entity already has the component, this acts as an idempotent overwrite.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">Component value to store.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int entityId, in T value)
    {
        if (entityId >= this.sparse.Length)
        {
            this.EnsureEntityCapacity(entityId + 1);
        }

        int indexPlusOne = this.sparse[entityId];
        if (indexPlusOne != 0)
        {
            // Overwrite existing value (idempotent).
            this.values[indexPlusOne - 1] = value;
            return;
        }

        this.EnsureComponentCapacity(this.Count + 1);
        this.dense[this.Count] = entityId;
        this.values[this.Count] = value;
        this.sparse[entityId] = this.Count + 1;
        this.Count++;
    }
}