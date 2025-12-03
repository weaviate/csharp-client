using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class CollectionsTests
{
    [Fact]
    public async Task Test_Legacy_Collection_Create_From_Json()
    {
        var className = MakeUniqueCollectionName("legacy");

        await _weaviate.Collections.Delete(className, TestContext.Current.CancellationToken);

        string json =
            $@"{{
    ""class"": ""{className}"",
    ""invertedIndexConfig"": {{
        ""bm25"": {{
            ""b"": 0.75,
            ""k1"": 1.2
        }},
        ""cleanupIntervalSeconds"": 60,
        ""stopwords"": {{
            ""additions"": null,
            ""preset"": ""en"",
            ""removals"": null
        }},
        ""usingBlockMaxWAND"": true
    }},
    ""multiTenancyConfig"": {{
        ""autoTenantActivation"": false,
        ""autoTenantCreation"": false,
        ""enabled"": false
    }},
    ""properties"": [
        {{
            ""dataType"": [
                ""text""
            ],
            ""indexFilterable"": true,
            ""indexRangeFilters"": false,
            ""indexSearchable"": true,
            ""name"": ""name"",
            ""tokenization"": ""word""
        }}
    ],
    ""replicationConfig"": {{
        ""asyncEnabled"": false,
        ""deletionStrategy"": ""NoAutomatedResolution"",
        ""factor"": 1
    }},
    ""shardingConfig"": {{
        ""virtualPerPhysical"": 128,
        ""desiredCount"": 1,
        ""actualCount"": 1,
        ""desiredVirtualCount"": 128,
        ""actualVirtualCount"": 128,
        ""key"": ""_id"",
        ""strategy"": ""hash"",
        ""function"": ""murmur3""
    }},
    ""vectorIndexConfig"": {{
        ""skip"": false,
        ""cleanupIntervalSeconds"": 300,
        ""maxConnections"": 32,
        ""efConstruction"": 128,
        ""ef"": -1,
        ""dynamicEfMin"": 100,
        ""dynamicEfMax"": 500,
        ""dynamicEfFactor"": 8,
        ""vectorCacheMaxObjects"": 1000000000000,
        ""flatSearchCutoff"": 40000,
        ""distance"": ""cosine"",
        ""pq"": {{
            ""enabled"": false,
            ""bitCompression"": false,
            ""segments"": 0,
            ""centroids"": 256,
            ""trainingLimit"": 100000,
            ""encoder"": {{
                ""type"": ""kmeans"",
                ""distribution"": ""log-normal""
            }}
        }},
        ""bq"": {{
            ""enabled"": false
        }},
        ""sq"": {{
            ""enabled"": false,
            ""trainingLimit"": 100000,
            ""rescoreLimit"": 20
        }},
        ""rq"": {{
            ""enabled"": false,
            ""bits"": 8,
            ""rescoreLimit"": 20
        }},
        ""filterStrategy"": ""sweeping"",
        ""multivector"": {{
            ""enabled"": false,
            ""muvera"": {{
                ""enabled"": false,
                ""ksim"": 4,
                ""dprojections"": 16,
                ""repetitions"": 10
            }},
            ""aggregation"": ""maxSim""
        }},
        ""skipDefaultQuantization"": false,
        ""trackDefaultQuantization"": false
    }},
    ""vectorIndexType"": ""hnsw"",
    ""vectorizer"": ""none""
}}";

        var collectionClient = await _weaviate.Collections.Create(
            json,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(collectionClient);
        Assert.Equal(className, collectionClient.Name);

        // Generate random vectors for testing
        var random = new Random(42); // Seed for reproducibility
        var vector1 = Enumerable.Range(0, 4).Select(_ => (float)random.NextDouble()).ToArray();
        var vector2 = Enumerable.Range(0, 4).Select(_ => (float)random.NextDouble()).ToArray();
        var vector3 = Enumerable.Range(0, 4).Select(_ => (float)random.NextDouble()).ToArray();
        var vector4 = Enumerable.Range(0, 4).Select(_ => (float)random.NextDouble()).ToArray();
        var vector5 = Enumerable.Range(0, 4).Select(_ => (float)random.NextDouble()).ToArray();

        // Insert a few items with normal Insert, manually passing "default" vector
        var id1 = await collectionClient.Data.Insert(
            new { name = "Item1" },
            vectors: vector1,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var id2 = await collectionClient.Data.Insert(
            new { name = "Item2" },
            vectors: vector2,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var id3 = await collectionClient.Data.Insert(
            new { name = "Item3" },
            vectors: vector3,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Insert a few more with InsertMany, manually passing "default" vector
        var insertManyResults = await collectionClient.Data.InsertMany(
            [
                BatchInsertRequest.Create(new { name = "Item4" }, vectors: vector4),
                BatchInsertRequest.Create(new { name = "Item5" }, vectors: vector5),
            ],
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(insertManyResults);
        Assert.False(insertManyResults.HasErrors);
        Assert.Equal(2, insertManyResults.Count);

        // Fetch objects with includeVector: true
        var fetchedObjects = await collectionClient.Query.FetchObjects(
            includeVectors: true,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(fetchedObjects);
        Assert.Equal(5, fetchedObjects.Objects.Count);

        // Assert that all objects have the "default" vector and correct data
        var obj1 = fetchedObjects.Objects.First(o => o.ID == id1);
        Assert.NotNull(obj1);
        Assert.Equal("Item1", obj1.Properties["name"]);
        Assert.True(obj1.Vectors.ContainsKey("default"));
        Assert.Equal(4, obj1.Vectors["default"].Dimensions);
        float[] obj1Vector = obj1.Vectors["default"];
        Assert.Equal(vector1, obj1Vector);

        var obj2 = fetchedObjects.Objects.First(o => o.ID == id2);
        Assert.NotNull(obj2);
        Assert.Equal("Item2", obj2.Properties["name"]);
        Assert.True(obj2.Vectors.ContainsKey("default"));
        float[] obj2Vector = obj2.Vectors["default"];
        Assert.Equal(vector2, obj2Vector);

        var obj3 = fetchedObjects.Objects.First(o => o.ID == id3);
        Assert.NotNull(obj3);
        Assert.Equal("Item3", obj3.Properties["name"]);
        Assert.True(obj3.Vectors.ContainsKey("default"));
        float[] obj3Vector = obj3.Vectors["default"];
        Assert.Equal(vector3, obj3Vector);

        var obj4 = fetchedObjects.Objects.First(o => o.Properties["name"]?.ToString() == "Item4");
        Assert.NotNull(obj4);
        Assert.True(obj4.Vectors.ContainsKey("default"));
        float[] obj4Vector = obj4.Vectors["default"];
        Assert.Equal(vector4, obj4Vector);

        var obj5 = fetchedObjects.Objects.First(o => o.Properties["name"]?.ToString() == "Item5");
        Assert.NotNull(obj5);
        Assert.True(obj5.Vectors.ContainsKey("default"));
        float[] obj5Vector = obj5.Vectors["default"];
        Assert.Equal(vector5, obj5Vector);
    }
}
