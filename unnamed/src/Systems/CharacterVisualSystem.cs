using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Texture.DirectedAction;
using unnamed.Texture.NonDirectionalAction;

namespace unnamed.systems;

public sealed class CharacterVisualSystem(
    World world,
    IAssetStore assetStore,
    DirectedActionDatabase directedActionDatabase,
    NonDirectionalActionDatabase nonDirectionalActionDatabase) : EntitySetSystem<float>(world, new QueryBuilder()
    .With<VisibleEntity>()
    .With<Character>()
    .WithAny<AlignedCharacter, NonDirectionalCharacter>()
    .Without<Sleeping>()
    .Build()
)
{
    protected override void Update(float dt, in Entity e)
    {
        EntityHandle handle = this.world.Handle(e);

        VisualType resolvedType;
        if (handle.Has<AlignedCharacter>())
        {
            ref AlignedCharacter alignedCharacter = ref handle.Get<AlignedCharacter>();
            IDirectedSpriteResolver directedActionByChar =
                directedActionDatabase.GetByCharacterType(alignedCharacter.CharacterType);
            resolvedType = directedActionByChar.Get(alignedCharacter.ActionIndex, alignedCharacter.CharacterDirection);
        }
        else
        {
            ref NonDirectionalCharacter nonDirectionalCharacter = ref handle.Get<NonDirectionalCharacter>();
            INonDirectionalSpriteResolver nonDirectionalResolver =
                nonDirectionalActionDatabase.GetByCharacterType(nonDirectionalCharacter.CharacterType);
            resolvedType = nonDirectionalResolver.Get(nonDirectionalCharacter.ActionIndex);
        }

        switch (resolvedType)
        {
            case VisualType.StaticSpriteKey staticSprite:
                StaticSprite spriteById = assetStore.Get(staticSprite.Key);
                if (!handle.Has<Sprite>())
                {
                    handle.Add(new Sprite(spriteById));
                }

                handle.Get<Sprite>().Frame = spriteById;
                break;
            case VisualType.AnimationSpriteKey animationSprite:
                AnimationClip clipById = assetStore.Get(animationSprite.Key);
                if (!handle.Has<AnimatedSprite>())
                {
                    handle.Add(new AnimatedSprite { CurrentFrameIndex = 0, TimeInFrame = 0 });
                }

                handle.Get<AnimatedSprite>().RequestedAnimation = clipById;
                break;
        }
    }
}