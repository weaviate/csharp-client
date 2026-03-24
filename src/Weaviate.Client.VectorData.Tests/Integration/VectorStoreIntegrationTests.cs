using Microsoft.Extensions.VectorData;

namespace Weaviate.Client.VectorData.Tests.Integration;

/// <summary>
/// Tests <see cref="WeaviateVectorStore"/> top-level operations against a real Weaviate instance.
/// </summary>
public class VectorStoreIntegrationTests : VectorDataIntegrationTests
{
    private class SimpleRecord
    {
        [VectorStoreKey]
        public Guid Id { get; set; }

        [VectorStoreData]
        public string Title { get; set; } = "";

        [VectorStoreVector(4)]
        public float[] Vector { get; set; } = [];
    }

    [Fact]
    public async Task GetCollection_ReturnsUsableCollection()
    {
        var store = new WeaviateVectorStore(_weaviate);
        var name = $"VdStoreTest{Guid.NewGuid():N}";
        TrackCollection(name);

        var collection = store.GetCollection<Guid, SimpleRecord>(name);

        Assert.NotNull(collection);
        Assert.Equal(name, collection.Name);

        await collection.EnsureCollectionExistsAsync(CT);
        Assert.True(await collection.CollectionExistsAsync(CT));
    }

    [Fact]
    public async Task ListCollectionNamesAsync_IncludesCreatedCollection()
    {
        var store = new WeaviateVectorStore(_weaviate);
        var name = $"VdStoreListTest{Guid.NewGuid():N}";
        TrackCollection(name);

        var collection = store.GetCollection<Guid, SimpleRecord>(name);
        await collection.EnsureCollectionExistsAsync(CT);

        var names = new List<string>();
        await foreach (var n in store.ListCollectionNamesAsync(CT))
        {
            names.Add(n);
        }

        Assert.Contains(name, names);
    }

    [Fact]
    public async Task CollectionExistsAsync_OnStore_WorksCorrectly()
    {
        var store = new WeaviateVectorStore(_weaviate);
        var name = $"VdStoreExistsTest{Guid.NewGuid():N}";
        TrackCollection(name);

        Assert.False(await store.CollectionExistsAsync(name, CT));

        var collection = store.GetCollection<Guid, SimpleRecord>(name);
        await collection.EnsureCollectionExistsAsync(CT);

        Assert.True(await store.CollectionExistsAsync(name, CT));
    }

    [Fact]
    public async Task EnsureCollectionDeletedAsync_OnStore_DeletesCollection()
    {
        var store = new WeaviateVectorStore(_weaviate);
        var name = $"VdStoreDeleteTest{Guid.NewGuid():N}";
        TrackCollection(name);

        var collection = store.GetCollection<Guid, SimpleRecord>(name);
        await collection.EnsureCollectionExistsAsync(CT);
        Assert.True(await store.CollectionExistsAsync(name, CT));

        await store.EnsureCollectionDeletedAsync(name, CT);
        Assert.False(await store.CollectionExistsAsync(name, CT));
    }

    [Fact]
    public void GetCollection_WithStringKey_Works()
    {
        var store = new WeaviateVectorStore(_weaviate);
        var collection = store.GetCollection<string, SimpleRecord>("TestStringKey");

        Assert.NotNull(collection);
        Assert.Equal("TestStringKey", collection.Name);
    }

    [Fact]
    public void GetCollection_WithUnsupportedKey_Throws()
    {
        var store = new WeaviateVectorStore(_weaviate);

        Assert.Throws<NotSupportedException>(() =>
            store.GetCollection<int, SimpleRecord>("TestIntKey")
        );
    }
}
