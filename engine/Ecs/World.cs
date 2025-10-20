using System;
using System.Collections.Generic;
using esc_test.Engine.Ecs.Pools;
using esc_test.Engine.Ecs.Querying;

namespace esc_test.Engine.Ecs;

/// <summary>
/// ECS world: owns entity lifecycle and component pools, and provides query building.
/// </summary>
/// <remarks>
/// - Not thread-safe.
/// - Entities are identified by integer IDs; a per-ID version counter prevents stale handles.
/// - Component pools use sparse-set storage and are instantiated lazily per component type.
/// </remarks>
public sealed class World
{
    private readonly Dictionary<Type, IComponentPool> _pools = new();
    private readonly List<int> _freeIds = new();
    private int[] _versions = new int[256];
    private bool[] _alive = new bool[256];
    private int _nextId = 0;

    /// <summary>
    /// Creates a new entity and returns its handle.
    /// </summary>
    public Entity CreateEntity()
    {
        int id = _freeIds.Count > 0 ? PopFreeId() : _nextId++;
        EnsureCapacityForId(id);
        _alive[id] = true;
        return new Entity(this, id, _versions[id]);
    }

    /// <summary>
    /// Destroys the given entity, removes all of its components, invalidates existing handles by bumping the version,
    /// and recycles the ID.
    /// </summary>
    /// <param name="e">Entity to destroy. Must be a valid handle.</param>
    public void DestroyEntity(in Entity e)
    {
        Validate(e);
        foreach (var pool in _pools.Values)
            if (pool.Has(e.Id)) pool.Remove(e.Id);

        _alive[e.Id] = false;
        _versions[e.Id]++; // invalidate stale handles
        _freeIds.Add(e.Id);
    }

    /// <summary>
    /// Returns whether the given (id, version) pair refers to a currently live entity.
    /// </summary>
    public bool IsAlive(int id, int version)
        => id < _alive.Length && _alive[id] && _versions[id] == version;

    /// <summary>
    /// Throws if the handle is not valid in this world at this time.
    /// </summary>
    /// <param name="e">Entity handle to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown if the handle is invalid.</exception>
    internal void Validate(in Entity e)
    {
        if (!IsAlive(e.Id, e.Version))
            throw new InvalidOperationException($"Invalid entity handle (Id={e.Id}, Version={e.Version}).");
    }

    /// <summary>
    /// Returns the typed component pool for <typeparamref name="T"/>, creating it on first use.
    /// </summary>
    internal ComponentPool<T> GetPool<T>() where T : struct
    {
        var type = typeof(T);
        if (!_pools.TryGetValue(type, out var pool))
        {
            var created = new ComponentPool<T>(_alive.Length, 128);
            _pools[type] = created;
            return created;
        }
        return (ComponentPool<T>)pool;
    }

    /// <summary>
    /// Returns a non-generic component pool for the given component <see cref="Type"/>.
    /// Uses reflection to construct the closed generic pool on first access (cold path only).
    /// </summary>
    /// <param name="t">Component type.</param>
    internal IComponentPool GetPool(Type t)
    {
        if (_pools.TryGetValue(t, out var pool)) return pool;

        var concrete = typeof(ComponentPool<>).MakeGenericType(t);
        pool = (IComponentPool)Activator.CreateInstance(concrete, _alive.Length, 128)!; // ctor(int,int)
        _pools[t] = pool;
        return pool;
    }

    /// <summary>
    /// Tries to get the current version for a live entity ID without constructing a handle.
    /// </summary>
    /// <param name="id">Entity ID.</param>
    /// <param name="version">Current version if the entity is alive, or 0 otherwise.</param>
    /// <returns><c>true</c> if the entity is alive; otherwise <c>false</c>.</returns>
    internal bool TryGetEntityVersion(int id, out int version)
    {
        if (id < _alive.Length && _alive[id])
        {
            version = _versions[id];
            return true;
        }
        version = 0;
        return false;
    }

    /// <summary>
    /// Pops a recycled entity ID from the free list (LIFO).
    /// </summary>
    private int PopFreeId()
    {
        int i = _freeIds[^1];
        _freeIds.RemoveAt(_freeIds.Count - 1);
        return i;
    }

    /// <summary>
    /// Ensures internal arrays can address the given entity ID, growing capacities if needed,
    /// and updates all existing pools to the new entity capacity.
    /// </summary>
    private void EnsureCapacityForId(int id)
    {
        int needed = id + 1;
        if (needed <= _alive.Length) return;

        int newCap = Math.Max(needed, Math.Max(4, _alive.Length * 2));
        Array.Resize(ref _alive, newCap);
        Array.Resize(ref _versions, newCap);
        foreach (var pool in _pools.Values)
            pool.EnsureEntityCapacity(newCap);
    }

    /// <summary>
    /// Starts a fluent query builder scoped to this world.
    /// </summary>
    public QueryBuilder Query() => new QueryBuilder(this);
}
