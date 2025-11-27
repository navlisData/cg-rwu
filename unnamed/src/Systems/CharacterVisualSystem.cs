using Engine.Ecs;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using OpenTK.Mathematics;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Texture.DirectedAction;
using unnamed.Texture.NonDirectionalAction;

namespace unnamed.systems;

public sealed class CharacterVisualSystem(
    World world,
    IAssetStore assetStore,
    DirectedActionDatabase directedActionDatabase,
    NonDirectionalActionDatabase nonDirectionalActionDatabase) : EntitySetSystem<float>(world, world.Query()
    .With<Character>()
    .Without<Sleeping>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        if (!e.Has<AlignedCharacter>() && !e.Has<NonDirectionalCharacter>())
        {
            return;
        }

        VisualType resolvedType;
        if (e.Has<AlignedCharacter>())
        {
            ref AlignedCharacter alignedCharacter = ref e.Get<AlignedCharacter>();
            var directedActionByChar = directedActionDatabase.GetByCharacterType(alignedCharacter.CharacterType);
            resolvedType = directedActionByChar.Get(alignedCharacter.ActionIndex, alignedCharacter.CharacterDirection);
        }
        else
        {
            ref NonDirectionalCharacter nonDirectionalCharacter = ref e.Get<NonDirectionalCharacter>();
            var nonDirectionalResolver =
                nonDirectionalActionDatabase.GetByCharacterType(nonDirectionalCharacter.CharacterType);
            resolvedType = nonDirectionalResolver.Get(nonDirectionalCharacter.ActionIndex);
        }

        switch (resolvedType)
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
                e.Get<AnimatedSprite>().RequestedAnimation = clipById;
                break;
        }
    }
}