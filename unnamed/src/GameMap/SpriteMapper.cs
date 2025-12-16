using System.Diagnostics;

using engine.TextureProcessing;

using unnamed.Enums;
using unnamed.Texture;

namespace unnamed.GameMap;

public class SpriteMapper(IAssetStore assetStore)
{
    private readonly Random rng = Random.Shared;

    internal (StaticSprite sprite, StaticSprite? overlay, ushort layer) MapToSprite(int x, int y,
        in IntermediateMap map)
    {
        TileFlags tile = map[x, y];
        StaticSprite sprite;
        StaticSprite? overlay = null;
        ushort layer = 0;

        if (tile.IsWalkable())
        {
            sprite = this.GetSprite(GameAssets.MapTiles.Grass);

            if (map.IsWallBottomCenterOf(x, y))
            {
                overlay = this.GetWallTop(x, y, in map);
                sprite = this.GetSprite(GameAssets.MapTiles.Grass);
                layer = 1;
            }
            else if (tile.IsPath())
            {
                sprite = this.GetSprite(GameAssets.MapTiles.Pathway);
            }
        }
        else
        {
            (sprite, overlay, layer) = this.GetWall(x, y, in map);
        }

        return (sprite, overlay, layer);
    }

    private (StaticSprite, StaticSprite?, ushort) GetWall(int x, int y, in IntermediateMap map)
    {
        if (!map.IsWallTopCenterOf(x, y) && !map.IsWallBottomCenterOf(x, y))
        {
            this.PrintDebug(x, y, 1, "OneHighWall", in map);
            return (this.GetSprite(GameAssets.WallTiles.Illegal), null, 0);
        }

        if (!map.IsWallLeftOf(x, y) && !map.IsWallRightOf(x, y))
        {
            this.PrintDebug(x, y, 1, "SingleWallColumn", in map);
            return (this.GetSprite(GameAssets.WallTiles.Illegal), null, 0);
        }

        StaticSprite under = this.GetSprite(GameAssets.MapTiles.Grass);

        if (!map.IsWallBottomCenterOf(x, y))
        {
            if (map.IsWallLeftOf(x, y))
            {
                if (map.IsWallRightOf(x, y))
                {
                    if (map.IsWallBottomLeftOf(x, y))
                    {
                        return (this.GetSprite(GameAssets.WallTiles.WallTileBaseLeftInner), null, 0);
                    }

                    return (this.GetSprite(GameAssets.WallTiles.WallTileBaseCenter), null, 0);
                }

                return (under, this.GetSprite(GameAssets.WallTiles.WallTileBaseRight), 0);
            }

            if (map.IsWallRightOf(x, y))
            {
                return (under, this.GetSprite(GameAssets.WallTiles.WallTileBaseLeft), 0);
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
                        return (this.GetSprite(GameAssets.WallTiles.WallTileTopLeftInner), null, 0);
                    }

                    return (this.GetSprite(GameAssets.WallTiles.WallTileTopCenter), null, 0);
                }

                return (under, this.GetSprite(GameAssets.WallTiles.WallTileTopRight), 0);
            }

            if (map.IsWallBottomRightOf(x, y))
            {
                return (under, this.GetSprite(GameAssets.WallTiles.WallTileTopLeft), 0);
            }
        }

        if (!map.IsWallLeftOf(x, y) || !map.IsWallBottomLeftOf(x, y))
        {
            if (map.IsWallBottomLeftOf(x, y))
            {
                return (under, this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerBottomRight), 0);
            }

            return (under, this.GetSprite(GameAssets.WallTiles.WallFrameLeft), 1);
        }

        if (!map.IsWallRightOf(x, y) || !map.IsWallBottomRightOf(x, y))
        {
            if (map.IsWallBottomRightOf(x, y))
            {
                return (under, this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerBottomLeft), 1);
            }

            return (under, this.GetSprite(GameAssets.WallTiles.WallFrameRight), 1);
        }

        if (!map.IsWallBottomBottomLeftOf(x, y))
        {
            return (under, this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerTopRight), 1);
        }

        if (!map.IsWallBottomBottomRightOf(x, y))
        {
            return (under, this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerTopLeft), 1);
        }

        return (under, this.GetSprite(GameAssets.WallTiles.WallFrameCenter), 1);
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