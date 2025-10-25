using System.Drawing;

using engine.TextureProcessing;

using unnamed.Components.Rendering;

namespace unnamed.Utils;

public static class GameSprites
{
    public static class Map
    {
        private const int Tile = 32;
        
        public static Dictionary<string, RectangleF> GetPathwaySprites()
        {
            var dict = new Dictionary<string, RectangleF>();
            for (int row = 4; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    dict.Add($"floor_pathway_{row}_{column}",
                        new RectangleF(column * Tile, row * Tile, Tile, Tile));
                }
            }
            return dict;
        }
    
        public static Dictionary<string, RectangleF> GetGrassSprites()
        {
            var dict = new Dictionary<string, RectangleF>();
            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    dict.Add($"floor_grass_{row}_{column}",
                        new RectangleF(column * Tile, row * Tile, Tile, Tile));
                }
            }
            return dict;
        }
    
        public static Dictionary<string, RectangleF> GetFlowerSprites()
        {
            var dict = new Dictionary<string, RectangleF>();
            for (int row = 0; row < 4; row++)
            {
                for (int column = 4; column < 8; column++)
                {
                    dict.Add($"floor_flowers_{row}_{column}",
                        new RectangleF(column * Tile, row * Tile, Tile, Tile));
                }
            }
            return dict;
        }
    
        public static Dictionary<string, RectangleF> GetAllFloorSprites()
        {
            return new[] { GetPathwaySprites(), GetGrassSprites(), GetFlowerSprites() }
                .SelectMany(d => d)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
    

    public static class Player
    {
        private static readonly string PLAYER_UP = "player_up";
        private static readonly string PLAYER_UP_RIGHT = "player_up_right";
        private static readonly string PLAYER_RIGHT  = "player_right";
        private static readonly string PLAYER_RIGHT_DOWN = "player_right_down";
        private static readonly string PLAYER_DOWN = "player_down";
        private static readonly string PLAYER_DOWN_LEFT = "player_down_left";
        private static readonly string PLAYER_LEFT = "player_left";
        private static readonly string PLAYER_LEFT_UP = "player_left_up";
            
        public static Dictionary<string, RectangleF> GetPlayerSprites()
        {
            return new Dictionary<string, RectangleF>()
            {
                { PLAYER_UP, new RectangleF(8, 588, 16, 34) },
                { PLAYER_UP_RIGHT, new RectangleF(8, 1020, 16, 34) },
                { PLAYER_RIGHT, new RectangleF(10, 876, 15, 34) },
                { PLAYER_RIGHT_DOWN, new RectangleF(8, 731, 16, 35) },
                { PLAYER_DOWN, new RectangleF(7, 12, 16, 34) },
                { PLAYER_DOWN_LEFT, new RectangleF(8, 155, 16, 35) },
                { PLAYER_LEFT, new RectangleF(7, 300, 15, 34) },
                { PLAYER_LEFT_UP, new RectangleF(8, 444, 16, 34) },
            };
        }

        public static AlignedCharacter ToAlignedCharacter(SpriteSheetId spriteSheet, AssetStore assets)
        {
            return new AlignedCharacter {
                CharacterDirection = CharacterDirection.Down,
                FrameUp = assets.GetFrame(spriteSheet, PLAYER_UP),
                FrameUpRight = assets.GetFrame(spriteSheet, PLAYER_UP_RIGHT),
                FrameRight = assets.GetFrame(spriteSheet, PLAYER_RIGHT),
                FrameDownRight = assets.GetFrame(spriteSheet, PLAYER_RIGHT_DOWN),
                FrameDown = assets.GetFrame(spriteSheet, PLAYER_DOWN),
                FrameDownLeft = assets.GetFrame(spriteSheet, PLAYER_DOWN_LEFT),
                FrameLeft = assets.GetFrame(spriteSheet, PLAYER_LEFT),
                FrameUpLeft = assets.GetFrame(spriteSheet, PLAYER_LEFT_UP)
            };
        }
    }
}