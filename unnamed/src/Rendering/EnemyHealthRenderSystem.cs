using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.Rendering;

public class EnemyHealthRenderSystem() : EntitySetSystem<RenderContext.RenderContext>(
    new QueryBuilder()
        .With<Enemy>()
        .With<Sprite>()
        .With<Position>()
        .With<Velocity>()
        .With<Transform>()
        .With<EntityStats>()
        .Without<Sleeping>()
        .Build())
{
    private static readonly Color4 BgColor = new(0f, 0f, 0f, 0.55f);
    private static readonly Color4 FgColorGreen = new(0.20f, 0.85f, 0.25f, 0.95f);
    private static readonly Color4 FgColorOrange = new(0.95f, 0.85f, 0.15f, 0.95f);
    private static readonly Color4 FgColorRed = new(0.90f, 0.20f, 0.20f, 0.95f);

    protected override void Update(ref RenderContext.RenderContext ctx, EntityHandle e)
    {
        ref Transform transform = ref e.Get<Transform>();
        Vector2 position = e.Get<Position>().ToWorldPosition();
        ref EntityStats stats = ref e.Get<EntityStats>();

        ref Sprite sprite = ref e.Get<Sprite>();

        Transform bgTransform = transform with
        {
            Size = new Vector2(transform.Size.X, 0.1f),
            Rotation = 0f,
            Height = transform.Height +
                     (transform.Size.Y * transform.Scale)
        };

        float fgRatio = (float)stats.Hitpoints / stats.MaxHealthUnits;

        Transform fgTransform = bgTransform with
        {
            Size = new Vector2(fgRatio * bgTransform.Size.X, bgTransform.Size.Y)
        };
        Vector2 fgPosition = position with
        {
            X = position.X + ((bgTransform.Size.X - fgTransform.Size.X) * transform.Scale / 2f)
        };

        Color4 fgColor = ((float)stats.Hitpoints / stats.MaxHealthUnits) switch
        {
            > 0.6f => FgColorGreen,
            > 0.3f => FgColorOrange,
            _ => FgColorRed
        };

        Vector2 healthbarPivot = new(sprite.Frame.Pivot.X, 0.5f);

        ctx.BeginDraw().WithPositionAndTransform(position, bgTransform, bgTransform.Size, healthbarPivot)
            .WithoutSprite().WithColoration(in BgColor, 1f).Draw();

        ctx.BeginDraw().WithPositionAndTransform(fgPosition, fgTransform, fgTransform.Size, healthbarPivot)
            .WithoutSprite().WithColoration(in fgColor, 1f).Draw();
    }
}