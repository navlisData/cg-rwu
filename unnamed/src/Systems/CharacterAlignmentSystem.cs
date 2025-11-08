using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Texture.DirectedAction;

namespace unnamed.systems;

public sealed class CharacterAlignmentSystem(
    World world,
    IAssetStore assetStore,
    DirectedActionDatabase directedActionDatabase) : EntitySetSystem<float>(world, world.Query()
    .With<AlignedCharacter>()
    .Without<Sleeping>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        ref AlignedCharacter alignedCharacter = ref e.Get<AlignedCharacter>();

        var directedActionByChar = directedActionDatabase.GetByCharacterType(alignedCharacter.CharacterType);
        VisualType type = directedActionByChar.Get(alignedCharacter.ActionIndex, alignedCharacter.CharacterDirection);
        switch (type)
        {
            case VisualType.StaticSpriteKey staticSprite:
                StaticSprite spriteById = assetStore.Get(staticSprite.Key);
                if (!e.Has<Sprite>())
                    e.Add(new Sprite { Tint = new Vector4(0f, 0f, 0f, 1f), Layer = 0 });
                e.Get<Sprite>().Frame = spriteById;
                break;
            case VisualType.AnimationSpriteKey animationSprite:
                AnimationClip clipById = assetStore.Get(animationSprite.Key);
                if (!e.Has<AnimatedSprite>())
                    e.Add(new AnimatedSprite { CurrentFrameIndex = 0, TimeInFrame = 0 });
                e.Get<AnimatedSprite>().AnimationClip = clipById;
                break;
        }
    }
}