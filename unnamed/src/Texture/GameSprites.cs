using engine.TextureProcessing;

using unnamed.Enums;

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

        var playerClips =
            new List<(AssetRef<AnimationClip> Clip, TextureGrid Grid, bool loop, byte priority, float fps)>
            {
                // Run
                (GameAssets.Player.Run.North, runNorthTextureGrid, true, PlayerAction.Move.Priority(), 7f),
                (GameAssets.Player.Run.NorthEast, runNorthEastTextureGrid, true, PlayerAction.Move.Priority(), 7f),
                (GameAssets.Player.Run.East, runEastTextureGrid, true, PlayerAction.Move.Priority(), 7f),
                (GameAssets.Player.Run.SouthEast, runSouthEastTextureGrid, true, PlayerAction.Move.Priority(), 7f),
                (GameAssets.Player.Run.South, runSouthTextureGrid, true, PlayerAction.Move.Priority(), 7f),
                (GameAssets.Player.Run.SouthWest, runSouthWestTextureGrid, true, PlayerAction.Move.Priority(), 7f),
                (GameAssets.Player.Run.West, runWestTextureGrid, true, PlayerAction.Move.Priority(), 7f),
                (GameAssets.Player.Run.NorthWest, runNorthWestTextureGrid, true, PlayerAction.Move.Priority(), 7f),
                // Attack
                (GameAssets.Player.Attack.North, attackNorthTextureGrid, false, PlayerAction.Shoot.Priority(), 12f),
                (GameAssets.Player.Attack.NorthEast, attackNorthEastTextureGrid, false, PlayerAction.Shoot.Priority(),
                    12f),
                (GameAssets.Player.Attack.East, attackEastTextureGrid, false, PlayerAction.Shoot.Priority(), 12f),
                (GameAssets.Player.Attack.SouthEast, attackSouthEastTextureGrid, false, PlayerAction.Shoot.Priority(),
                    12f),
                (GameAssets.Player.Attack.South, attackSouthTextureGrid, false, PlayerAction.Shoot.Priority(), 12f),
                (GameAssets.Player.Attack.SouthWest, attackSouthWestTextureGrid, false, PlayerAction.Shoot.Priority(),
                    12f),
                (GameAssets.Player.Attack.West, attackWestTextureGrid, false, PlayerAction.Shoot.Priority(), 12f),
                (GameAssets.Player.Attack.NorthWest, attackNorthWestTextureGrid, false, PlayerAction.Shoot.Priority(),
                    12f),
                // Idle
                (GameAssets.Player.Idle.North, idleNorthTextureGrid, true, PlayerAction.Idle.Priority(), 7f),
                (GameAssets.Player.Idle.NorthEast, idleNorthEastTextureGrid, true, PlayerAction.Idle.Priority(), 7f),
                (GameAssets.Player.Idle.East, idleEastTextureGrid, true, PlayerAction.Idle.Priority(), 7f),
                (GameAssets.Player.Idle.SouthEast, idleSouthEastTextureGrid, true, PlayerAction.Idle.Priority(), 7f),
                (GameAssets.Player.Idle.South, idleSouthTextureGrid, true, PlayerAction.Idle.Priority(), 7f),
                (GameAssets.Player.Idle.SouthWest, idleSouthWestTextureGrid, true, PlayerAction.Idle.Priority(), 7f),
                (GameAssets.Player.Idle.West, idleWestTextureGrid, true, PlayerAction.Idle.Priority(), 7f),
                (GameAssets.Player.Idle.NorthWest, idleNorthWestTextureGrid, true, PlayerAction.Idle.Priority(), 7f),
            };

        foreach (var (clip, grid, loop, priority, fps) in playerClips)
        {
            var animation = SpriteSlicer.ClipFromGrid(playerSpriteSheet, grid, frameCount: 6, fps: fps,
                priority: priority, loop: loop);
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