using System.Drawing;

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
        InitWallTiles(assetStore);
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

        List<(AssetRef<AnimationClip> Clip, TextureGrid Grid, bool loop, byte priority, float fps)> playerClips =
            new()
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
                (GameAssets.Player.Idle.NorthWest, idleNorthWestTextureGrid, true, PlayerAction.Idle.Priority(), 7f)
            };

        foreach ((AssetRef<AnimationClip> clip, TextureGrid grid, bool loop, byte priority, float fps) in playerClips)
        {
            AnimationClip animation = SpriteSlicer.ClipFromGrid(playerSpriteSheet, grid, 6, fps,
                priority, loop);
            assetStore.Register(clip, animation);
        }
    }

    private static void InitProjectileSprites(IAssetStore assetStore)
    {
        SpriteSheet projectileSpriteSheet =
            assetStore.LoadSpriteSheet(Path.Combine(AppContext.BaseDirectory, "Assets", "fireball.png"));

        TextureGrid projectileTextureGrid = new(64, 32, 0, 16);
        AnimationClip idleAnimation =
            SpriteSlicer.ClipFromGrid(projectileSpriteSheet, projectileTextureGrid, 8, 24f);
        assetStore.Register(GameAssets.Projectile.Fireball, idleAnimation);
    }

    private static void InitMapTiles(IAssetStore assetStore)
    {
        SpriteSheet mapTileSpriteSheet =
            assetStore.LoadSpriteSheet(Path.Combine(AppContext.BaseDirectory, "Assets", "floor.png"));

        TextureGrid flowerTextureGrid = new(32, 32, 128, Rows: 4);
        SpriteSet flowerTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, flowerTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Flowers, flowerTiles);

        TextureGrid pathwayTextureGrid = new(32, 32, 0, 128, Rows: 4);
        SpriteSet pathwayTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, pathwayTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Pathway, pathwayTiles);

        TextureGrid grassTextureGrid = new(32, 32, Rows: 4, Columns: 4);
        SpriteSet grassTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, grassTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Grass, grassTiles);
    }

    private static void InitWallTiles(IAssetStore assetStore)
    {
        SpriteSheet wallTileSpriteSheet =
            assetStore.LoadSpriteSheet(Path.Combine(AppContext.BaseDirectory, "Assets", "walls.png"));

        assetStore.Register(GameAssets.WallTiles.Illegal,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(0, 0, 32, 32)));

        assetStore.Register(GameAssets.WallTiles.WallFrameTopLeft,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(32, 32, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameTopCenter,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(64, 32, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameTopRight,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(96, 32, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameLeft,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(32, 64, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameCenter,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(64, 64, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameRight,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(96, 64, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameOuterCornerTopLeft,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(128, 32, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameOuterCornerTopRight,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(256, 32, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameOuterCornerBottomLeft,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(128, 128, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallFrameOuterCornerBottomRight,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(256, 128, 32, 32)));

        assetStore.Register(GameAssets.WallTiles.WallTileTopLeft,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(32, 96, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallTileTopCenter,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(64, 96, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallTileTopRight,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(96, 96, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallTileBaseLeft,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(32, 128, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallTileBaseCenter,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(64, 128, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallTileBaseRight,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(96, 128, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallTileTopLeftInner,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(160, 32, 32, 32)));
        assetStore.Register(GameAssets.WallTiles.WallTileBaseLeftInner,
            SpriteSlicer.FromRect(wallTileSpriteSheet, new Rectangle(160, 64, 32, 32)));
    }
}