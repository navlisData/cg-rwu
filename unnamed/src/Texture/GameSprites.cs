using engine.TextureProcessing;

namespace unnamed.Texture;

public static class GameSprites
{
    public static void Init(IAssetStore assetStore)
    {
        InitPlayerSprites(assetStore);
        InitProjectileSprites(assetStore);
        InitMapTiles(assetStore);
    }

    private static void InitPlayerSprites(IAssetStore assetStore)
    {
        SpriteSheet playerSpriteSheet =
            assetStore.LoadSpriteSheet(Path.Combine(AppContext.BaseDirectory, "Assets", "player_sheet.png"));

        /* Run Sprites */
        TextureGrid runNorthTextureGrid = new(32, 36, 0, 636);
        TextureGrid runNorthEastTextureGrid = new(32, 35, 0, 1068);
        TextureGrid runEastTextureGrid = new(32, 35, 0, 923);
        TextureGrid runSouthEastTextureGrid = new(32, 35, 0, 779);
        TextureGrid runSouthTextureGrid = new(32, 34, 0, 60);
        TextureGrid runSouthWestTextureGrid = new(32, 35, 0, 203);
        TextureGrid runWestTextureGrid = new(32, 35, 0, 347);
        TextureGrid runNorthWestTextureGrid = new(32, 35, 0, 492);

        /* Attack Sprites */
        TextureGrid attackNorthTextureGrid = new(32, 35, 0, 683);
        TextureGrid attackNorthEastTextureGrid = new(32, 35, 0, 1115);
        TextureGrid attackEastTextureGrid = new(32, 35, 0, 971);
        TextureGrid attackSouthEastTextureGrid = new(32, 36, 0, 826);
        TextureGrid attackSouthTextureGrid = new(32, 35, 0, 107);
        TextureGrid attackSouthWestTextureGrid = new(32, 36, 0, 250);
        TextureGrid attackWestTextureGrid = new(32, 35, 0, 395);
        TextureGrid attackNorthWestTextureGrid = new(32, 35, 0, 539);

        /* Idle Sprites */
        TextureGrid idleNorthTextureGrid = new(32, 35, 0, 587);
        TextureGrid idleNorthEastTextureGrid = new(32, 35, 0, 1019);
        TextureGrid idleEastTextureGrid = new(32, 35, 0, 875);
        TextureGrid idleSouthEastTextureGrid = new(32, 36, 0, 730);
        TextureGrid idleSouthTextureGrid = new(32, 35, 0, 11);
        TextureGrid idleSouthWestTextureGrid = new(32, 36, 0, 154);
        TextureGrid idleWestTextureGrid = new(32, 35, 0, 299);
        TextureGrid idleNorthWestTextureGrid = new(32, 35, 0, 443);

        var playerClips = new List<(AssetRef<AnimationClip> Clip, TextureGrid Grid, bool loop)>
        {
            // Run
            (GameAssets.Player.Run.North, runNorthTextureGrid, true),
            (GameAssets.Player.Run.NorthEast, runNorthEastTextureGrid, true),
            (GameAssets.Player.Run.East, runEastTextureGrid, true),
            (GameAssets.Player.Run.SouthEast, runSouthEastTextureGrid, true),
            (GameAssets.Player.Run.South, runSouthTextureGrid, true),
            (GameAssets.Player.Run.SouthWest, runSouthWestTextureGrid, true),
            (GameAssets.Player.Run.West, runWestTextureGrid, true),
            (GameAssets.Player.Run.NorthWest, runNorthWestTextureGrid, true),
            // Attack
            (GameAssets.Player.Attack.North, attackNorthTextureGrid, false),
            (GameAssets.Player.Attack.NorthEast, attackNorthEastTextureGrid, false),
            (GameAssets.Player.Attack.East, attackEastTextureGrid, false),
            (GameAssets.Player.Attack.SouthEast, attackSouthEastTextureGrid, false),
            (GameAssets.Player.Attack.South, attackSouthTextureGrid, false),
            (GameAssets.Player.Attack.SouthWest, attackSouthWestTextureGrid, false),
            (GameAssets.Player.Attack.West, attackWestTextureGrid, false),
            (GameAssets.Player.Attack.NorthWest, attackNorthWestTextureGrid, false),
            // Idle
            (GameAssets.Player.Idle.North, idleNorthTextureGrid, true),
            (GameAssets.Player.Idle.NorthEast, idleNorthEastTextureGrid, true),
            (GameAssets.Player.Idle.East, idleEastTextureGrid, true),
            (GameAssets.Player.Idle.SouthEast, idleSouthEastTextureGrid, true),
            (GameAssets.Player.Idle.South, idleSouthTextureGrid, true),
            (GameAssets.Player.Idle.SouthWest, idleSouthWestTextureGrid, true),
            (GameAssets.Player.Idle.West, idleWestTextureGrid, true),
            (GameAssets.Player.Idle.NorthWest, idleNorthWestTextureGrid, true),
        };

        foreach (var (clip, grid, loop) in playerClips)
        {
            var animation = SpriteSlicer.ClipFromGrid(playerSpriteSheet, grid, frameCount: 6, fps: 7f, loop: loop);
            assetStore.Register(clip, animation);
        }
    }

    private static void InitProjectileSprites(IAssetStore assetStore)
    {
        SpriteSheet projectileSpriteSheet =
            assetStore.LoadSpriteSheet(Path.Combine(AppContext.BaseDirectory, "Assets", "fireball.png"));

        TextureGrid projectileTextureGrid = new(64, 32, 0, 16);
        var idleAnimation =
            SpriteSlicer.ClipFromGrid(projectileSpriteSheet, projectileTextureGrid, frameCount: 8, fps: 24f);
        assetStore.Register(GameAssets.Projectile.Fireball, idleAnimation);
    }

    private static void InitMapTiles(IAssetStore assetStore)
    {
        SpriteSheet mapTileSpriteSheet =
            assetStore.LoadSpriteSheet(Path.Combine(AppContext.BaseDirectory, "Assets", "floor.png"));

        TextureGrid flowerTextureGrid = new(32, 32, 128, 0, Rows: 4);
        var flowerTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, flowerTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Flowers, flowerTiles);

        TextureGrid pathwayTextureGrid = new(32, 32, 0, 128, Rows: 4);
        var pathwayTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, pathwayTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Pathway, pathwayTiles);

        TextureGrid grassTextureGrid = new(32, 32, 0, 0, Rows: 4, Columns: 4);
        var grassTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, grassTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Grass, grassTiles);
    }
}