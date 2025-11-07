using engine.TextureProcessing;

namespace unnamed.Texture;

public static class GameAssets
{
    public static class Player
    {
        private const string BasePrefix = "player/";

        public static class Run
        {
            private const string RunPath = BasePrefix + "run/";

            public static readonly AssetRef<AnimationClip> North =
                AssetRef<AnimationClip>.FromPath(RunPath + "north");

            public static readonly AssetRef<AnimationClip> NorthEast =
                AssetRef<AnimationClip>.FromPath(RunPath + "north-east");

            public static readonly AssetRef<AnimationClip> East =
                AssetRef<AnimationClip>.FromPath(RunPath + "east");

            public static readonly AssetRef<AnimationClip> SouthEast =
                AssetRef<AnimationClip>.FromPath(RunPath + "south-east");

            public static readonly AssetRef<AnimationClip> South =
                AssetRef<AnimationClip>.FromPath(RunPath + "south");

            public static readonly AssetRef<AnimationClip> SouthWest =
                AssetRef<AnimationClip>.FromPath(RunPath + "south-west");

            public static readonly AssetRef<AnimationClip> West =
                AssetRef<AnimationClip>.FromPath(RunPath + "west");

            public static readonly AssetRef<AnimationClip> NorthWest =
                AssetRef<AnimationClip>.FromPath(RunPath + "north-west");
        }
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