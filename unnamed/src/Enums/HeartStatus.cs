using engine.TextureProcessing;

using unnamed.Texture;

namespace unnamed.Enums;

public enum HeartStatus : byte
{
    Empty = 0,
    Half = 1,
    Full = 2,
}

public static class HeartStatusExtensions
{
    /// <summary>
    ///     Returns the asset according to the heart-status
    /// </summary>
    /// <param name="status">The heart-status</param>
    /// <returns>The asset matching the given <paramref name="status"/>.</returns>
    public static AssetRef<StaticSprite> GetAsset(this HeartStatus status) => status switch
    {
        HeartStatus.Empty => GameAssets.Hearts.Empty,
        HeartStatus.Half => GameAssets.Hearts.Half,
        HeartStatus.Full => GameAssets.Hearts.Full,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}