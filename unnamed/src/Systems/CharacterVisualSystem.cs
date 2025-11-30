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
        EntityHandle handle = this.world.Handle(e);

        if (!handle.Has<AlignedCharacter>() && !handle.Has<NonDirectionalCharacter>())
        {
            return;
        }

        VisualType resolvedType;
        if (handle.Has<AlignedCharacter>())
        {
            ref AlignedCharacter alignedCharacter = ref handle.Get<AlignedCharacter>();
            var directedActionByChar = directedActionDatabase.GetByCharacterType(alignedCharacter.CharacterType);
            resolvedType = directedActionByChar.Get(alignedCharacter.ActionIndex, alignedCharacter.CharacterDirection);
        }
        else
        {
            ref NonDirectionalCharacter nonDirectionalCharacter = ref handle.Get<NonDirectionalCharacter>();
            var nonDirectionalResolver =
                nonDirectionalActionDatabase.GetByCharacterType(nonDirectionalCharacter.CharacterType);
            resolvedType = nonDirectionalResolver.Get(nonDirectionalCharacter.ActionIndex);
        }

        switch (resolvedType)
        {
            case VisualType.StaticSpriteKey staticSprite:
                StaticSprite spriteById = assetStore.Get(staticSprite.Key);
                if (!handle.Has<Sprite>())
                    handle.Add(new Sprite { Tint = new Vector4(0f, 0f, 0f, 1f), Layer = 0 });
                handle.Get<Sprite>().Frame = spriteById;
                break;
            case VisualType.AnimationSpriteKey animationSprite:
                AnimationClip clipById = assetStore.Get(animationSprite.Key);
                if (!handle.Has<AnimatedSprite>())
                    handle.Add(new AnimatedSprite { CurrentFrameIndex = 0, TimeInFrame = 0 });
                handle.Get<AnimatedSprite>().RequestedAnimation = clipById;
                break;
        }
    }
}