namespace unnamed.GameMap.MapGeneration;

public interface IMapGenerator
{
    void GenerateMap(in IntermediateMap map);
}