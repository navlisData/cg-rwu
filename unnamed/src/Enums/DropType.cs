using Engine.Ecs;

using engine.TextureProcessing;

using unnamed.Components.Drops;
using unnamed.Texture;

namespace unnamed.Enums;

public enum DropType : byte
{
    UpdateHealthDrop = 0,
    MaxHealthDrop = 1
}

public static class DropTypeExtensions
{
    public static AssetRef<StaticSprite> GetAsset(this DropType type) => type switch
    {
        DropType.UpdateHealthDrop => GameAssets.Drops.RedHeart,
        DropType.MaxHealthDrop => GameAssets.Drops.BlueHeart,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static void AddDefaultDropComponent(this EntityHandle handle, DropType type)
    {
        switch (type)
        {
            case DropType.UpdateHealthDrop:
                handle.Add(UpdateHealthDrop.Default);
                break;

            case DropType.MaxHealthDrop:
                handle.Add(MaxHealthDrop.Default);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}