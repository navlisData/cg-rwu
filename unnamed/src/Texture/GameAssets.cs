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

        public static class Idle
        {
            private const string IdlePath = BasePrefix + "idle/";

            public static readonly AssetRef<AnimationClip> North =
                AssetRef<AnimationClip>.FromPath(IdlePath + "north");

            public static readonly AssetRef<AnimationClip> NorthEast =
                AssetRef<AnimationClip>.FromPath(IdlePath + "north-east");

            public static readonly AssetRef<AnimationClip> East =
                AssetRef<AnimationClip>.FromPath(IdlePath + "east");

            public static readonly AssetRef<AnimationClip> SouthEast =
                AssetRef<AnimationClip>.FromPath(IdlePath + "south-east");

            public static readonly AssetRef<AnimationClip> South =
                AssetRef<AnimationClip>.FromPath(IdlePath + "south");

            public static readonly AssetRef<AnimationClip> SouthWest =
                AssetRef<AnimationClip>.FromPath(IdlePath + "south-west");

            public static readonly AssetRef<AnimationClip> West =
                AssetRef<AnimationClip>.FromPath(IdlePath + "west");

            public static readonly AssetRef<AnimationClip> NorthWest =
                AssetRef<AnimationClip>.FromPath(IdlePath + "north-west");
        }

        public static class Attack
        {
            private const string AttackPath = BasePrefix + "attack/";

            public static readonly AssetRef<AnimationClip> North =
                AssetRef<AnimationClip>.FromPath(AttackPath + "north");

            public static readonly AssetRef<AnimationClip> NorthEast =
                AssetRef<AnimationClip>.FromPath(AttackPath + "north-east");

            public static readonly AssetRef<AnimationClip> East =
                AssetRef<AnimationClip>.FromPath(AttackPath + "east");

            public static readonly AssetRef<AnimationClip> SouthEast =
                AssetRef<AnimationClip>.FromPath(AttackPath + "south-east");

            public static readonly AssetRef<AnimationClip> South =
                AssetRef<AnimationClip>.FromPath(AttackPath + "south");

            public static readonly AssetRef<AnimationClip> SouthWest =
                AssetRef<AnimationClip>.FromPath(AttackPath + "south-west");

            public static readonly AssetRef<AnimationClip> West =
                AssetRef<AnimationClip>.FromPath(AttackPath + "west");

            public static readonly AssetRef<AnimationClip> NorthWest =
                AssetRef<AnimationClip>.FromPath(AttackPath + "north-west");
        }
    }

    public static class Enemy
    {
        private const string BasePrefix = "enemy/";

        public static class Slime1
        {
            private const string Slime1Path = BasePrefix + "slime1/";

            public static readonly AssetRef<AnimationClip> Idle =
                AssetRef<AnimationClip>.FromPath(Slime1Path + "idle");

            public static readonly AssetRef<AnimationClip> Move =
                AssetRef<AnimationClip>.FromPath(Slime1Path + "move");

            public static readonly AssetRef<AnimationClip> Attack =
                AssetRef<AnimationClip>.FromPath(Slime1Path + "attack");

            public static readonly AssetRef<AnimationClip> Damage =
                AssetRef<AnimationClip>.FromPath(Slime1Path + "damage");
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

    public static class WallTiles
    {
        private const string Prefix = "walls/";

        public static readonly AssetRef<StaticSprite> Illegal =
            AssetRef<StaticSprite>.FromPath(Prefix + "Illegal");

        public static readonly AssetRef<StaticSprite> WallFrameTopLeft =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameTopLeft");

        public static readonly AssetRef<StaticSprite> WallFrameTopCenter =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameTopCenter");

        public static readonly AssetRef<StaticSprite> WallFrameTopRight =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameTopRight");

        public static readonly AssetRef<StaticSprite> WallFrameLeft =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameLeft");

        public static readonly AssetRef<StaticSprite> WallFrameCenter =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameCenter");

        public static readonly AssetRef<StaticSprite> WallFrameRight =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameRight");

        public static readonly AssetRef<StaticSprite> WallFrameOuterCornerTopRight =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameOuterCornerTopRight");

        public static readonly AssetRef<StaticSprite> WallFrameOuterCornerTopLeft =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameOuterCornerTopLeft");

        public static readonly AssetRef<StaticSprite> WallFrameOuterCornerBottomLeft =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameOuterCornerBottomLeft");

        public static readonly AssetRef<StaticSprite> WallFrameOuterCornerBottomRight =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallFrameOuterCornerBottomRight");


        public static readonly AssetRef<StaticSprite> WallTileTopLeft =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallTileTopLeft");

        public static readonly AssetRef<StaticSprite> WallTileTopCenter =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallTileTopCenter");

        public static readonly AssetRef<StaticSprite> WallTileTopRight =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallTileTopRight");

        public static readonly AssetRef<StaticSprite> WallTileBaseLeft =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallTileBaseLeft");

        public static readonly AssetRef<StaticSprite> WallTileBaseCenter =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallTileBaseCenter");

        public static readonly AssetRef<StaticSprite> WallTileBaseRight =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallTileBaseRight");

        public static readonly AssetRef<StaticSprite> WallTileTopLeftInner =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallTileTopLeftInner");

        public static readonly AssetRef<StaticSprite> WallTileBaseLeftInner =
            AssetRef<StaticSprite>.FromPath(Prefix + "WallTileBaseLeftInner");
    }

    public static class Explosion
    {
        private const string Prefix = "explosion/";

        public static readonly AssetRef<AnimationClip> BulletExplosion =
            AssetRef<AnimationClip>.FromPath(Prefix + "bullet-explosion");
    }

    public static class Props
    {
        private const string Prefix = "props/";

        public static readonly AssetRef<StaticSprite> Chest =
            AssetRef<StaticSprite>.FromPath(Prefix + "chest");
    }

    public static class Crosshair
    {
        private const string Prefix = "crosshair/";

        public static readonly AssetRef<StaticSprite> Simple =
            AssetRef<StaticSprite>.FromPath(Prefix + "simple");
    }

    public static class Hearts
    {
        private const string Prefix = "hearts/";

        public static readonly AssetRef<StaticSprite> Empty =
            AssetRef<StaticSprite>.FromPath(Prefix + "empty");

        public static readonly AssetRef<StaticSprite> Half =
            AssetRef<StaticSprite>.FromPath(Prefix + "half");

        public static readonly AssetRef<StaticSprite> Full =
            AssetRef<StaticSprite>.FromPath(Prefix + "full");
    }

    public static class Drops
    {
        private const string Prefix = "drops/";

        public static readonly AssetRef<StaticSprite> RedHeart =
            AssetRef<StaticSprite>.FromPath(Prefix + "red-heart");

        public static readonly AssetRef<StaticSprite> BlueHeart =
            AssetRef<StaticSprite>.FromPath(Prefix + "blue-heart");
    }
}