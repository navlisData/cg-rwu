using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;

namespace unnamed.Rendering;

public sealed class FadeAnimationSystem(World world) : EntitySetSystem<float>(world, new QueryBuilder()
    .With<FadeAnimation>()
    .With<Sprite>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        ref FadeAnimation fade = ref handle.Get<FadeAnimation>();
        ref Sprite sprite = ref handle.Get<Sprite>();

        if (!sprite.Tint.HasValue)
        {
            handle.Remove<FadeAnimation>();
            return;
        }

        fade.Time += dt;
        float alpha = sprite.Tint.Value.A;

        if (fade.Time > fade.Interval)
        {
            if (fade.Repeating)
            {
                fade.Time = 0f;
                fade.Type = fade.Type == FadeAnimationType.FadeIn
                    ? FadeAnimationType.FadeOut
                    : FadeAnimationType.FadeIn;
            }
            else
            {
                handle.Remove<FadeAnimation>();
                return;
            }
        }

        alpha = fade.Type switch
        {
            FadeAnimationType.FadeIn => MathHelper.Lerp(alpha, 1f, fade.Time / fade.Interval),
            FadeAnimationType.FadeOut => MathHelper.Lerp(alpha, 0f, fade.Time / fade.Interval),
            _ => alpha
        };

        sprite.Tint = sprite.Tint.Value with { A = alpha };
    }
}