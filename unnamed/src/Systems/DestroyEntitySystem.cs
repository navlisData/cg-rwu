using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.Drops;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Prefabs;
using unnamed.Resources;
using unnamed.Utils.Loot;

namespace unnamed.systems;

public sealed class DestroyEntitySystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<MarkedToDestroy>()
        .Build();

    private static readonly Query PlayerQuery = new QueryBuilder().With<Player>().Build();

    public override void Run(World world)
    {
        ref DeltaTime dt = ref world.GetResource<DeltaTime>();
        Entity player = PlayerQuery.Single(world);

        foreach (Entity e in Query.AsEnumerator(world))
        {
            EntityHandle handle = world.Handle(e);

            ref MarkedToDestroy mtd = ref handle.Get<MarkedToDestroy>();
            mtd.RemainingLifetime -= dt;

            if (mtd.RemainingLifetime > 0f)
            {
                continue;
            }

            if (handle.Has<LootTable>())
            {
                DropLoot(handle, player, world);
            }

            world.DestroyEntity(e);
        }
    }

    private static void DropLoot(EntityHandle handle, Entity player, World world)
    {
        ref LootTable lootTable = ref handle.Get<LootTable>();
        ref Position position = ref handle.Get<Position>();

        Span<DropType> drops = stackalloc DropType[lootTable.DropCount];
        int count = LootApi.Roll(lootTable, Random.Shared, drops);
        for (int i = 0; i < count; i++)
        {
#if DEBUG
            Console.WriteLine("Loot dropped: " + drops[i]);
#endif
            PrefabFactory.CreateDrop(world, drops[i], position, player);
        }

#if DEBUG
        if (count == 0)
        {
            Console.WriteLine("no drop this time :(");
        }
#endif
    }
}