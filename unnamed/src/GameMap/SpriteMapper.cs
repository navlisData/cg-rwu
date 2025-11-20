using System.Diagnostics;

using engine.TextureProcessing;

using unnamed.Enums;
using unnamed.Texture;

namespace unnamed.GameMap;

public class SpriteMapper(IAssetStore assetStore)
{
    private readonly Random rng = Random.Shared;

    internal StaticSprite MapToSprite(int x, int y, in IntermediateMap map)
    {
        TileFlags tile = map[x, y];
        StaticSprite sprite;

        if (tile.IsWalkable())
        {
            sprite = this.GetSprite(GameAssets.MapTiles.Grass);

            if (map.IsWallBottomCenterOf(x, y))
            {
                return this.GetWallTop(x, y, in map);
            }

            if (tile.IsPath())
            {
                return this.GetSprite(GameAssets.MapTiles.Pathway);
            }
        }
        else
        {
            sprite = this.GetWall(x, y, in map);
        }

        return sprite;
    }

    private StaticSprite GetWall(int x, int y, in IntermediateMap map)
    {
        if (!map.IsWallTopCenterOf(x, y) && !map.IsWallBottomCenterOf(x, y))
        {
            this.PrintDebug(x, y, 1, "OneHighWall", in map);
            return this.GetSprite(GameAssets.WallTiles.Illegal);
        }

        if (!map.IsWallLeftOf(x, y) && !map.IsWallRightOf(x, y))
        {
            this.PrintDebug(x, y, 1, "SingleWallColumn", in map);
            return this.GetSprite(GameAssets.WallTiles.Illegal);
        }

        if (!map.IsWallBottomCenterOf(x, y))
        {
            if (map.IsWallLeftOf(x, y))
            {
                if (map.IsWallRightOf(x, y))
                {
                    if (map.IsWallBottomLeftOf(x, y))
                    {
                        return this.GetSprite(GameAssets.WallTiles.WallTileBaseLeftInner);
                    }

                    return this.GetSprite(GameAssets.WallTiles.WallTileBaseCenter);
                }

                return this.GetSprite(GameAssets.WallTiles.WallTileBaseRight);
            }

            if (map.IsWallRightOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallTileBaseLeft);
            }
        }

        if (!map.IsWallBottomBottomCenterOf(x, y))
        {
            if (map.IsWallBottomLeftOf(x, y))
            {
                if (map.IsWallBottomRightOf(x, y))
                {
                    if (map.IsWallBottomBottomLeftOf(x, y))
                    {
                        return this.GetSprite(GameAssets.WallTiles.WallTileTopLeftInner);
                    }

                    return this.GetSprite(GameAssets.WallTiles.WallTileTopCenter);
                }

                return this.GetSprite(GameAssets.WallTiles.WallTileTopRight);
            }

            if (map.IsWallBottomRightOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallTileTopLeft);
            }
        }

        if (!map.IsWallLeftOf(x, y) || !map.IsWallBottomLeftOf(x, y))
        {
            if (map.IsWallBottomLeftOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerBottomRight);
            }

            return this.GetSprite(GameAssets.WallTiles.WallFrameLeft);
        }

        if (!map.IsWallRightOf(x, y) || !map.IsWallBottomRightOf(x, y))
        {
            if (map.IsWallBottomRightOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerBottomLeft);
            }

            return this.GetSprite(GameAssets.WallTiles.WallFrameRight);
        }

        if (!map.IsWallBottomBottomLeftOf(x, y))
        {
            return this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerTopRight);
        }

        if (!map.IsWallBottomBottomRightOf(x, y))
        {
            return this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerTopLeft);
        }

        return this.GetSprite(GameAssets.WallTiles.WallFrameCenter);
    }

    private StaticSprite GetWallTop(int x, int y, in IntermediateMap map)
    {
        if (map.IsWallBottomRightOf(x, y))
        {
            if (map.IsWallBottomLeftOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallFrameTopCenter);
            }

            return this.GetSprite(GameAssets.WallTiles.WallFrameTopLeft);
        }

        if (map.IsWallBottomLeftOf(x, y))
        {
            return this.GetSprite(GameAssets.WallTiles.WallFrameTopRight);
        }

        if (map.IsWallRightOf(x, y))
        {
            if (map.IsWallLeftOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallFrameTopCenter);
            }

            return this.GetSprite(GameAssets.WallTiles.WallFrameTopLeft);
        }

        if (map.IsWallLeftOf(x, y))
        {
            return this.GetSprite(GameAssets.WallTiles.WallFrameTopRight);
        }

        this.PrintDebug(x, y, 1, "SingleWallColumn", in map);
        return this.GetSprite(GameAssets.WallTiles.Illegal);
    }

    private StaticSprite GetSprite(AssetRef<SpriteSet> key)
    {
        SpriteSet set = assetStore.Get(key);
        return set[this.rng.Next(set.Count)];
    }

    private StaticSprite GetSprite(AssetRef<StaticSprite> key)
    {
        return assetStore.Get(key);
    }

    [Conditional("DEBUG")]
    private void PrintDebug(int x, int y, int radius, string context, in IntermediateMap map)
    {
        Console.WriteLine($"Mapgen Error ({context}) at {x}, {y}");
        for (int yi = y + radius; yi >= y - radius; yi -= 1)
        {
            for (int xi = x - radius; xi <= x + radius; xi += 1)
            {
                try
                {
                    Console.Write($"{(int)map[xi, yi]} ");
                }
                catch
                {
                    Console.WriteLine("X");
                }
            }

            Console.WriteLine();
        }
    }
}