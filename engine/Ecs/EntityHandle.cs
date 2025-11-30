using System.Runtime.CompilerServices;

namespace Engine.Ecs;

/// <summary>
///     Lightweight convenience wrapper combining a world context with an entity identity.
/// </summary>
/// <remarks>
///     This type is intended to be short-lived. It enables ergonomic fluent setup while keeping
///     the persisted data model clean: components store only <see cref="Entity"/> (Id + Version).
/// </remarks>
public readonly struct EntityHandle
{
    private readonly World world;
    private readonly Entity entity;

    /// <summary>
    ///     Creates a new handle bound to a specific world and entity identity.
    /// </summary>
    /// <param name="world">Owning world.</param>
    /// <param name="entity">Entity identity (Id + Version).</param>
    internal EntityHandle(World world, in Entity entity)
    {
        this.world = world ?? throw new ArgumentNullException(nameof(world));
        this.entity = entity;
    }

    /// <summary>
    ///     Extracts the underlying entity identity (Id + Version).
    /// </summary>
    /// <returns>The entity identity.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity ToEntity() => this.entity;

    /// <summary>
    ///     Returns whether this handle currently points to a live entity.
    /// </summary>
    /// <returns><c>true</c> if the entity is alive; otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAlive() => this.world.IsAlive(this.entity);

    /// <summary>
    ///     Gets a by-ref reference to component <typeparamref name="T" /> for this entity.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <returns>By-ref component reference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get<T>() where T : struct => ref this.world.Get<T>(this.entity);

    /// <summary>
    ///     Adds (or overwrites) component <typeparamref name="T" /> for this entity.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="value">Component value.</param>
    /// <returns>A copy of this handle to enable fluent chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityHandle Add<T>(in T value = default) where T : struct
    {
        this.world.Add(this.entity, value);
        return this;
    }

    /// <summary>
    ///     Removes component <typeparamref name="T" /> for this entity if present.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <returns>A copy of this handle to enable fluent chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityHandle Remove<T>() where T : struct
    {
        this.world.Remove<T>(this.entity);
        return this;
    }

    /// <summary>
    ///     Returns whether this entity currently has component <typeparamref name="T" />.
    ///     Validates the identity (Id + Version) before access.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <returns><c>true</c> if the component is present; otherwise <c>false</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the identity is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct => this.world.Has<T>(this.entity);

    /// <summary>
    ///     Ensures component <typeparamref name="T" /> exists on this entity and returns a writable reference to it.
    ///     Validates the identity (Id + Version) before mutation/access.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="value">Initial value to add when the component is missing.</param>
    /// <returns>Writable reference to the component stored on the entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the identity is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Ensure<T>(in T value = default) where T : struct
        => ref this.world.Ensure<T>(this.entity, value);

    /// <summary>
    ///     Destroys this entity in the owning world.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy() => this.world.DestroyEntity(this.entity);
}