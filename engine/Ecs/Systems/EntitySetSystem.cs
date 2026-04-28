using Engine.Ecs.Querying;

namespace Engine.Ecs.Systems;

// TODO: Write a macro or whatever its called in c# for this

public abstract class EntitySetSystem(Query query) : BaseSystem
{
    public override void Run(World world)
    {
        foreach (Entity e in query.AsEnumerator(world))
        {
            this.Update(world.Handle(e));
        }
    }

    protected abstract void Update(EntityHandle e);
}

public abstract class EntitySetSystem<T1>(Query query) : BaseSystem
    where T1 : struct
{
    public override void Run(World world)
    {
        ref T1 ctx1 = ref world.GetResource<T1>();

        foreach (Entity e in query.AsEnumerator(world))
        {
            this.Update(ref ctx1, world.Handle(e));
        }
    }

    protected abstract void Update(ref T1 ctx1, EntityHandle e);
}

public abstract class EntitySetSystem<T1, T2>(Query query) : BaseSystem
    where T1 : struct
    where T2 : struct
{
    public override void Run(World world)
    {
        ref T1 ctx1 = ref world.GetResource<T1>();
        ref T2 ctx2 = ref world.GetResource<T2>();

        foreach (Entity e in query.AsEnumerator(world))
        {
            this.Update(ref ctx1, ref ctx2, world.Handle(e));
        }
    }

    protected abstract void Update(ref T1 ctx1, ref T2 ctx2, EntityHandle e);
}

public abstract class EntitySetSystem<T1, T2, T3>(Query query) : BaseSystem
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    public override void Run(World world)
    {
        ref T1 ctx1 = ref world.GetResource<T1>();
        ref T2 ctx2 = ref world.GetResource<T2>();
        ref T3 ctx3 = ref world.GetResource<T3>();

        foreach (Entity e in query.AsEnumerator(world))
        {
            this.Update(ref ctx1, ref ctx2, ref ctx3, world.Handle(e));
        }
    }

    protected abstract void Update(ref T1 ctx1, ref T2 ctx2, ref T3 ctx3, EntityHandle e);
}

public abstract class EntitySetSystem<T1, T2, T3, T4>(Query query) : BaseSystem
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    public override void Run(World world)
    {
        ref T1 ctx1 = ref world.GetResource<T1>();
        ref T2 ctx2 = ref world.GetResource<T2>();
        ref T3 ctx3 = ref world.GetResource<T3>();
        ref T4 ctx4 = ref world.GetResource<T4>();

        foreach (Entity e in query.AsEnumerator(world))
        {
            this.Update(ref ctx1, ref ctx2, ref ctx3, ref ctx4, world.Handle(e));
        }
    }

    protected abstract void Update(ref T1 ctx1, ref T2 ctx2, ref T3 ctx3, ref T4 ctx4, EntityHandle e);
}