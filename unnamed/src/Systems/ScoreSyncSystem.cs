using Engine.Ecs;
using Engine.Ecs.Querying;
using Engine.Ecs.Systems;

using engine.TextureProcessing;

using unnamed.Components.Rendering;
using unnamed.Components.Tags;
using unnamed.Components.UI;
using unnamed.Texture.DigitTextures;

namespace unnamed.systems;

public sealed class ScoreSyncSystem : BaseSystem
{
    private static readonly Query PlayerQuery = new QueryBuilder()
        .With<ScoreDigits>()
        .With<Player>()
        .With<ScoreVisualDirty>()
        .Build();

    public override void Run(World world)
    {
        if (!PlayerQuery.TrySingle(world, out Entity player))
        {
            return;
        }

        ref DigitTextureDatabase digitTextures = ref world.GetResource<DigitTextureDatabase>();
        ref ScoreDigits scoreDigits = ref world.Get<ScoreDigits>(player);

        foreach (Entity digitEntity in scoreDigits.digits)
        {
            if (!world.IsAlive(digitEntity))
            {
                continue;
            }

            EntityHandle digitHandle = world.Handle(digitEntity);

            ref ScoreDigit scoreDigit = ref digitHandle.Get<ScoreDigit>();
            DigitTexture digitTexture = digitTextures.GetDigitTexture(scoreDigit.Value);

            digitHandle.Ensure(new StaticTextTexture(digitTexture.Texture, Pivot.TopRight));

            ref StaticTextTexture staticTextTexture = ref digitHandle.Get<StaticTextTexture>();
            staticTextTexture = new StaticTextTexture(digitTexture.Texture, Pivot.TopRight);
        }

        world.Remove<ScoreVisualDirty>(player);
    }
}