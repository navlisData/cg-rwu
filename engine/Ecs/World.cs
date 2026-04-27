using System.Runtime.CompilerServices;

using Engine.Ecs.Pools;

namespace Engine.Ecs;

/// <summary>
///     ECS world: owns entity lifecycle and component pools, and provides query building.
/// </summary>
/// <remarks>
///     - Not thread-safe.
///     - Entities are identified by integer IDs; a per-ID version counter prevents stale handles.
///     - Component pools use sparse-set storage and are instantiated lazily per component type.
/// </remarks>
public sealed class World
{
    private readonly List<int> freeIds = new();
    private readonly Dictionary<Type, IComponentPool> pools = new();
    private readonly Dictionary<Type, object> resources = new();
    private bool[] alive = new bool[256];
    private int nextId;
    private int[] versions = new int[256];

    /// <summary>
    ///     Creates a new entity and returns its identity.
    /// </summary>
    public Entity CreateEntity()
    {
        int id = this.freeIds.Count > 0 ? this.PopFreeId() : this.nextId++;
        this.EnsureCapacityForId(id);
        this.alive[id] = true;
        return new Entity(id, this.versions[id]);
    }

    /// <summary>
    ///     Creates a new entity and returns a convenience handle bound to this world.
    /// </summary>
    /// <returns>A handle that can be used to add/remove/get components fluently.</returns>
    public EntityHandle Create()
    {
        Entity entity = this.CreateEntity();
        return new EntityHandle(this, entity);
    }

    /// <summary>
    ///     Creates a stack-only handle for ergonomic entity operations in this world.
    /// </summary>
    /// <param name="e">Entity identity.</param>
    /// <returns>A stack-only handle bound to this world.</returns>
    public EntityHandle Handle(in Entity e)
    {
        return new EntityHandle(this, in e);
    }

    /// <summary>
    ///     Returns whether the given entity identity refers to a currently live entity in this world.
    /// </summary>
    /// <param name="e">Entity identity.</param>
    /// <returns><c>true</c> if alive and version matches; otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAlive(in Entity e)
    {
        return this.IsAlive(e.Id, e.Version);
    }

    /// <summary>
    ///     Throws if the entity identity is not valid in this world at this time.
    /// </summary>
    /// <param name="e">Entity identity to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown if the identity is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Validate(in Entity e)
    {
        if (!this.IsAlive(e.Id, e.Version))
        {
            throw new InvalidOperationException($"Invalid entity handle (Id={e.Id}, Version={e.Version}).");
        }
    }

    /// <summary>
    ///     Gets a by-ref reference to component <typeparamref name="T" /> for the given entity.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="e">Entity identity (Id + Version).</param>
    /// <returns>By-ref reference to the component in pool storage.</returns>
    public ref T Get<T>(Entity e) where T : struct
    {
        this.Validate(e);
        return ref this.GetPool<T>().GetRef(e.Id);
    }

    /// <summary>
    ///     Returns <c>true</c> if the given entity currently has component <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="e">Entity identity.</param>
    /// <returns><c>true</c> if present; otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>(in Entity e) where T : struct
    {
        this.Validate(e);
        return this.GetPool<T>().Has(e.Id);
    }

    /// <summary>
    ///     Adds (or overwrites) component <typeparamref name="T" /> for the given entity.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="e">Entity identity.</param>
    /// <param name="value">Component value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>(in Entity e, in T value = default) where T : struct
    {
        this.Validate(e);
        this.GetPool<T>().Add(e.Id, value);
    }

    /// <summary>
    ///     Removes component <typeparamref name="T" /> from the given entity if present.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="e">Entity identity.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove<T>(in Entity e) where T : struct
    {
        this.Validate(e);
        this.GetPool<T>().Remove(e.Id);
    }

    /// <summary>
    ///     Ensures component <typeparamref name="T" /> exists on the given entity and returns a writable reference to it.
    ///     Validates the identity (Id + Version) before mutation/access.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="e">Entity identity (Id + Version).</param>
    /// <param name="value">Initial value to add when the component is missing.</param>
    /// <returns>Writable reference to the component stored on the entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the identity is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Ensure<T>(Entity e, in T value = default) where T : struct
    {
        this.Validate(e);

        ComponentPool<T> pool = this.GetPool<T>();
        int id = e.Id;

        if (!pool.Has(id))
        {
            pool.Add(id, value);
        }

        return ref pool.GetRef(id);
    }

    /// <summary>
    ///     Destroys the given entity, removes all of its components, invalidates existing handles by bumping the version,
    ///     and recycles the ID.
    /// </summary>
    /// <param name="e">Entity to destroy. Must be a valid handle.</param>
    public void DestroyEntity(in Entity e)
    {
        this.Validate(e);
        foreach (IComponentPool pool in this.pools.Values)
        {
            if (pool.Has(e.Id))
            {
                pool.Remove(e.Id);
            }
        }

        this.alive[e.Id] = false;
        this.versions[e.Id]++; // invalidate stale handles
        this.freeIds.Add(e.Id);
    }

    /// <summary>
    ///     Returns whether the given (id, version) pair refers to a currently live entity.
    /// </summary>
    public bool IsAlive(int id, int version)
    {
        return id < this.alive.Length && this.alive[id] && this.versions[id] == version;
    }

