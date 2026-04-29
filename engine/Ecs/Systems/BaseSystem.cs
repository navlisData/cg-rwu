namespace Engine.Ecs.Systems;

public abstract class BaseSystem
{
    /// <summary>
    ///     Executes the system once.
    /// </summary>
    public abstract void Run(World world);
}