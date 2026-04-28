using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Resources;
using unnamed.Texture;
using unnamed.Utils.Health;

namespace unnamed.systems;

public sealed class HealthHudSyncSystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<EntityStats>()
        .With<HudHearts>()
        .With<HealthHudVisualDirty>()
        .Build();

    public override void Run(World world)
    {
        ref DeltaTime dt = ref world.GetResource<DeltaTime>();
        ref AssetStore assetStore = ref world.GetResource<AssetStore>();

        foreach (Entity e in Query.AsEnumerator(world))
        {
            Update(world, ref dt, ref assetStore, world.Handle(e));
        }
    }

    /// <summary>
    ///     Updates heart sprites for a single entity when the visuals are marked dirty.
    /// </summary>
    private static void Update(World world, ref DeltaTime dt, ref AssetStore assetStore, EntityHandle e)
    {
        ref EntityStats stats = ref e.Get<EntityStats>();
        ref HudHearts hud = ref e.Get<HudHearts>();

        int requiredSlots = HeartStatusUtil.RequiredSlots(stats.MaxHealthUnits);

        if (hud.hearts.Length < requiredSlots)
        {
            // Layout is out of date; request a layout refresh and retry on the next update.
            e.Ensure<HealthHudLayoutDirty>();
            return;
        }

        for (int i = 0; i < requiredSlots; i++)
        {
            EntityHandle heartHandle = world.Handle(hud.hearts[i]);

            if (!heartHandle.IsAlive())
            {
                if (!e.Has<HealthHudLayoutDirty>())
                {
                    e.Add<HealthHudLayoutDirty>();
                }

                return;
            }

            HeartStatus status = HeartStatusUtil.ComputeStatus(stats.Hitpoints, i);
            ref Sprite sprite = ref heartHandle.Get<Sprite>();
            sprite.Frame = assetStore.Get(status.GetAsset());
        }

        e.Remove<HealthHudVisualDirty>();
    }
}