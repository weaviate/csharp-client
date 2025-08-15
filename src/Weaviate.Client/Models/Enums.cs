namespace Weaviate.Client.Models;

public enum HybridFusion
{
    Ranked = 0,
    RelativeScore = 1,
}

public enum NearMediaType
{
    Audio,
    Depth,
    Image,
    IMU,
    Thermal,
    Video,
}
