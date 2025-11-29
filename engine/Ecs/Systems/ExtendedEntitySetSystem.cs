using Engine.Ecs.Querying;

namespace Engine.Ecs.Systems;

/// <summary>
///     Base class for systems that operate on a set of entities selected by a <see cref="Query" />.
///     Iterates allocation-free using the query's <see cref="EntityEnumerator" /> and calls <see cref="Update" />
///     for each matching entity.
/// </summary>
/// <typeparam name="TSetup">
///     Any context payload passed to <see cref="Run(TSetup, TUpdate)" /> and
///     <see cref="BeforeUpdate(TSetup)" />.
/// </typeparam>
/// <typeparam name="TUpdate">
///     Any context payload passed to <see cref="Run(TSetup, TUpdate)" /> and
///     <see cref="Update(TUpdate, in Entity)" />.
/// </typeparam>
/// <remarks>
///     - Not thread-safe.
///     - Avoid structural changes that would invalidate the anchor pool during iteration
///     (e.g., removing a component that is part of the query's required set on the current entity),
///     as this can perturb enumeration order. Non-anchor mutations are typically safe.
/// </remarks>
public abstract class ExtendedEntitySetSystem<TSetup, TUpdate>(World world, Query query)
    : EntitySetSystem<TUpdate>(world, query)
{
    private readonly Query query = query ?? throw new ArgumentNullException(nameof(query));

    /// <summary>
    ///     The ECS world this system operates on.
    /// </summary>
    protected new readonly World world = world ?? throw new ArgumentNullException(nameof(world));

    /// <summary>
    ///     Set to <c>false</c> to skip further updates
    /// </summary>
    protected bool doUpdate = true;

    /// <summary>
    ///     Executes the system once for all entities matching the query.
    /// </summary>
    /// <param name="setupContext">Context payload forwarded to <see cref="BeforeUpdate(TSetup)" />.</param>
    /// <param name="updateContext">Context payload forwarded to <see cref="Update(TUpdate, in Entity)" />.</param>
    public void Run(TSetup? setupContext, TUpdate? updateContext)
    {
        this.doUpdate = true;
        this.BeforeUpdate(setupContext);

        EntityEnumerator it = this.query.AsEnumerator(this.world);
        foreach (Entity e in it)
        {
            if (!this.doUpdate) { return; }

            this.Update(updateContext, in e);
        }
    }

    /// <summary>
    ///     Per-entity update hook.
    /// </summary>
    /// <param name="context">Context payload.</param>
    /// <param name="e">Current entity (validated handle).</param>
    protected abstract override void Update(TUpdate? context, in Entity e);

    /// <summary>
    ///     Per-update hook. Runs before any update.
    /// </summary>
    /// <param name="context">Context payload</param>
    protected abstract void BeforeUpdate(TSetup? context);
}