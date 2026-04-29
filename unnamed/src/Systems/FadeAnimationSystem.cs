using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Resources;

namespace unnamed.Systems;

public sealed class FadeAnimationSystem() : EntitySetSystem<DeltaTime>(new QueryBuilder()
    .With<FadeAnimation>()
    .With<Sprite>()
    .Build()
)
{
    protected override void Update(ref DeltaTime dt, EntityHandle e)
    {
        ref FadeAnimation fade = ref e.Get<FadeAnimation>();
        ref Sprite sprite = ref e.Get<Sprite>();

        if (!sprite.Tint.HasValue)
        {
            e.Remove<FadeAnimation>();
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
                e.Remove<FadeAnimation>();
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