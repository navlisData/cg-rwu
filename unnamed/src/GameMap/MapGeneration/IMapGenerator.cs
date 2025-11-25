using OpenTK.Mathematics;

namespace unnamed.GameMap.MapGeneration;

public interface IMapGenerator
{
    List<Vector2i> GenerateMap(in IntermediateMap map);
}