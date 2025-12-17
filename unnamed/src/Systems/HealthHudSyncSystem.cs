using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.General;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Enums;
using unnamed.Utils.Health;

namespace unnamed.systems;

public sealed class HealthHudSyncSystem(World world, IAssetStore assets)
    : EntitySetSystem<float>(world, new QueryBuilder()
        .With<EntityStats>()
        .With<HudHearts>()
        .With<HealthHudVisualDirty>()
        .Build())
{
    /// <summary>
    ///     Updates heart sprites for a single entity when the visuals are marked dirty.
    /// </summary>
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref EntityStats stats = ref handle.Get<EntityStats>();
        ref HudHearts hud = ref handle.Get<HudHearts>();

        int requiredSlots = HeartStatusUtil.RequiredSlots(stats.MaxHealthUnits);

        if (hud.hearts.Length < requiredSlots)
        {
            // Layout is out of date; request a layout refresh and retry on the next update.
            handle.Ensure<HealthHudLayoutDirty>();
            return;
        }

        for (int i = 0; i < requiredSlots; i++)
        {
            EntityHandle heartHandle = this.world.Handle(hud.hearts[i]);

            if (!heartHandle.IsAlive())
            {
                if (!handle.Has<HealthHudLayoutDirty>())
                {
                    handle.Add<HealthHudLayoutDirty>();
                }

                return;
            }

            HeartStatus status = HeartStatusUtil.ComputeStatus(stats.Hitpoints, i);
            ref Sprite sprite = ref heartHandle.Get<Sprite>();
            sprite.Frame = assets.Get(status.GetAsset());
        }

        handle.Remove<HealthHudVisualDirty>();
    }
}