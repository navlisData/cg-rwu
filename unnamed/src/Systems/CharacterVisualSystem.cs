using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Texture;
using unnamed.Texture.DirectedAction;
using unnamed.Texture.NonDirectionalAction;

namespace unnamed.systems;

public sealed class CharacterVisualSystem()
    : EntitySetSystem<AssetStore, DirectedActionDatabase, NonDirectionalActionDatabase>(
        new QueryBuilder()
            .With<VisibleEntity>()
            .With<Character>()
            .WithAny<AlignedCharacter, NonDirectionalCharacter>()
            .Without<Sleeping>()
            .Build()
    )
{
    protected override void Update(ref AssetStore assetStore, ref DirectedActionDatabase directedActionDatabase,
        ref NonDirectionalActionDatabase nonDirectionalActionDatabase, EntityHandle e)
    {
        VisualType resolvedType;
        if (e.Has<AlignedCharacter>())
        {
            ref AlignedCharacter alignedCharacter = ref e.Get<AlignedCharacter>();
            IDirectedSpriteResolver directedActionByChar =
                directedActionDatabase.GetByCharacterType(alignedCharacter.CharacterType);
            resolvedType = directedActionByChar.Get(alignedCharacter.ActionIndex, alignedCharacter.CharacterDirection);
        }
        else
        {
            ref NonDirectionalCharacter nonDirectionalCharacter = ref e.Get<NonDirectionalCharacter>();
            INonDirectionalSpriteResolver nonDirectionalResolver =
                nonDirectionalActionDatabase.GetByCharacterType(nonDirectionalCharacter.CharacterType);
            resolvedType = nonDirectionalResolver.Get(nonDirectionalCharacter.ActionIndex);
        }

        switch (resolvedType)
        {
            case VisualType.StaticSpriteKey staticSprite:
                StaticSprite spriteById = assetStore.Get(staticSprite.Key);
                if (!e.Has<Sprite>())
                {
                    e.Add(new Sprite(spriteById));
                }

                e.Get<Sprite>().Frame = spriteById;
                break;
            case VisualType.AnimationSpriteKey animationSprite:
                AnimationClip clipById = assetStore.Get(animationSprite.Key);
                if (!e.Has<AnimatedSprite>())
                {
                    e.Add(new AnimatedSprite { CurrentFrameIndex = 0, TimeInFrame = 0 });
                }

                e.Get<AnimatedSprite>().RequestedAnimation = clipById;
                break;
        }
    }
}