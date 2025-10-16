namespace Weaviate.Client.Tests.Integration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weaviate.Client.Models;
using Xunit;

public class TestNamedVectorMultiTarget : IntegrationTests
{
    [Fact]
    public async Task Test_NamedVector_MultiTargetVectorPerTarget()
    {
        RequireVersion("1.26.0");

        var collection = await CollectionFactory(
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(name: "first"),
                Configure.Vectors.SelfProvided(name: "second"),
            }
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            }
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f } },
                { "second", new[] { 1f, 0f, 0f } },
            }
        );

        var objs = await collection.Query.NearVector(
            new Vectors { { "first", new[] { 1f, 0f } }, { "second", new[] { 1f, 0f, 0f } } },
            targetVector: ["first", "second"]
        );
        var ids = objs.Select(o => o.ID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid1, uuid2 }.OrderBy(x => x).ToList();
        Assert.Equal(expected, ids);
    }

    public static TheoryData<Vectors, string[]> MultiInputCombinations =>
        new(
            (
                new Vectors
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
                new[] { "first", "second" }
            ),
            (
                new Vectors
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
                new[] { "first", "second" }
            ),
            (
                new Vectors
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
                },
                new[] { "first", "second" }
            ),
            (
                new Vectors
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
                },
                new[] { "first", "second" }
            ),
            (
                new Vectors
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
                },
                new[] { "second", "first" }
            )
        );

    [Theory]
    [MemberData(nameof(MultiInputCombinations))]
    public async Task Test_SameTargetVector_MultipleInputCombinations(
        Vectors nearVector,
        string[] targetVector
    )
    {
        RequireVersion("1.27.0");

        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(name: "first"),
                Configure.Vectors.SelfProvided(name: "second"),
            }
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            }
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f } },
                { "second", new[] { 1f, 0f, 0f } },
            }
        );

        var objs = await collection.Query.NearVector(nearVector, targetVector: targetVector);
        var ids = objs.Select(o => o.ID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid2, uuid1 }.OrderBy(x => x).ToList();
        Assert.Equal(expected, ids);
    }

    public static IEnumerable<object[]> HybridMultiInputCombinations =>
        new List<object[]>
        {
            new object[]
            {
                new Vectors
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
                new[] { "first", "second" },
            },
            new object[]
            {
                new Vectors
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
                },
                new[] { "first", "second" },
            },
            new object[]
            {
                new Vectors
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
                },
                new[] { "first", "second" },
            },
            // The following are equivalent to above, but mimic the Python HybridVector.near_vector usage
            new object[]
            {
                new HybridNearVector(
                    new Vectors
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
                new[] { "first", "second" },
            },
            new object[]
            {
                new HybridNearVector(
                    new Vectors
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
                new[] { "first", "second" },
            },
            new object[]
            {
                new HybridNearVector(
                    new Vectors
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
                new[] { "first", "second" },
            },
        };

    [Theory]
    [MemberData(nameof(HybridMultiInputCombinations))]
    public async Task Test_SameTargetVector_MultipleInputCombinations_Hybrid(
        IHybridVectorInput nearVector,
        string[] targetVector
    )
    {
        RequireVersion("1.27.0");

        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(name: "first"),
                Configure.Vectors.SelfProvided(name: "second"),
            }
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            }
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f } },
                { "second", new[] { 1f, 0f, 0f } },
            }
        );

        var objs = await collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            targetVector: targetVector,
            returnMetadata: MetadataOptions.All
        );
        var ids = objs.Objects.Select(o => o.ID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid2, uuid1 }.OrderBy(x => x).ToList();
        Assert.Equal(expected, ids);
    }

    public static IEnumerable<object[]> MultiTargetVectorsWithDistances =>
        new List<object[]>
        {
            new object[] { TargetVectors.Sum(["first", "second"]), new float[] { 1.0f, 3.0f } },
            new object[]
            {
                TargetVectors.ManualWeights(("first", 1.0f), ("second", [1.0f, 1.0f])),
                new float[] { 1.0f, 3.0f },
            },
            new object[]
            {
                TargetVectors.ManualWeights(("first", 1.0f), ("second", [1.0f, 2.0f])),
                new float[] { 2.0f, 4.0f },
            },
            new object[]
            {
                TargetVectors.ManualWeights(("second", [1.0f, 2.0f]), ("first", 1.0f)),
                new float[] { 2.0f, 4.0f },
            },
        };

    [Theory]
    [MemberData(nameof(MultiTargetVectorsWithDistances))]
    public async Task Test_SameTargetVector_MultipleInput(
        TargetVectors targetVector,
        float[] expectedDistances
    )
    {
        RequireVersion("1.26.0");

        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(name: "first"),
                Configure.Vectors.SelfProvided(name: "second"),
            }
        );

        var inserts = (
            await collection.Data.InsertMany(
                new BatchInsertRequest<object>(
                    Data: new { },
                    Vectors: new Vectors
                    {
                        { "first", new[] { 1f, 0f } },
                        { "second", new[] { 0f, 1f, 0f } },
                    }
                ),
                new BatchInsertRequest<object>(
                    Data: new { },
                    Vectors: new Vectors
                    {
                        { "first", new[] { 0f, 1f } },
                        { "second", new[] { 1f, 0f, 0f } },
                    }
                )
            )
        ).ToList();

        var results = (
            await collection.Query.FetchObjects(returnMetadata: MetadataOptions.All)
        ).ToList();

        var uuid1 = results[0].ID!.Value;
        var uuid2 = results[1].ID!.Value;

        var objs = await collection.Query.NearVector(
            new Vectors
            {
                { "first", new[] { 0f, 1f } },
                {
                    "second",
                    new[,]
                    {
                        { 1f, 0f, 0f },
                        { 0f, 0f, 1f },
                    }
                },
            },
            targetVector: targetVector,
            returnMetadata: MetadataOptions.All
        );
        var ids = objs.Select(o => o.ID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid1, uuid2 }.OrderBy(x => x).ToList();
        Assert.Equal(expected, ids);
        Assert.Equal(expectedDistances.Length, objs.Count());
        Assert.Equal(expectedDistances[0], objs.ElementAt(0).Metadata.Distance);
        Assert.Equal(expectedDistances[1], objs.ElementAt(1).Metadata.Distance);
    }

    public static IEnumerable<object[]> MultiTargetVectors =>
        new List<object[]>
        {
            new object[] { (TargetVectors)new[] { "first", "second" } },
            new object[] { TargetVectors.Sum(new[] { "first", "second" }) },
            new object[] { TargetVectors.Minimum(new[] { "first", "second" }) },
            new object[] { TargetVectors.Average(new[] { "first", "second" }) },
            new object[] { TargetVectors.ManualWeights(("first", 1.2), ("second", 0.7)) },
            new object[] { TargetVectors.RelativeScore(("first", 1.2), ("second", 0.7)) },
        };

    [Theory]
    [MemberData(nameof(MultiTargetVectors))]
    public async Task Test_NamedVector_MultiTarget(string[] targetVector)
    {
        RequireVersion("1.26.0");

        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(name: "first"),
                Configure.Vectors.SelfProvided(name: "second"),
            }
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 1f, 0f, 0f } },
                { "second", new[] { 0f, 1f, 0f } },
            }
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new[] { 0f, 1f, 0f } },
                { "second", new[] { 1f, 0f, 0f } },
            }
        );

        var objs = await collection.Query.NearVector(
            new[] { 1f, 0f, 0f },
            targetVector: targetVector
        );
        var ids = objs.Select(o => o.ID!.Value).OrderBy(x => x).ToList();
        var expected = new[] { uuid1, uuid2 }.OrderBy(x => x).ToList();
        Assert.Equal(expected, ids);
    }
}
