using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying the NearMedia search input syntax using NearMediaInput.FactoryFn lambda builders.
/// Target vectors MUST be specified via lambda builders - there is no separate targets parameter.
/// </summary>
[Collection("Unit Tests")]
public class TestNearMediaSyntax : IAsyncLifetime
{
    private const string CollectionName = "TestCollection";
    private static readonly byte[] TestMediaBytes = new byte[] { 1, 2, 3, 4, 5 };

    private Func<Grpc.Protobuf.V1.SearchRequest?> _getRequest = null!;
    private CollectionClient _collection = null!;

    public ValueTask InitializeAsync()
    {
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        _getRequest = getRequest;
        _collection = client.Collections.Use(CollectionName);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    #region Simple Media Type Tests

    [Fact]
    public async Task NearMedia_Image_WithoutTargetVectors_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearImage.Image);
    }

    [Fact]
    public async Task NearMedia_Video_WithoutTargetVectors_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Video(TestMediaBytes).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVideo);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearVideo.Video);
    }

    [Fact]
    public async Task NearMedia_Audio_WithoutTargetVectors_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Audio(TestMediaBytes).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearAudio);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearAudio.Audio);
    }

    [Fact]
    public async Task NearMedia_Thermal_WithoutTargetVectors_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Thermal(TestMediaBytes).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearThermal);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearThermal.Thermal);
    }

    [Fact]
    public async Task NearMedia_Depth_WithoutTargetVectors_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Depth(TestMediaBytes).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearDepth);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearDepth.Depth);
    }

    [Fact]
    public async Task NearMedia_IMU_WithoutTargetVectors_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.IMU(TestMediaBytes).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImu);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearImu.Imu);
    }

    #endregion

    #region Certainty and Distance Tests

    [Fact]
    public async Task NearMedia_Image_WithCertainty_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes, certainty: 0.8f).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.Equal(0.8, request.NearImage.Certainty, precision: 5);
    }

    [Fact]
    public async Task NearMedia_Video_WithDistance_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Video(TestMediaBytes, distance: 0.3f).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVideo);
        Assert.Equal(0.3, request.NearVideo.Distance, precision: 5);
    }

    [Fact]
    public async Task NearMedia_Audio_WithBothCertaintyAndDistance_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Audio(TestMediaBytes, certainty: 0.7f, distance: 0.2f).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearAudio);
        Assert.Equal(0.7, request.NearAudio.Certainty, precision: 5);
        Assert.Equal(0.2, request.NearAudio.Distance, precision: 5);
    }

    #endregion

    #region Target Vectors - Sum

    [Fact]
    public async Task NearMedia_Image_WithSum_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes).Sum("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.NotNull(request.NearImage.Targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearImage.Targets.Combination);
        Assert.Equal(2, request.NearImage.Targets.TargetVectors.Count);
        Assert.Contains("title", request.NearImage.Targets.TargetVectors);
        Assert.Contains("description", request.NearImage.Targets.TargetVectors);
    }

    [Fact]
    public async Task NearMedia_Video_WithSum_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Video(TestMediaBytes, certainty: 0.8f).Sum("visual", "audio", "metadata"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVideo);
        Assert.Equal(0.8, request.NearVideo.Certainty, precision: 5);
        Assert.NotNull(request.NearVideo.Targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearVideo.Targets.Combination);
        Assert.Equal(3, request.NearVideo.Targets.TargetVectors.Count);
    }

    #endregion

    #region Target Vectors - Average

    [Fact]
    public async Task NearMedia_Audio_WithAverage_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Audio(TestMediaBytes).Average("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearAudio);
        Assert.NotNull(request.NearAudio.Targets);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.NearAudio.Targets.Combination);
        Assert.Equal(2, request.NearAudio.Targets.TargetVectors.Count);
    }

    #endregion

    #region Target Vectors - ManualWeights

    [Fact]
    public async Task NearMedia_Image_WithManualWeights_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m =>
                m.Image(TestMediaBytes, distance: 0.3f)
                    .ManualWeights(("title", 1.2), ("description", 0.8)),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.Equal(0.3, request.NearImage.Distance, precision: 5);
        Assert.NotNull(request.NearImage.Targets);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.NearImage.Targets.Combination);
        Assert.Equal(2, request.NearImage.Targets.WeightsForTargets.Count);
    }

    #endregion

    #region Target Vectors - Minimum

    [Fact]
    public async Task NearMedia_Thermal_WithMinimum_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Thermal(TestMediaBytes).Minimum("v1", "v2", "v3"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearThermal);
        Assert.NotNull(request.NearThermal.Targets);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.NearThermal.Targets.Combination);
        Assert.Equal(3, request.NearThermal.Targets.TargetVectors.Count);
    }

    #endregion

    #region Target Vectors - RelativeScore

    [Fact]
    public async Task NearMedia_Depth_WithRelativeScore_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Depth(TestMediaBytes).RelativeScore(("visual", 0.7), ("depth", 0.3)),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearDepth);
        Assert.NotNull(request.NearDepth.Targets);
        Assert.Equal(V1.CombinationMethod.TypeRelativeScore, request.NearDepth.Targets.Combination);
        Assert.Equal(2, request.NearDepth.Targets.WeightsForTargets.Count);
    }

    #endregion

    #region GroupBy Tests

    [Fact]
    public async Task NearMedia_Image_WithGroupBy_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes).Sum("title", "description"),
            new GroupByRequest("category") { ObjectsPerGroup = 5 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("category", request.GroupBy.Path[0]);
        Assert.Equal(5, request.GroupBy.ObjectsPerGroup);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearImage.Targets.Combination);
    }

    [Fact]
    public async Task NearMedia_Video_WithGroupBy_WithoutTargets_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Video(TestMediaBytes, certainty: 0.75f).Build(),
            new GroupByRequest("status") { ObjectsPerGroup = 3 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVideo);
        Assert.Equal(0.75, request.NearVideo.Certainty, precision: 5);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("status", request.GroupBy.Path[0]);
        Assert.Equal(3, request.GroupBy.ObjectsPerGroup);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public async Task NearMedia_Image_WithLimitAndAutoLimit_ProducesValidRequest()
    {
        // Act - Test with limit and autoLimit parameters
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes, certainty: 0.8f).ManualWeights(("v1", 1.2), ("v2", 0.8)),
            limit: 10,
            autoLimit: 3,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.Equal(0.8, request.NearImage.Certainty, precision: 5);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.NearImage.Targets.Combination);
        Assert.Equal(3, (int)request.Autocut);
    }

    [Fact]
    public async Task NearMedia_AllMediaTypes_WithSameTargets_ProduceConsistentRequests()
    {
        // Test that all media types produce consistent target vector structure
        var mediaTypes = new[]
        {
            (NearMediaInput.FactoryFn)(m => m.Image(TestMediaBytes).Sum("v1", "v2")),
            (NearMediaInput.FactoryFn)(m => m.Video(TestMediaBytes).Sum("v1", "v2")),
            (NearMediaInput.FactoryFn)(m => m.Audio(TestMediaBytes).Sum("v1", "v2")),
            (NearMediaInput.FactoryFn)(m => m.Thermal(TestMediaBytes).Sum("v1", "v2")),
            (NearMediaInput.FactoryFn)(m => m.Depth(TestMediaBytes).Sum("v1", "v2")),
            (NearMediaInput.FactoryFn)(m => m.IMU(TestMediaBytes).Sum("v1", "v2")),
        };

        foreach (var mediaBuilder in mediaTypes)
        {
            // Reinitialize to clear previous request
            await DisposeAsync();
            await InitializeAsync();

            // Act
            await _collection.Query.NearMedia(
                mediaBuilder,
                cancellationToken: TestContext.Current.CancellationToken
            );

            // Assert
            var request = _getRequest();
            Assert.NotNull(request);

            // Each media type should have its specific field set with consistent target vectors
            var hasMediaField =
                request.NearImage != null
                || request.NearVideo != null
                || request.NearAudio != null
                || request.NearThermal != null
                || request.NearDepth != null
                || request.NearImu != null;
            Assert.True(hasMediaField, "Request should have at least one media field set");

            // Get the targets from whichever media field is set
            V1.Targets? targets = null;
            if (request.NearImage != null)
                targets = request.NearImage.Targets;
            else if (request.NearVideo != null)
                targets = request.NearVideo.Targets;
            else if (request.NearAudio != null)
                targets = request.NearAudio.Targets;
            else if (request.NearThermal != null)
                targets = request.NearThermal.Targets;
            else if (request.NearDepth != null)
                targets = request.NearDepth.Targets;
            else if (request.NearImu != null)
                targets = request.NearImu.Targets;

            Assert.NotNull(targets);
            Assert.Equal(V1.CombinationMethod.TypeSum, targets.Combination);
            Assert.Equal(2, targets.TargetVectors.Count);
        }
    }

    #endregion

    #region Implicit Conversion Tests (Without .Build())

    [Fact]
    public async Task NearMedia_Image_WithoutBuild_ImplicitConversion_ProducesValidRequest()
    {
        // Act - No .Build() call, tests implicit conversion
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearImage.Image);
    }

    [Fact]
    public async Task NearMedia_Video_WithoutBuild_ImplicitConversion_ProducesValidRequest()
    {
        // Act - No .Build() call
        await _collection.Query.NearMedia(
            m => m.Video(TestMediaBytes),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVideo);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearVideo.Video);
    }

    [Fact]
    public async Task NearMedia_Audio_WithoutBuild_ImplicitConversion_ProducesValidRequest()
    {
        // Act - No .Build() call
        await _collection.Query.NearMedia(
            m => m.Audio(TestMediaBytes),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearAudio);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearAudio.Audio);
    }

    [Fact]
    public async Task NearMedia_Thermal_WithCertainty_WithoutBuild_ImplicitConversion_ProducesValidRequest()
    {
        // Act - No .Build() call, with certainty parameter
        await _collection.Query.NearMedia(
            m => m.Thermal(TestMediaBytes, certainty: 0.85f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearThermal);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearThermal.Thermal);
        Assert.Equal(0.85, request.NearThermal.Certainty, precision: 5);
    }

    [Fact]
    public async Task NearMedia_Depth_WithDistance_WithoutBuild_ImplicitConversion_ProducesValidRequest()
    {
        // Act - No .Build() call, with distance parameter
        await _collection.Query.NearMedia(
            m => m.Depth(TestMediaBytes, distance: 0.25f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearDepth);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearDepth.Depth);
        Assert.Equal(0.25, request.NearDepth.Distance, precision: 5);
    }

    [Fact]
    public async Task NearMedia_IMU_WithBothParams_WithoutBuild_ImplicitConversion_ProducesValidRequest()
    {
        // Act - No .Build() call, with both certainty and distance
        await _collection.Query.NearMedia(
            m => m.IMU(TestMediaBytes, certainty: 0.9f, distance: 0.1f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImu);
        Assert.Equal(Convert.ToBase64String(TestMediaBytes), request.NearImu.Imu);
        Assert.Equal(0.9, request.NearImu.Certainty, precision: 5);
        Assert.Equal(0.1, request.NearImu.Distance, precision: 5);
    }

    [Fact]
    public async Task NearMedia_AllMediaTypes_WithoutBuild_ImplicitConversion_ProducesConsistentRequests()
    {
        // Test that all media types work without .Build() via implicit conversion
        var mediaTypes = new[]
        {
            (NearMediaInput.FactoryFn)(m => m.Image(TestMediaBytes)),
            (NearMediaInput.FactoryFn)(m => m.Video(TestMediaBytes)),
            (NearMediaInput.FactoryFn)(m => m.Audio(TestMediaBytes)),
            (NearMediaInput.FactoryFn)(m => m.Thermal(TestMediaBytes)),
            (NearMediaInput.FactoryFn)(m => m.Depth(TestMediaBytes)),
            (NearMediaInput.FactoryFn)(m => m.IMU(TestMediaBytes)),
        };

        foreach (var mediaBuilder in mediaTypes)
        {
            // Reinitialize to clear previous request
            await DisposeAsync();
            await InitializeAsync();

            // Act - No .Build() calls
            await _collection.Query.NearMedia(
                mediaBuilder,
                cancellationToken: TestContext.Current.CancellationToken
            );

            // Assert
            var request = _getRequest();
            Assert.NotNull(request);

            // Each media type should have its specific field set
            var hasMediaField =
                request.NearImage != null
                || request.NearVideo != null
                || request.NearAudio != null
                || request.NearThermal != null
                || request.NearDepth != null
                || request.NearImu != null;
            Assert.True(hasMediaField, "Request should have at least one media field set");
        }
    }

    [Fact]
    public async Task NearMedia_WithGroupBy_WithoutBuild_ImplicitConversion_ProducesValidRequest()
    {
        // Act - No .Build() call, with GroupBy
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes, certainty: 0.75f),
            new GroupByRequest("status") { ObjectsPerGroup = 3 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.Equal(0.75, request.NearImage.Certainty, precision: 5);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("status", request.GroupBy.Path[0]);
        Assert.Equal(3, request.GroupBy.ObjectsPerGroup);
    }

    #endregion
}
