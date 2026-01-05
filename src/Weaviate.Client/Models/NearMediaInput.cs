using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Input for near-media searches (Image, Video, Audio, Thermal, etc.).
/// Supports optional target vectors for multi-vector collections.
/// </summary>
public record NearMediaInput(
    byte[] Media,
    NearMediaType Type,
    TargetVectors? TargetVectors = null,
    float? Certainty = null,
    float? Distance = null
)
{
    /// <summary>
    /// Delegate for lambda builder pattern with NearMediaInput.
    /// </summary>
    /// <example>
    /// m => m.Image(imageBytes).Sum("vector1", "vector2")
    /// m => m.Video(videoBytes).ManualWeights(("title", 1.2), ("desc", 0.8))
    /// m => m.Audio(audioBytes).Build()  // No targets, uses default vector
    /// </example>
    public delegate NearMediaInput FactoryFn(INearMediaBuilderStart builder);
}

// ============================================================================
// NearMedia Builder Infrastructure
// ============================================================================

/// <summary>
/// Starting point for NearMedia builder - select media type first.
/// </summary>
public interface INearMediaBuilderStart
{
    /// <summary>
    /// Creates a near-image search with optional target vectors.
    /// </summary>
    INearMediaBuilder Image(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-video search with optional target vectors.
    /// </summary>
    INearMediaBuilder Video(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-audio search with optional target vectors.
    /// </summary>
    INearMediaBuilder Audio(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-thermal search with optional target vectors.
    /// </summary>
    INearMediaBuilder Thermal(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-depth search with optional target vectors.
    /// </summary>
    INearMediaBuilder Depth(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-IMU search with optional target vectors.
    /// </summary>
    INearMediaBuilder IMU(byte[] media, float? certainty = null, float? distance = null);
}

/// <summary>
/// Builder interface for creating NearMediaInput with optional target vectors.
/// Can be used directly (for no targets) or chained with target vector methods.
/// </summary>
public interface INearMediaBuilder
{
    /// <summary>
    /// Creates a NearMediaInput with manually weighted target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, weight)</param>
    NearMediaInput ManualWeights(params (string Name, double Weight)[] targets);

    /// <summary>
    /// Creates a NearMediaInput that sums all target vectors.
    /// </summary>
    /// <param name="targets">Target vector names</param>
    NearMediaInput Sum(params string[] targets);

    /// <summary>
    /// Creates a NearMediaInput that averages all target vectors.
    /// </summary>
    /// <param name="targets">Target vector names</param>
    NearMediaInput Average(params string[] targets);

    /// <summary>
    /// Creates a NearMediaInput using minimum combination of target vectors.
    /// </summary>
    /// <param name="targets">Target vector names</param>
    NearMediaInput Minimum(params string[] targets);

    /// <summary>
    /// Creates a NearMediaInput using relative score combination of target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, weight)</param>
    NearMediaInput RelativeScore(params (string Name, double Weight)[] targets);

    /// <summary>
    /// Completes the builder without specifying target vectors (uses default behavior).
    /// </summary>
    NearMediaInput Build();
}

/// <summary>
/// Internal implementation of INearMediaBuilder.
/// </summary>
internal sealed class NearMediaBuilder : INearMediaBuilderStart, INearMediaBuilder
{
    private byte[]? _media;
    private NearMediaType? _type;
    private float? _certainty;
    private float? _distance;

    public INearMediaBuilder Image(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Image;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    public INearMediaBuilder Video(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Video;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    public INearMediaBuilder Audio(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Audio;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    public INearMediaBuilder Thermal(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Thermal;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    public INearMediaBuilder Depth(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Depth;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    public INearMediaBuilder IMU(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.IMU;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    public NearMediaInput ManualWeights(params (string Name, double Weight)[] targets)
    {
        ValidateMedia();
        return new NearMediaInput(
            _media!,
            _type!.Value,
            TargetVectors.ManualWeights(targets),
            _certainty,
            _distance
        );
    }

    public NearMediaInput Sum(params string[] targets)
    {
        ValidateMedia();
        return new NearMediaInput(
            _media!,
            _type!.Value,
            TargetVectors.Sum(targets),
            _certainty,
            _distance
        );
    }

    public NearMediaInput Average(params string[] targets)
    {
        ValidateMedia();
        return new NearMediaInput(
            _media!,
            _type!.Value,
            TargetVectors.Average(targets),
            _certainty,
            _distance
        );
    }

    public NearMediaInput Minimum(params string[] targets)
    {
        ValidateMedia();
        return new NearMediaInput(
            _media!,
            _type!.Value,
            TargetVectors.Minimum(targets),
            _certainty,
            _distance
        );
    }

    public NearMediaInput RelativeScore(params (string Name, double Weight)[] targets)
    {
        ValidateMedia();
        return new NearMediaInput(
            _media!,
            _type!.Value,
            TargetVectors.RelativeScore(targets),
            _certainty,
            _distance
        );
    }

    public NearMediaInput Build()
    {
        ValidateMedia();
        return new NearMediaInput(_media!, _type!.Value, null, _certainty, _distance);
    }

    private void ValidateMedia()
    {
        if (_media == null || _type == null)
        {
            throw new InvalidOperationException(
                "Media type and data must be set before building. "
                    + "Call Image(), Video(), Audio(), Thermal(), Depth(), or IMU() first."
            );
        }
    }

    // Implicit conversion to allow usage without calling Build()
    public static implicit operator NearMediaInput(NearMediaBuilder builder) => builder.Build();
}
