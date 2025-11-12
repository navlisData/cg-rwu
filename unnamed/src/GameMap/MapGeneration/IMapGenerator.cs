using unnamed.Enums;

namespace unnamed.GameMap.MapGeneration;

public interface IMapGenerator
{
    void GenerateMap(in TileFlags[,] map);
}