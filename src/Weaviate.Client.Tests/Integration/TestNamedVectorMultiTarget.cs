namespace Weaviate.Client.Tests.Integration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weaviate.Client.Models;
using Xunit;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1861:Avoid constant arrays as arguments",
    Justification = "Irrelevant"
)]
public class TestNamedVectorMultiTarget : IntegrationTests
{
    [Fact]
    public async Task Test_NamedVector_MultiTargetVectorPerTarget()
    {
        RequireVersion("1.26.0");

        var collection = await CollectionFactory(
            vectorConfig:
            [
                Configure.Vector("first", v => v.SelfProvided()),
                Configure.Vector("second", v => v.SelfProvided()),
            ]
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f } },
                { "second", new[] { 1f, 0f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var vectorsObj = new Vectors
        {
            { "first", new[] { 1f, 0f } },
            { "second", new[] { 1f, 0f, 0f } },
        };
        var objs = await collection.Query.NearVector(
            vectorsObj,
            cancellationToken: TestContext.Current.CancellationToken
        );
        var ids = objs.Select(o => o.UUID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid1, uuid2 }.OrderBy(x => x).ToList();
        Assert.Equal(expected, ids);
    }

    public static TheoryData<VectorSearchInput> MultiInputCombinations =>
        new(
            [
                ("first", new[] { 0f, 1f }),
                ("second", new[] { 1f, 0f, 0f }),
                ("second", new[] { 0f, 0f, 1f }),
            ],
            [
                ("first", new[] { 0f, 1f }),
                ("first", new[] { 0f, 1f }),
                ("second", new[] { 1f, 0f, 0f }),
            ],
            [
                ("first", new[] { 0f, 1f }),
                ("first", new[] { 0f, 1f }),
                ("second", new[] { 1f, 0f, 0f }),
                ("second", new[] { 0f, 0f, 1f }),
            ]
        );

    [Theory]
    [MemberData(nameof(MultiInputCombinations))]
    public async Task Test_SameTargetVector_MultipleInputCombinations(VectorSearchInput nearVector)
    {
        RequireVersion("1.27.0");

        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vector("first", v => v.SelfProvided()),
                Configure.Vector("second", v => v.SelfProvided()),
            }
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f } },
                { "second", new[] { 1f, 0f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = await collection.Query.NearVector(
            vectors: nearVector,
            cancellationToken: TestContext.Current.CancellationToken
        );
        var ids = objs.Select(o => o.UUID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid2, uuid1 }.OrderBy(x => x).ToList();
        Assert.Equal(expected, ids);
    }

    public static IEnumerable<object[]> HybridMultiInputCombinations =>
        new List<object[]>
        {
            new object[]
            {
                new NearVectorInput(
                    Vector: new Vectors
                    {
                        { "first", new[] { 0f, 1f } },
                        {
                            "second",
                            new float[,]
                            {
                                { 1f, 0f, 0f },
                                { 0f, 0f, 1f },
                            }
                        },
                    }
                ),
            },
            new object[]
            {
                new NearVectorInput(
                    Vector: new Vectors
                    {
                        {
                            "first",
                            new float[,]
                            {
                                { 0f, 1f },
                                { 0f, 1f },
                            }
                        },
                        { "second", new[] { 1f, 0f, 0f } },
                    }
                ),
            },
            new object[]
            {
                new NearVectorInput(
                    Vector: new Vectors
                    {
                        {
                            "first",
                            new float[,]
                            {
                                { 0f, 1f },
                                { 0f, 1f },
                            }
                        },
                        {
                            "second",
                            new float[,]
                            {
                                { 1f, 0f, 0f },
                                { 0f, 0f, 1f },
                            }
                        },
                    }
                ),
            },
            // The following are equivalent to above, but mimic the Python HybridVector.near_vector usage
            new object[]
            {
                new NearVectorInput(
                    Vector: new Vectors
                    {
                        { "first", new[] { 0f, 1f } },
                        {
                            "second",
                            new float[,]
                            {
                                { 1f, 0f, 0f },
                                { 0f, 0f, 1f },
                            }
                        },
                    },
                    Certainty: null,
                    Distance: null
                ),
            },
            new object[]
            {
                new NearVectorInput(
                    Vector: new Vectors
                    {
                        {
                            "first",
                            new float[,]
                            {
                                { 0f, 1f },
                                { 0f, 1f },
                            }
                        },
                        { "second", new[] { 1f, 0f, 0f } },
                    }
                ),
            },
            new object[]
            {
                new NearVectorInput(
                    Vector: new Vectors
                    {
                        {
                            "first",
                            new float[,]
                            {
                                { 0f, 1f },
                                { 0f, 1f },
                            }
                        },
                        {
                            "second",
                            new float[,]
                            {
                                { 1f, 0f, 0f },
                                { 0f, 0f, 1f },
                            }
                        },
                    }
                ),
            },
        };

    [Theory]
    [MemberData(nameof(HybridMultiInputCombinations))]
    public async Task Test_SameTargetVector_MultipleInputCombinations_Hybrid(
        HybridVectorInput nearVector
    )
    {
        RequireVersion("1.27.0");

        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vector("first", v => v.SelfProvided()),
                Configure.Vector("second", v => v.SelfProvided()),
            }
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f } },
                { "second", new[] { 1f, 0f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = await collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            returnMetadata: MetadataOptions.All,
            cancellationToken: TestContext.Current.CancellationToken
        );
        var ids = objs.Objects.Select(o => o.UUID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid2, uuid1 }.OrderBy(x => x).ToList();
        Assert.Equal(expected, ids);
    }

    /// <summary>
    /// Ported from Python: test_same_target_vector_multiple_input
    /// Tests multiple vectors for the same target with Sum combination and distance verification.
    /// </summary>
    public static TheoryData<
        Func<VectorSearchInput.Builder, VectorSearchInput>,
        float[]
    > MultiTargetVectorsWithDistances =>
        new()
        {
            // Sum combination: first has 1 vector, second has 2 vectors
            {
                tv =>
                    tv.Sum(
                        ("first", new[] { 0f, 1f }),
                        ("second", new[] { 1f, 0f, 0f }),
                        ("second", new[] { 0f, 0f, 1f })
                    ),
                new[] { 1.0f, 3.0f }
            },
            // ManualWeights: weight 1 for each vector
            {
                tv =>
                    tv.ManualWeights(
                        ("first", 1.0, new[] { 0f, 1f }),
                        ("second", 1.0, new[] { 1f, 0f, 0f }),
                        ("second", 1.0, new[] { 0f, 0f, 1f })
                    ),
                new[] { 1.0f, 3.0f }
            },
            // ManualWeights: different weights (1 for first, 1 and 2 for second)
            {
                tv =>
                    tv.ManualWeights(
                        ("first", 1.0, new[] { 0f, 1f }),
                        ("second", 1.0, new[] { 1f, 0f, 0f }),
                        ("second", 2.0, new[] { 0f, 0f, 1f })
                    ),
                new[] { 2.0f, 4.0f }
            },
            // ManualWeights: same weights but different order (second before first)
            {
                tv =>
                    tv.ManualWeights(
                        ("second", 1.0, new[] { 1f, 0f, 0f }),
                        ("second", 2.0, new[] { 0f, 0f, 1f }),
                        ("first", 1.0, new[] { 0f, 1f })
                    ),
                new[] { 2.0f, 4.0f }
            },
        };

    [Theory]
    [MemberData(nameof(MultiTargetVectorsWithDistances))]
    public async Task Test_SameTargetVector_MultipleInput(
        Func<VectorSearchInput.Builder, VectorSearchInput> nearVectorInput,
        float[] expectedDistances
    )
    {
        RequireVersion("1.27.0");

        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vector("first", v => v.SelfProvided()),
                Configure.Vector("second", v => v.SelfProvided()),
            }
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f } },
                { "second", new[] { 1f, 0f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = await collection.Query.NearVector(
            nearVectorInput,
            returnMetadata: MetadataOptions.All,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // uuid2 should be first (closer match)
        Assert.Equal(2, objs.Count());
        Assert.Equal(uuid2, objs.ElementAt(0).UUID);
        Assert.Equal(uuid1, objs.ElementAt(1).UUID);
        Assert.Equal(expectedDistances[0], objs.ElementAt(0).Metadata.Distance);
        Assert.Equal(expectedDistances[1], objs.ElementAt(1).Metadata.Distance);
    }

    /// <summary>
    /// Ported from Python: test_named_vector_multi_target
    /// Tests various combination methods (Sum, Average, Minimum, ManualWeights, RelativeScore).
    /// Uses VectorSearchInput.Combine to combine TargetVectors with shared vector data.
    /// </summary>
    private static readonly Vectors SharedQueryVectors = new()
    {
        { "first", new[] { 1f, 0f, 0f } },
        { "second", new[] { 1f, 0f, 0f } },
    };

    public static TheoryData<TargetVectors, string> MultiTargetVectors =>
        new()
        {
            { new[] { "first", "second" }, "Targets" },
            { TargetVectors.Sum("first", "second"), "Sum" },
            { TargetVectors.Minimum("first", "second"), "Minimum" },
            { TargetVectors.Average("first", "second"), "Average" },
            { TargetVectors.ManualWeights(("first", 1.2), ("second", 0.7)), "ManualWeights" },
            { TargetVectors.RelativeScore(("first", 1.2), ("second", 0.7)), "RelativeScore" },
        };

    [Theory]
    [MemberData(nameof(MultiTargetVectors))]
    public async Task Test_NamedVector_MultiTarget(TargetVectors targets, string combinationName)
    {
        RequireVersion("1.26.0");

        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vector("first", v => v.SelfProvided()),
                Configure.Vector("second", v => v.SelfProvided()),
            }
        );

        // Both vectors are 3D so we can query with a single 3D vector
        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f, 0f } },
                { "second", new[] { 1f, 0f, 0f } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Query using VectorSearchInput.Combine to pair targets with shared vectors
        var objs = await collection.Query.NearVector(
            VectorSearchInput.Combine(targets, SharedQueryVectors),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var ids = objs.Select(o => o.UUID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid1, uuid2 }.OrderBy(x => x).ToList();
        Assert.True(
            expected.SequenceEqual(ids),
            $"Expected both objects to be returned with combination method: {combinationName}"
        );
    }
}
