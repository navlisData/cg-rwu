using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.Drops;
using unnamed.Components.Map;
using unnamed.Components.Physics;
using unnamed.Enums;
using unnamed.Prefabs;
using unnamed.Utils.Loot;

namespace unnamed.systems;

public sealed class DestroyEntitySystem(World world, IAssetStore assetStore) : EntitySetSystem<float>(world,
    world.Query()
        .With<MarkedToDestroy>()
        .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref MarkedToDestroy mtd = ref handle.Get<MarkedToDestroy>();
        mtd.RemainingLifetime -= dt;
        if (mtd.RemainingLifetime <= 0f)
        {
            if (handle.Has<LootTable>())
            {
                DropLoot(handle);
            }

            this.world.DestroyEntity(e);
        }
    }

    private void DropLoot(EntityHandle handle)
    {
        ref LootTable lootTable = ref handle.Get<LootTable>();
        ref Position position = ref handle.Get<Position>();

        Span<DropType> drops = stackalloc DropType[lootTable.DropCount];
        Random rng = new();
        var count = LootApi.Roll(lootTable, rng, drops);
        for (var i = 0; i < count; i++)
        {
            Console.WriteLine("Loot dropped: " + drops[i]);
            PrefabFactory.CreateDrop(this.world, drops[i], assetStore, position);
        }

        if (count == 0)
        {
            Console.WriteLine("no drop this time :(");
        }
    }
}