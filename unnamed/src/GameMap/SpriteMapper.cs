using System.Diagnostics;

using engine.TextureProcessing;

using unnamed.Enums;
using unnamed.Texture;

namespace unnamed.GameMap;

public class SpriteMapper(IAssetStore assetStore)
{
    private readonly Random rng = Random.Shared;
    public TileFlags[,]? Map;

    internal StaticSprite MapToSprite(int x, int y)
    {
        Debug.Assert(this.Map != null);
        TileFlags tile = this.Map[x, y];
        StaticSprite sprite;

        if (IsWalkable(tile))
        {
            sprite = this.GetSprite(GameAssets.MapTiles.Grass);

            if (this.IsWallBeneath(x, y))
            {
                return this.GetWallTop(x, y);
            }

            if (IsPath(tile))
            {
                return this.GetSprite(GameAssets.MapTiles.Pathway);
            }
        }
        else
        {
            sprite = this.GetWall(x, y);
        }

        return sprite;
    }

    private StaticSprite GetWall(int x, int y)
    {
        if (!this.IsWallAbove(x, y) && !this.IsWallBeneath(x, y))
        {
            this.PrintDebug(x, y, 1, "OneHighWall");
            return this.GetSprite(GameAssets.WallTiles.Illegal);
        }

        if (!this.IsWallLeftOf(x, y) && !this.IsWallRightOf(x, y))
        {
            this.PrintDebug(x, y, 1, "SingleWallColumn");
            return this.GetSprite(GameAssets.WallTiles.Illegal);
        }

        if (!this.IsWallBeneath(x, y))
        {
            if (this.IsWallLeftOf(x, y))
            {
                if (this.IsWallRightOf(x, y))
                {
                    if (this.IsWallBottomLeftOf(x, y))
                    {
                        return this.GetSprite(GameAssets.WallTiles.WallTileBaseLeftInner);
                    }

                    return this.GetSprite(GameAssets.WallTiles.WallTileBaseCenter);
                }

                return this.GetSprite(GameAssets.WallTiles.WallTileBaseRight);
            }

            if (this.IsWallRightOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallTileBaseLeft);
            }
        }

        if (!this.IsWallBeneath(x, y - 1))
        {
            if (this.IsWallBottomLeftOf(x, y))
            {
                if (this.IsWallBottomRightOf(x, y))
                {
                    if (this.IsWallBottomLeftOf(x, y - 1))
                    {
                        return this.GetSprite(GameAssets.WallTiles.WallTileTopLeftInner);
                    }

                    return this.GetSprite(GameAssets.WallTiles.WallTileTopCenter);
                }

                return this.GetSprite(GameAssets.WallTiles.WallTileTopRight);
            }

            if (this.IsWallBottomRightOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallTileTopLeft);
            }
        }

        if (!this.IsWallLeftOf(x, y) || !this.IsWallBottomLeftOf(x, y))
        {
            if (this.IsWallBottomLeftOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerBottomRight);
            }

            return this.GetSprite(GameAssets.WallTiles.WallFrameLeft);
        }

        if (!this.IsWallRightOf(x, y) || !this.IsWallBottomRightOf(x, y))
        {
            if (this.IsWallBottomRightOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerBottomLeft);
            }

            return this.GetSprite(GameAssets.WallTiles.WallFrameRight);
        }

        if (!this.IsWallBottomLeftOf(x, y - 1))
        {
            return this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerTopRight);
        }

        if (!this.IsWallBottomRightOf(x, y - 1))
        {
            return this.GetSprite(GameAssets.WallTiles.WallFrameOuterCornerTopLeft);
        }

        return this.GetSprite(GameAssets.WallTiles.WallFrameCenter);
    }

    private StaticSprite GetWallTop(int x, int y)
    {
        if (this.IsWallBottomRightOf(x, y))
        {
            if (this.IsWallBottomLeftOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallFrameTopCenter);
            }

            return this.GetSprite(GameAssets.WallTiles.WallFrameTopLeft);
        }

        if (this.IsWallBottomLeftOf(x, y))
        {
            return this.GetSprite(GameAssets.WallTiles.WallFrameTopRight);
        }

        if (this.IsWallRightOf(x, y))
        {
            if (this.IsWallLeftOf(x, y))
            {
                return this.GetSprite(GameAssets.WallTiles.WallFrameTopCenter);
            }

            return this.GetSprite(GameAssets.WallTiles.WallFrameTopLeft);
        }

        if (this.IsWallLeftOf(x, y))
        {
            return this.GetSprite(GameAssets.WallTiles.WallFrameTopRight);
        }

        this.PrintDebug(x, y, 1, "SingleWallColumn");
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

    private bool IsWallAtPosition(int x, int y)
    {
        Debug.Assert(this.Map != null);
        try
        {
            return !IsWalkable(this.Map[x, y]);
        }
        catch
        {
            return true;
        }
    }

    private bool IsWallTopLeftOf(int x, int y)
    {
        return this.IsWallAtPosition(x - 1, y + 1);
    }

    private bool IsWallAbove(int x, int y)
    {
        return this.IsWallAtPosition(x, y + 1);
    }

    private bool IsWallTopRightOf(int x, int y)
    {
        return this.IsWallAtPosition(x + 1, y + 1);
    }

    private bool IsWallLeftOf(int x, int y)
    {
        return this.IsWallAtPosition(x - 1, y);
    }

    private bool IsWallRightOf(int x, int y)
    {
        return this.IsWallAtPosition(x + 1, y);
    }

    private bool IsWallBottomLeftOf(int x, int y)
    {
        return this.IsWallAtPosition(x - 1, y - 1);
    }

    private bool IsWallBeneath(int x, int y)
    {
        return this.IsWallAtPosition(x, y - 1);
    }

    private bool IsWallBottomRightOf(int x, int y)
    {
        return this.IsWallAtPosition(x + 1, y - 1);
    }

    private static bool IsWalkable(TileFlags tile)
    {
        return (tile & TileFlags.Walkable) == TileFlags.Walkable;
    }

    private static bool IsPath(TileFlags tile)
    {
        return (tile & TileFlags.Path) == TileFlags.Path;
    }

    [Conditional("DEBUG")]
    private void PrintDebug(int x, int y, int radius, string context)
    {
        Console.WriteLine($"Mapgen Error ({context}) at {x}, {y}");
        for (int yi = y + radius; yi >= y - radius; yi -= 1)
        {
            for (int xi = x - radius; xi <= x + radius; xi += 1)
            {
                try
                {
                    Console.Write($"{(int)this.Map![xi, yi]} ");
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