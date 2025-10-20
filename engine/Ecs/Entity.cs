using System.Runtime.CompilerServices;

namespace esc_test.Engine.Ecs;

/// <summary>
/// Lightweight, immutable handle to an entity inside a specific <see cref="World"/>.
/// The handle is validated via (Id, Version) before each operation to guard against use-after-destroy.
/// </summary>
public readonly struct Entity
{
    /// <summary>
    /// Owning world (internal).
    /// </summary>
    internal readonly World World;

    /// <summary>
    /// Numeric identifier within the world.
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// Version counter used to invalidate stale handles when entities are destroyed and IDs recycled.
    /// </summary>
    public readonly int Version;

    /// <summary>
    /// Creates a new entity handle. Internal: use <see cref="World.CreateEntity"/> to obtain valid handles.
    /// </summary>
    internal Entity(World world, int id, int version)
    {
        World = world;
        Id = id;
        Version = version;
    }

    /// <summary>
    /// Returns <c>true</c> if this handle currently refers to a live entity in <see cref="World"/>.
    /// </summary>
    public bool IsAlive => World.IsAlive(Id, Version);

    /// <summary>
    /// Gets a by-ref reference to component <typeparamref name="T"/> for this entity.
    /// Validates the handle before access.
    /// </summary>
    /// <remarks>
    /// The returned reference aliases pool storage and can be invalidated by structural changes
    /// (e.g., removing this component, pool resizes). Do not cache beyond immediate use.
    /// </remarks>
    /// <typeparam name="T">Component type.</typeparam>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if the handle is invalid or the component is missing.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T Get<T>() where T : struct
    {
        World.Validate(this);
        return ref World.GetPool<T>().GetRef(Id);
    }

    /// <summary>
    /// Returns <c>true</c> if this entity currently has component <typeparamref name="T"/>.
    /// Validates the handle before access.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T>() where T : struct
    {
        World.Validate(this);
        return World.GetPool<T>().Has(Id);
    }

    /// <summary>
    /// Adds (or overwrites) component <typeparamref name="T"/> for this entity.
    /// Validates the handle before mutation.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="value">Component value; default creates an uninitialized struct.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Add<T>(in T value = default) where T : struct
    {
        World.Validate(this);
        World.GetPool<T>().Add(Id, value);
    }

    /// <summary>
    /// Removes component <typeparamref name="T"/> from this entity if present.
    /// Validates the handle before mutation.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Remove<T>() where T : struct
    {
        World.Validate(this);
        World.GetPool<T>().Remove(Id);
    }
}
