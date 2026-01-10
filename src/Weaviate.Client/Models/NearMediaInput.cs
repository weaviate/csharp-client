using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Input for near-media searches (Image, Video, Audio, Thermal, etc.).
/// Supports optional target vectors for multi-vector collections.
/// </summary>
/// <param name="Media">The media content as a byte array</param>
/// <param name="Type">The type of media (Image, Video, Audio, Thermal, Depth, IMU)</param>
/// <param name="TargetVectors">Optional target vectors for multi-vector collections</param>
/// <param name="Certainty">Optional certainty threshold for the search</param>
/// <param name="Distance">Optional distance threshold for the search</param>
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
    public delegate NearMediaInput FactoryFn(NearMediaBuilder builder);
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
    NearMediaBuilder Image(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-video search with optional target vectors.
    /// </summary>
    NearMediaBuilder Video(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-audio search with optional target vectors.
    /// </summary>
    NearMediaBuilder Audio(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-thermal search with optional target vectors.
    /// </summary>
    NearMediaBuilder Thermal(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-depth search with optional target vectors.
    /// </summary>
    NearMediaBuilder Depth(byte[] media, float? certainty = null, float? distance = null);

    /// <summary>
    /// Creates a near-IMU search with optional target vectors.
    /// </summary>
    NearMediaBuilder IMU(byte[] media, float? certainty = null, float? distance = null);
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
    NearMediaInput TargetVectorsManualWeights(params (string Name, double Weight)[] targets);

    /// <summary>
    /// Creates a NearMediaInput that sums all target vectors.
    /// </summary>
    /// <param name="targets">Target vector names</param>
    NearMediaInput TargetVectorsSum(params string[] targets);

    /// <summary>
    /// Creates a NearMediaInput that averages all target vectors.
    /// </summary>
    /// <param name="targets">Target vector names</param>
    NearMediaInput TargetVectorsAverage(params string[] targets);

    /// <summary>
    /// Creates a NearMediaInput using minimum combination of target vectors.
    /// </summary>
    /// <param name="targets">Target vector names</param>
    NearMediaInput TargetVectorsMinimum(params string[] targets);

    /// <summary>
    /// Creates a NearMediaInput using relative score combination of target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, weight)</param>
    NearMediaInput TargetVectorsRelativeScore(params (string Name, double Weight)[] targets);

    /// <summary>
    /// Completes the builder without specifying target vectors (uses default behavior).
    /// </summary>
    NearMediaInput Build();
}

/// <summary>
/// Internal implementation of INearMediaBuilder.
/// </summary>
public sealed class NearMediaBuilder : INearMediaBuilderStart, INearMediaBuilder
{
    /// <summary>
    /// The media
    /// </summary>
    private byte[]? _media;

    /// <summary>
    /// The type
    /// </summary>
    private NearMediaType? _type;

    /// <summary>
    /// The certainty
    /// </summary>
    private float? _certainty;

    /// <summary>
    /// The distance
    /// </summary>
    private float? _distance;

    /// <summary>
    /// Images the media
    /// </summary>
    /// <param name="media">The media</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <returns>The near media builder</returns>
    public NearMediaBuilder Image(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Image;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    /// <summary>
    /// Videoes the media
    /// </summary>
    /// <param name="media">The media</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <returns>The near media builder</returns>
    public NearMediaBuilder Video(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Video;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    /// <summary>
    /// Audioes the media
    /// </summary>
    /// <param name="media">The media</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <returns>The near media builder</returns>
    public NearMediaBuilder Audio(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Audio;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    /// <summary>
    /// Thermals the media
    /// </summary>
    /// <param name="media">The media</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <returns>The near media builder</returns>
    public NearMediaBuilder Thermal(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Thermal;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    /// <summary>
    /// Depths the media
    /// </summary>
    /// <param name="media">The media</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <returns>The near media builder</returns>
    public NearMediaBuilder Depth(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.Depth;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    /// <summary>
    /// Imus the media
    /// </summary>
    /// <param name="media">The media</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <returns>The near media builder</returns>
    public NearMediaBuilder IMU(byte[] media, float? certainty = null, float? distance = null)
    {
        _media = media;
        _type = NearMediaType.IMU;
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    /// <summary>
    /// Targets the vectors manual weights using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The near media input</returns>
    public NearMediaInput TargetVectorsManualWeights(params (string Name, double Weight)[] targets)
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

    /// <summary>
    /// Targets the vectors sum using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The near media input</returns>
    public NearMediaInput TargetVectorsSum(params string[] targets)
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

    /// <summary>
    /// Targets the vectors average using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The near media input</returns>
    public NearMediaInput TargetVectorsAverage(params string[] targets)
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

    /// <summary>
    /// Targets the vectors minimum using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The near media input</returns>
    public NearMediaInput TargetVectorsMinimum(params string[] targets)
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

    /// <summary>
    /// Targets the vectors relative score using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The near media input</returns>
    public NearMediaInput TargetVectorsRelativeScore(params (string Name, double Weight)[] targets)
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

    /// <summary>
    /// Builds this instance
    /// </summary>
    /// <returns>The near media input</returns>
    public NearMediaInput Build()
    {
        ValidateMedia();
        return new NearMediaInput(_media!, _type!.Value, null, _certainty, _distance);
    }

    /// <summary>
    /// Validates the media
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Implicitly converts a NearMediaBuilder to NearMediaInput
    /// </summary>
    /// <param name="builder">The builder</param>
    public static implicit operator NearMediaInput(NearMediaBuilder builder) => builder.Build();
}
