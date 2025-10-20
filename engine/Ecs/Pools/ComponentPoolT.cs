using System;
using System.Runtime.CompilerServices;

namespace esc_test.Engine.Ecs.Pools;

/// <summary>
/// Sparse-set based component pool for a specific component type <typeparamref name="T"/>.
/// Provides O(1) add/remove/has/lookup operations by mapping entity IDs to dense indices.
/// </summary>
/// <typeparam name="T">Component value type stored in this pool.</typeparam>
internal sealed class ComponentPool<T> : IComponentPool where T : struct
{
    // Sparse-set layout:
    //  - _sparse[entityId] -> (denseIndex + 1), or 0 if the entity does not have the component.
    //  - _dense[denseIndex] -> entityId for the component stored at _values[denseIndex].
    //  - _values[denseIndex] -> component value T for that entity.
    private int[] _sparse;
    private int[] _dense;
    private T[] _values;
    private int _count;

    /// <summary>
    /// Creates a new component pool.
    /// </summary>
    /// <param name="initialEntityCapacity">Initial capacity for the sparse array (by entity ID space).</param>
    /// <param name="initialComponentCapacity">Initial capacity for the dense/value arrays (by number of components).</param>
    public ComponentPool(int initialEntityCapacity = 128, int initialComponentCapacity = 128)
    {
        _sparse = new int[initialEntityCapacity];
        _dense  = new int[initialComponentCapacity];
        _values = new T[initialComponentCapacity];
        _count = 0;
    }

    /// <summary>
    /// Number of active components stored in the pool (i.e., current dense length).
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Read-only view over the dense entity IDs (valid range: [0, <see cref="Count"/>)).
    /// Useful as an anchor for fast iteration.
    /// </summary>
    public ReadOnlySpan<int> DenseEntitySpan => new ReadOnlySpan<int>(_dense, 0, _count);

    /// <summary>
    /// Ensures the sparse array can address entity IDs up to <paramref name="entityCapacity"/> - 1.
    /// Does not allocate values; only grows the sparse address space.
    /// </summary>
    /// <param name="entityCapacity">Required capacity in entity ID space.</param>
    public void EnsureEntityCapacity(int entityCapacity)
    {
        if (entityCapacity > _sparse.Length)
            Array.Resize(ref _sparse, Math.Max(entityCapacity, _sparse.Length * 2));
    }

    /// <summary>
    /// Ensures the dense/value arrays can store at least <paramref name="needed"/> components.
    /// </summary>
    /// <param name="needed">Required number of components.</param>
    private void EnsureComponentCapacity(int needed)
    {
        if (needed > _values.Length)
        {
            int newCap = Math.Max(needed, _values.Length * 2);
            Array.Resize(ref _values, newCap);
            Array.Resize(ref _dense, newCap);
        }
    }

    /// <summary>
    /// Returns whether an entity currently has this component.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(int entityId)
        => entityId < _sparse.Length && _sparse[entityId] != 0;

    /// <summary>
    /// Returns a by-ref reference to the component value for an entity.
    /// Throws if the entity does not have this component.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <exception cref="InvalidOperationException">Thrown if the component is missing on the entity.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRef(int entityId)
    {
        int indexPlusOne = _sparse[entityId];
        if (indexPlusOne == 0)
            throw new InvalidOperationException($"Component {typeof(T).Name} is missing on entity {entityId}.");

        return ref _values[indexPlusOne - 1];
    }

    /// <summary>
    /// Adds or overwrites the component value for an entity.
    /// If the entity already has the component, this acts as an idempotent overwrite.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">Component value to store.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int entityId, in T value)
    {
        if (entityId >= _sparse.Length)
            EnsureEntityCapacity(entityId + 1);

        int indexPlusOne = _sparse[entityId];
        if (indexPlusOne != 0)
        {
            // Overwrite existing value (idempotent).
            _values[indexPlusOne - 1] = value;
            return;
        }

        EnsureComponentCapacity(_count + 1);
        _dense[_count] = entityId;
        _values[_count] = value;
        _sparse[entityId] = _count + 1;
        _count++;
    }

    /// <summary>
    /// Removes the component from the entity if present.
    /// Uses swap-and-pop to keep the dense arrays compact (O(1)).
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int entityId)
    {
        if (!Has(entityId)) return;

        int idx = _sparse[entityId] - 1;
        int last = _count - 1;

        // Swap last element into the removed slot...
        int movedEntity = _dense[last];
        _dense[idx] = movedEntity;
        _values[idx] = _values[last];

        // ...and update sparse mapping.
        _sparse[movedEntity] = idx + 1;
        _sparse[entityId] = 0;
        _count--;
    }
}
