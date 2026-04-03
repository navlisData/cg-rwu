using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.General;
using unnamed.Components.Physics;
using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;

namespace unnamed.Rendering;

public sealed class EnemyHealthRenderSystem(World world)
    : EntitySetSystem<RenderContext.RenderContext>(world, new QueryBuilder()
        .With<Enemy>()
        .With<Sprite>()
        .With<Position>()
        .With<Velocity>()
        .With<Transform>()
        .With<EntityStats>()
        .Without<Sleeping>()
        .Build())
{
    private readonly Color4 bgColor = new(0f, 0f, 0f, 0.55f);
    private readonly Color4 fgColorGreen = new(0.20f, 0.85f, 0.25f, 0.95f);
    private readonly Color4 fgColorOrange = new(0.95f, 0.85f, 0.15f, 0.95f);
    private readonly Color4 fgColorRed = new(0.90f, 0.20f, 0.20f, 0.95f);

    protected override void Update(RenderContext.RenderContext ctx, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref Transform transform = ref handle.Get<Transform>();
        Vector2 position = handle.Get<Position>().ToWorldPosition();
        ref EntityStats stats = ref handle.Get<EntityStats>();

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
            > 0.6f => this.fgColorGreen,
            > 0.3f => this.fgColorOrange,
            _ => this.fgColorRed
        };

        ctx.BeginDraw().WithoutSprite().WithColoration(in this.bgColor, 1f)
            .WithPositionAndTransform(position, bgTransform, bgTransform.Size, UiPivot.Center).WithUnitQuad().Draw();

        ctx.BeginDraw().WithoutSprite().WithColoration(in fgColor, 1f).WithPositionAndTransform(fgPosition, fgTransform, fgTransform.Size, UiPivot.Center)
            .WithUnitQuad().Draw();
    }
}