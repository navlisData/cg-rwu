using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.General;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;
using unnamed.Enums;
using unnamed.Prefabs;
using unnamed.Resources;
using unnamed.Texture;
using unnamed.Utils.Health;

namespace unnamed.systems;

public sealed class HealthHudLayoutSystem : BaseSystem
{
    private static readonly Query Query = new QueryBuilder()
        .With<EntityStats>()
        .With<Player>()
        .With<HealthHudLayoutDirty>()
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

    private static void Update(World world, ref DeltaTime dt, ref AssetStore assetStore, EntityHandle e)
    {
        e.Ensure(new HudHearts { hearts = [] });

        ref EntityStats stats = ref e.Get<EntityStats>();
        ref HudHearts hud = ref e.Get<HudHearts>();

        int requiredSlots = HeartStatusUtil.RequiredSlots(stats.MaxHealthUnits);

        EnsureExactSlotCount(ref hud, requiredSlots, world);

        const int gap = 8;
        const int offset = 10;
        for (int i = 0; i < requiredSlots; i++)
        {
            Entity heart = ResolveOrCreateHeart(ref hud, i, world);
            EntityHandle heartHandle = world.Handle(heart);

            heartHandle.Ensure<UiReferenceOffset>();
            heartHandle.Ensure<UiReferenceSize>();

            ref UiReferenceSize referenceSize = ref heartHandle.Get<UiReferenceSize>();
            ref UiReferenceOffset referenceOffset = ref heartHandle.Get<UiReferenceOffset>();

            float x = offset + (i * (referenceSize.Width + gap));
            float y = offset;

            referenceOffset = new UiReferenceOffset(x, y);
        }

        e.Remove<HealthHudLayoutDirty>();
        e.Ensure<HealthHudVisualDirty>();
    }

    /// <summary>
    ///     Ensures <paramref name="hud" /> has exactly <paramref name="requiredSlots" /> references.
    ///     Grows by creating new heart entities initialized as Empty, shrinks by destroying trailing entities.
    /// </summary>
    /// <param name="hud">HUD binding component.</param>
    /// <param name="requiredSlots">Exact required slot count.</param>
    /// <param name="assetStore">AssetStore</param>
    /// <param name="world">World</param>
    private static void EnsureExactSlotCount(ref HudHearts hud, int requiredSlots, World world)
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
                world.DestroyEntity(hud.hearts[i]);
            }

            Array.Resize(ref hud.hearts, requiredSlots);
            return;
        }

        // Grow
        Array.Resize(ref hud.hearts, requiredSlots);
        for (int i = current; i < requiredSlots; i++)
        {
            hud.hearts[i] = PrefabFactory.CreateHealthIndicator(world, HeartStatus.Empty);
        }
    }

    /// <summary>
    ///     Resolves a heart entity for the given slot, creating it if missing or unbound.
    /// </summary>
    /// <param name="hud">HUD binding component.</param>
    /// <param name="slotIndex">Slot index to resolve or create.</param>
    /// <param name="assetStore">Asset Store</param>
    /// <param name="world">World</param>
    /// <returns>the resolved/created heart entity.</returns>
    private static Entity ResolveOrCreateHeart(ref HudHearts hud, int slotIndex, World world)
    {
        Entity heartEntity = hud.hearts[slotIndex];
        if (world.IsAlive(heartEntity))
        {
            return heartEntity;
        }

        Entity heart = PrefabFactory.CreateHealthIndicator(world, HeartStatus.Empty);
        hud.hearts[slotIndex] = heart;

        return heart;
    }
}