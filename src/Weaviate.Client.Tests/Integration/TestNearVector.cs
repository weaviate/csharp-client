using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class SearchTests
{
    [Fact]
    public async Task NearVectorSearch()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestData>(
            "NearVectorSearch3vecs",
            "Test collection description"
        );

        // Act
        await collectionClient.Data.Insert(
            new TestData { Name = "TestObject1" },
            vectors: new[] { 0.1f, 0.2f, 0.3f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await collectionClient.Data.Insert(
            new TestData { Name = "TestObject2" },
            vectors: new[] { 0.3f, 0.4f, 0.5f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await collectionClient.Data.Insert(
            properties: new TestData { Name = "TestObject3" },
            vectors: new[] { 0.5f, 0.6f, 0.7f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = await collectionClient.Query.FetchObjects(
            includeVectors: true,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var retrieved = await collectionClient.Query.NearVector(
            new[] { 0.1f, 0.2f, 0.3f },
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(retrieved);
        Assert.NotEmpty(retrieved.Objects);

        Assert.Equal("TestObject1", retrieved.Objects.First().As<TestData>()?.Name);
    }

    /*
    def test_same_target_vector_multiple_input_combinations(
    collection_factory: CollectionFactory,
    near_vector: Dict[str, Union[Sequence[float], Sequence[Sequence[float]], _ListOfVectorsQuery]],
    target_vector: List[str],
) -> None:
    dummy = collection_factory("dummy")
    if dummy._connection._weaviate_version.is_lower_than(1, 27, 0):
        pytest.skip("Multi vector per target is not supported in versions lower than 1.27.0")

    collection = collection_factory(
        properties=[],
        vector_config=[
            wvc.config.Configure.Vectors.self_provided(name="first"),
            wvc.config.Configure.Vectors.self_provided(name="second"),
        ],
    )

    uuid1 = collection.data.insert({}, vector={"first": [1, 0], "second": [0, 1, 0]})
    uuid2 = collection.data.insert({}, vector={"first": [0, 1], "second": [1, 0, 0]})

    objs = collection.query.near_vector(
        near_vector, target_vector=target_vector, return_metadata=wvc.query.MetadataQuery.full()
    ).objects
    assert sorted([obj.uuid for obj in objs]) == sorted([uuid2, uuid1])
    */
}