    /// <summary>
    ///     Returns the typed component pool for <typeparamref name="T" />, creating it on first use.
    /// </summary>
    internal ComponentPool<T> GetPool<T>() where T : struct
    {
        Type type = typeof(T);
        if (!this.pools.TryGetValue(type, out IComponentPool? pool))
        {
            ComponentPool<T> created = new(this.alive.Length);
            this.pools[type] = created;
            return created;
        }

        return (ComponentPool<T>)pool;
    }

    /// <summary>
    ///     Returns a non-generic component pool for the given component <see cref="Type" />.
    ///     Uses reflection to construct the closed generic pool on first access (cold path only).
    /// </summary>
    /// <param name="t">Component type.</param>
    internal IComponentPool GetPool(Type t)
    {
        if (this.pools.TryGetValue(t, out IComponentPool? pool))
        {
            return pool;
        }

        Type concrete = typeof(ComponentPool<>).MakeGenericType(t);
        pool = (IComponentPool)Activator.CreateInstance(concrete, this.alive.Length, 128)!; // ctor(int,int)
        this.pools[t] = pool;
        return pool;
    }

    /// <summary>
    ///     Tries to get the current version for a live entity ID without constructing a handle.
    /// </summary>
    /// <param name="id">Entity ID.</param>
    /// <param name="version">Current version if the entity is alive, or 0 otherwise.</param>
    /// <returns><c>true</c> if the entity is alive; otherwise <c>false</c>.</returns>
    internal bool TryGetEntityVersion(int id, out int version)
    {
        if (id < this.alive.Length && this.alive[id])
        {
            version = this.versions[id];
            return true;
        }

        version = 0;
        return false;
    }

    /// <summary>
    ///     Pops a recycled entity ID from the free list (LIFO).
    /// </summary>
    private int PopFreeId()
    {
        int i = this.freeIds[^1];
        this.freeIds.RemoveAt(this.freeIds.Count - 1);
        return i;
    }

    /// <summary>
    ///     Ensures internal arrays can address the given entity ID, growing capacities if needed,
    ///     and updates all existing pools to the new entity capacity.
    /// </summary>
    private void EnsureCapacityForId(int id)
    {
        int needed = id + 1;
        if (needed <= this.alive.Length)
        {
            return;
        }

        int newCap = Math.Max(needed, Math.Max(4, this.alive.Length * 2));
        Array.Resize(ref this.alive, newCap);
        Array.Resize(ref this.versions, newCap);
        foreach (IComponentPool pool in this.pools.Values)
        {
            pool.EnsureEntityCapacity(newCap);
        }
    }

    /// <summary>
    ///     Adds a new resource of type <typeparamref name="T" /> with the specified initial value.
    /// </summary>
    /// <typeparam name="T">The resource type. Only one instance per type can be stored.</typeparam>
    /// <param name="value">The initial value of the resource.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when a resource of type <typeparamref name="T" /> is already registered.
    /// </exception>
    public void AddResource<T>(T value) where T : struct
    {
        Type type = typeof(T);

        if (this.resources.ContainsKey(type))
        {
            throw new InvalidOperationException($"Type {type} already registered.");
        }

        this.resources[type] = new BoxRes<T> { Value = value };
    }

    /// <summary>
    ///     Adds a resource of type <typeparamref name="T" /> or updates the existing value if one is already registered.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="value">The value to store.</param>
    public void AddOrUpdateResource<T>(T value) where T : struct
    {
        Type type = typeof(T);

        if (this.resources.ContainsKey(type))
        {
            ref T res = ref this.GetResource<T>();
            res = value;
        }
        else
        {
            this.resources[type] = new BoxRes<T> { Value = value };
        }
    }

    /// <summary>
    ///     Gets a mutable reference to the resource of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <returns>
    ///     A reference to the stored resource. Mutating the returned value updates the stored instance.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when no resource of type <typeparamref name="T" /> is registered.
    /// </exception>
    public ref T GetResource<T>() where T : struct
    {
        if (!this.resources.TryGetValue(typeof(T), out object? obj))
        {
            throw new KeyNotFoundException($"Type {typeof(T)} not found.");
        }

        return ref ((BoxRes<T>)obj).Value;
    }

    /// <summary>
    ///     Attempts to get the resource of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="value">
    ///     When this method returns, contains a copy of the resource if found; otherwise, the default value of
    ///     <typeparamref name="T" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the resource exists; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This method returns a copy of the resource. Use <see cref="GetResource{T}" /> to obtain a mutable reference.
    /// </remarks>
    public bool TryGetResource<T>(out T value) where T : struct
    {
        if (this.resources.TryGetValue(typeof(T), out object? obj))
        {
            value = ((BoxRes<T>)obj).Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    ///     Removes the resource of type <typeparamref name="T" />, if it exists.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    public void RemoveResource<T>() where T : struct
    {
        this.resources.Remove(typeof(T));
    }

    /// <summary>
    ///     Determines whether a resource of type <typeparamref name="T" /> is registered.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <returns>
    ///     <see langword="true" /> if a resource of the specified type exists; otherwise, <see langword="false" />.
    /// </returns>
    public bool ContainsResource<T>() where T : struct
    {
        return this.resources.ContainsKey(typeof(T));
    }

    private class BoxRes<T> where T : struct
    {
        public T Value;
    }
}