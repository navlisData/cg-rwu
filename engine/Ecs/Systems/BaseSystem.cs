namespace Engine.Ecs.Systems;

public abstract class BaseSystem<T>(World world)
{
    /// <summary>
    ///     The ECS world this system operates on.
    /// </summary>
    protected readonly World world = world ?? throw new ArgumentNullException(nameof(world));

    /// <summary>
    ///     Executes the system once.
    /// </summary>
    /// <param name="context">Context payload/>.</param>
    public abstract void Run(T context);
}