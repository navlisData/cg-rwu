using Engine.Ecs;
using Engine.Ecs.Systems;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;

namespace unnamed.systems;

public sealed class CharacterAlignmentSystem(World world) : EntitySetSystem<float>(world, world.Query()
    .With<Sprite>()
    .With<AlignedCharacter>()
    .Without<Sleeping>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref Sprite sprite = ref e.Get<Sprite>();
        ref AlignedCharacter alignedCharacter = ref e.Get<AlignedCharacter>();
        
        // sprite.Frame = alignedCharacter.GetFrameIdByDirection();
    }
}