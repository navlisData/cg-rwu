using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using unnamed.Components.UI;
using unnamed.Layout;

namespace unnamed.systems;

public class UiLayoutSystem(World world)
    : EntitySetSystem<UiLayoutContext>(world,
        new QueryBuilder()
            .With<AbsolutePosition>()
            .With<AbsoluteSize>()
            .With<UiReferenceSize>()
            .With<UiReferenceOffset>()
            .With<UiAnchor>()
            .With<UiPivot>()
            .Build())
{
    protected override void Update(UiLayoutContext context, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        UiScaleMode scaleMode = handle.Has<UiScaleMode>()
            ? handle.Get<UiScaleMode>()
            : UiScaleMode.Uniform;

        UiResolvedRect rect = UiLayoutResolver.Resolve(
            context,
            handle.Get<UiReferenceSize>(),
            handle.Get<UiReferenceOffset>(),
            handle.Get<UiAnchor>(),
            handle.Get<UiPivot>(),
            scaleMode);

        ref AbsolutePosition absolutePosition = ref handle.Get<AbsolutePosition>();
        absolutePosition = new AbsolutePosition(rect.Position.X, rect.Position.Y, absolutePosition.AllowWrapping);

        ref AbsoluteSize absoluteSize = ref handle.Get<AbsoluteSize>();
        absoluteSize = new AbsoluteSize(rect.Size.X, rect.Size.Y);
    }
}