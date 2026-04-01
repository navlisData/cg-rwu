using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.General;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;
using unnamed.Enums;
using unnamed.Prefabs;
using unnamed.Utils.Health;

namespace unnamed.systems;

public sealed class HealthHudLayoutSystem(World world, IAssetStore assets)
    : EntitySetSystem<float>(world, new QueryBuilder()
        .With<EntityStats>()
        .With<Player>()
        .With<HealthHudLayoutDirty>()
        .Build())
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);
        handle.Ensure(new HudHearts { hearts = [] });

        ref EntityStats stats = ref handle.Get<EntityStats>();
        ref HudHearts hud = ref handle.Get<HudHearts>();

        int requiredSlots = HeartStatusUtil.RequiredSlots(stats.MaxHealthUnits);

        this.EnsureExactSlotCount(ref hud, requiredSlots);

        const int gap = 8;
        const int offset = 10;
        for (int i = 0; i < requiredSlots; i++)
        {
            Entity heart = this.ResolveOrCreateHeart(ref hud, i);
            EntityHandle heartHandle = this.world.Handle(heart);
            heartHandle.Ensure<AbsolutePosition>();
            heartHandle.Ensure<AbsoluteSize>();

            ref AbsolutePosition pos = ref heartHandle.Get<AbsolutePosition>();
            ref AbsoluteSize absSize = ref heartHandle.Get<AbsoluteSize>();
            float x = i * 20;
            float y = 10;
            pos = new AbsolutePosition(x, y);
        }

        handle.Remove<HealthHudLayoutDirty>();
        handle.Ensure<HealthHudVisualDirty>();
    }

    /// <summary>
    ///     Ensures <paramref name="hud" /> has exactly <paramref name="requiredSlots" /> references.
    ///     Grows by creating new heart entities initialized as Empty, shrinks by destroying trailing entities.
    /// </summary>
    /// <param name="hud">HUD binding component.</param>
    /// <param name="requiredSlots">Exact required slot count.</param>
    private void EnsureExactSlotCount(ref HudHearts hud, int requiredSlots)
    {
        requiredSlots = Math.Max(0, requiredSlots);

        int current = hud.hearts.Length;
        if (current == requiredSlots)
        {
            return;
        }

        // Shrink
        if (current > requiredSlots)
        {
            for (int i = current - 1; i >= requiredSlots; i--)
            {
                this.world.DestroyEntity(hud.hearts[i]);
            }

            Array.Resize(ref hud.hearts, requiredSlots);
            return;
        }

        // Grow
        Array.Resize(ref hud.hearts, requiredSlots);
        for (int i = current; i < requiredSlots; i++)
        {
            hud.hearts[i] = PrefabFactory.CreateHealthIndicator(this.world, assets, HeartStatus.Empty);
        }
    }

    /// <summary>
    ///     Resolves a heart entity for the given slot, creating it if missing or unbound.
    /// </summary>
    /// <param name="hud">HUD binding component.</param>
    /// <param name="slotIndex">Slot index to resolve or create.</param>
    /// <returns>the resolved/created heart entity.</returns>
    private Entity ResolveOrCreateHeart(ref HudHearts hud, int slotIndex)
    {
        Entity heartEntity = hud.hearts[slotIndex];
        if (this.world.IsAlive(heartEntity))
        {
            return heartEntity;
        }

        Entity heart = PrefabFactory.CreateHealthIndicator(this.world, assets, HeartStatus.Empty);
        hud.hearts[slotIndex] = heart;

        return heart;
    }
}