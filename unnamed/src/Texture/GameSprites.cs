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
        
        TextureGrid idleTextureGrid = new (32, 32, 0,12);
        var idleAnimation = SpriteSlicer.ClipFromGrid(playerSpriteSheet, idleTextureGrid, frameCount:8, fps:12f);
        assetStore.Register(GameAssets.Player.Idle, idleAnimation);
    }

    private static void InitProjectileSprites(IAssetStore assetStore)
    {
        SpriteSheet projectileSpriteSheet =
            assetStore.LoadSpriteSheet(Path.Combine(AppContext.BaseDirectory, "Assets", "fireball.png"));
        
        TextureGrid projectileTextureGrid = new (64, 32, 0,16 );
        var idleAnimation = SpriteSlicer.ClipFromGrid(projectileSpriteSheet, projectileTextureGrid, frameCount:8, fps:12f);
        assetStore.Register(GameAssets.Projectile.Fireball, idleAnimation);
    }
    
    private static void InitMapTiles(IAssetStore assetStore)
    {
        SpriteSheet mapTileSpriteSheet =
            assetStore.LoadSpriteSheet(Path.Combine(AppContext.BaseDirectory, "Assets", "floor.png"));
        
        TextureGrid flowerTextureGrid = new (32, 32, 128,0, Rows:4);
        var flowerTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, flowerTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Flowers, flowerTiles);
        
        TextureGrid pathwayTextureGrid = new (32, 32, 0,128, Rows:4);
        var pathwayTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, pathwayTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Pathway, pathwayTiles);
        
        TextureGrid grassTextureGrid = new (32, 32, 0,0, Rows:4, Columns:4);
        var grassTiles = SpriteSlicer.FromGrid(mapTileSpriteSheet, grassTextureGrid);
        assetStore.Register(GameAssets.MapTiles.Grass, grassTiles);
    }
}