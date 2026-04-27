using Engine.Ecs.Querying;

namespace Engine.Ecs.Systems;

/// <summary>
///     Base class for systems that operate on a set of entities selected by a <see cref="Query" />.
///     Iterates allocation-free using the query's <see cref="EntityEnumerator" /> and calls <see cref="Update" />
///     for each matching entity.
/// </summary>
/// <typeparam name="T">
///     Any context payload passed to <see cref="Run(T)" /> and
///     <see cref="Update(T, in Entity)" />.
/// </typeparam>
/// <remarks>
///     - Not thread-safe.
///     - Avoid structural changes that would invalidate the anchor pool during iteration
///     (e.g., removing a component that is part of the query's required set on the current entity),
///     as this can perturb enumeration order. Non-anchor mutations are typically safe.
/// </remarks>
public abstract class EntitySetSystem<T> : BaseSystem<T>
{
    private readonly Query query;

    /// <summary>
    ///     Creates a new system bound to a world and a precompiled query.
    /// </summary>
    /// <param name="world">The ECS world.</param>
    /// <param name="query">The entity filter used by this system.</param>
    protected EntitySetSystem(World world, Query query) : base(world)
    {
        this.query = query ?? throw new ArgumentNullException(nameof(query));
    }

    /// <summary>
    ///     Executes the system once for all entities matching the query.
    /// </summary>
    /// <param name="context">Context payload forwarded to <see cref="Update(T, in Entity)" />.</param>
    public override void Run(T context)
    {
        EntityEnumerator it = this.query.AsEnumerator(this.world);

        foreach (Entity e in it)
        {
            this.Update(context, in e);
        }
    }

    /// <summary>
    ///     Per-entity update hook. Implement system behavior here.
    /// </summary>
    /// <param name="context">Context payload.</param>
    /// <param name="e">Current entity (validated handle).</param>
    protected abstract void Update(T context, in Entity e);
}