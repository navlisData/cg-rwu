using OpenTK.Mathematics;

using unnamed.Components.Map;

namespace unnamed.GameMap.MapGeneration;

public interface IMapGenerator
{
    Tile GenerateTile(Vector2i position);
}