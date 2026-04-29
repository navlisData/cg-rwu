using unnamed.Enums;

namespace unnamed.Texture.NonDirectionalAction;

/// <summary>
///     Stores non-directional sprite resolvers by character type.
/// </summary>
public readonly struct NonDirectionalActionDatabase
{
    private readonly Dictionary<CharacterType, INonDirectionalSpriteResolver> sets = new();

    /// <summary>
    ///     Gets a resolver by character type.
    /// </summary>
    /// <param name="type">The character type.</param>
    /// <returns>The configured resolver.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no resolver is registered for the given type.</exception>
    public INonDirectionalSpriteResolver GetByCharacterType(CharacterType type)
    {
        return this.sets[type];
    }

    /// <summary>
    ///     Registers a resolver for a character type.
    /// </summary>
    /// <param name="type">The character type.</param>
    /// <param name="resolver">The resolver instance.</param>
    private void Register(CharacterType type, INonDirectionalSpriteResolver resolver)
    {
        this.sets[type] = resolver;
    }

    /// <summary>
    ///     Creates a default database instance.
    /// </summary>
    public NonDirectionalActionDatabase()
    {
        NonDirectionalActionSpriteResolver<EnemyAction> enemySet = new()
        {
            SpriteByAction =
            {
                [EnemyAction.Idle] = GameAssets.Enemy.Slime1.Idle,
                [EnemyAction.Move] = GameAssets.Enemy.Slime1.Move,
                [EnemyAction.Attack] = GameAssets.Enemy.Slime1.Attack,
                [EnemyAction.Damage] = GameAssets.Enemy.Slime1.Damage
            }
        };

        this.Register(CharacterType.Enemy, enemySet);
    }
}