using engine.TextureProcessing;

namespace unnamed.Texture;

public static class GameAssets
{
    public static class Player
    {
        private const string Prefix = "player/";
        
        public static readonly AssetRef<AnimationClip> Run =
            AssetRef<AnimationClip>.FromPath(Prefix + "run");

        public static readonly AssetRef<AnimationClip> Idle =
            AssetRef<AnimationClip>.FromPath(Prefix + "idle");
    }
    
    public static class Projectile
    {
        private const string Prefix = "projectile/";
        
        public static readonly AssetRef<AnimationClip> Fireball =
            AssetRef<AnimationClip>.FromPath(Prefix + "fireball");
    }

    public static class MapTiles
    {
        private const string Prefix = "tiles/";
        
        public static readonly AssetRef<SpriteSet> Pathway =
            AssetRef<SpriteSet>.FromPath(Prefix + "pathway");
        
        public static readonly AssetRef<SpriteSet> Flowers =
            AssetRef<SpriteSet>.FromPath(Prefix + "flowers");
        
        public static readonly AssetRef<SpriteSet> Grass =
            AssetRef<SpriteSet>.FromPath(Prefix + "grass");
    }

    public static class Props
    {
        private const string Prefix = "props/";
        public static readonly AssetRef<StaticSprite> Chest =
            AssetRef<StaticSprite>.FromPath(Prefix + "chest");
    }
}