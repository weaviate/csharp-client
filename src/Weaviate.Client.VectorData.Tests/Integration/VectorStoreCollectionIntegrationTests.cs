using Microsoft.Extensions.VectorData;

namespace Weaviate.Client.VectorData.Tests.Integration;

/// <summary>
/// Tests the full VectorData contract against a real Weaviate instance:
/// collection lifecycle, CRUD, vector search.
/// </summary>
public class VectorStoreCollectionIntegrationTests : VectorDataIntegrationTests
{
    private class Hotel
    {
        [VectorStoreKey]
        public Guid Id { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string Name { get; set; } = "";

        [VectorStoreData]
        public int Stars { get; set; }

        [VectorStoreVector(4, DistanceFunction = DistanceFunction.CosineDistance)]
        public float[] Embedding { get; set; } = [];
    }

    private WeaviateVectorStoreCollection<Guid, Hotel> CreateCollection(string name)
    {
        TrackCollection(name);
        return new WeaviateVectorStoreCollection<Guid, Hotel>(_weaviate, name);
    }

    [Fact]
    public async Task CollectionLifecycle_CreateExistsDelete()
    {
        var name = $"VdTestLifecycle{Guid.NewGuid():N}";
        var collection = CreateCollection(name);

        Assert.False(await collection.CollectionExistsAsync(CT));

        await collection.EnsureCollectionExistsAsync(CT);
        Assert.True(await collection.CollectionExistsAsync(CT));

        await collection.EnsureCollectionDeletedAsync(CT);
        Assert.False(await collection.CollectionExistsAsync(CT));
    }

    [Fact]
    public async Task Upsert_And_Get_SingleRecord()
    {
        var name = $"VdTestUpsertGet{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        var id = Guid.NewGuid();
        var hotel = new Hotel
        {
            Id = id,
            Name = "Grand Hotel",
            Stars = 5,
            Embedding = [0.1f, 0.2f, 0.3f, 0.4f],
        };

        await collection.UpsertAsync(hotel, CT);

        var retrieved = await collection.GetAsync(id, cancellationToken: CT);

        Assert.NotNull(retrieved);
        Assert.Equal(id, retrieved!.Id);
        Assert.Equal("Grand Hotel", retrieved.Name);
        Assert.Equal(5, retrieved.Stars);
    }

    [Fact]
    public async Task Upsert_Overwrites_ExistingRecord()
    {
        var name = $"VdTestUpsertOverwrite{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        var id = Guid.NewGuid();
        await collection.UpsertAsync(
            new Hotel
            {
                Id = id,
                Name = "Old Name",
                Stars = 3,
                Embedding = [0.1f, 0.2f, 0.3f, 0.4f],
            },
            CT
        );

        await collection.UpsertAsync(
            new Hotel
            {
                Id = id,
                Name = "New Name",
                Stars = 5,
                Embedding = [0.5f, 0.6f, 0.7f, 0.8f],
            },
            CT
        );

        var retrieved = await collection.GetAsync(id, cancellationToken: CT);

        Assert.NotNull(retrieved);
        Assert.Equal("New Name", retrieved!.Name);
        Assert.Equal(5, retrieved.Stars);
    }

    [Fact]
    public async Task Delete_RemovesRecord()
    {
        var name = $"VdTestDelete{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        var id = Guid.NewGuid();
        await collection.UpsertAsync(
            new Hotel
            {
                Id = id,
                Name = "To Delete",
                Stars = 1,
                Embedding = [0.1f, 0.2f, 0.3f, 0.4f],
            },
            CT
        );

        await collection.DeleteAsync(id, CT);

        var retrieved = await collection.GetAsync(id, cancellationToken: CT);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteBatch_RemovesMultipleRecords()
    {
        var name = $"VdTestDeleteBatch{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        foreach (var id in ids)
        {
            await collection.UpsertAsync(
                new Hotel
                {
                    Id = id,
                    Name = $"Hotel {id}",
                    Stars = 3,
                    Embedding = [0.1f, 0.2f, 0.3f, 0.4f],
                },
                CT
            );
        }

        await collection.DeleteAsync(ids, CT);

        foreach (var id in ids)
        {
            var retrieved = await collection.GetAsync(id, cancellationToken: CT);
            Assert.Null(retrieved);
        }
    }

    [Fact]
    public async Task GetBatch_ReturnsMultipleRecords()
    {
        var name = $"VdTestGetBatch{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        foreach (var id in ids)
        {
            await collection.UpsertAsync(
                new Hotel
                {
                    Id = id,
                    Name = $"Hotel {id}",
                    Stars = 4,
                    Embedding = [0.1f, 0.2f, 0.3f, 0.4f],
                },
                CT
            );
        }

        var results = new List<Hotel>();
        await foreach (var record in collection.GetAsync(ids, cancellationToken: CT))
        {
            results.Add(record);
        }

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task SearchAsync_ReturnsNearestResults()
    {
        var name = $"VdTestSearch{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        await collection.UpsertAsync(
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Close Hotel",
                Stars = 5,
                Embedding = [1.0f, 0.0f, 0.0f, 0.0f],
            },
            CT
        );
        await collection.UpsertAsync(
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Far Hotel",
                Stars = 2,
                Embedding = [0.0f, 0.0f, 0.0f, 1.0f],
            },
            CT
        );

        var results = new List<VectorSearchResult<Hotel>>();
        await foreach (
            var result in collection.SearchAsync(
                new float[] { 1.0f, 0.0f, 0.0f, 0.0f },
                top: 2,
                cancellationToken: CT
            )
        )
        {
            results.Add(result);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Close Hotel", results[0].Record.Name);
    }

    [Fact]
    public async Task Get_NonExistentRecord_ReturnsNull()
    {
        var name = $"VdTestGetNull{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        var result = await collection.GetAsync(Guid.NewGuid(), cancellationToken: CT);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithExpressionFilter_ReturnsMatchingRecords()
    {
        var name = $"VdTestExprFilter{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        await collection.UpsertAsync(
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Luxury Hotel",
                Stars = 5,
                Embedding = [0.1f, 0.2f, 0.3f, 0.4f],
            },
            CT
        );
        await collection.UpsertAsync(
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Budget Hotel",
                Stars = 2,
                Embedding = [0.5f, 0.6f, 0.7f, 0.8f],
            },
            CT
        );

        var results = new List<Hotel>();
        await foreach (
            var record in collection.GetAsync(x => x.Stars == 5, top: 10, cancellationToken: CT)
        )
        {
            results.Add(record);
        }

        Assert.Single(results);
        Assert.Equal("Luxury Hotel", results[0].Name);
    }

    [Fact]
    public async Task HybridSearchAsync_ReturnsResults()
    {
        var name = $"VdTestHybrid{Guid.NewGuid():N}";
        var collection = CreateCollection(name);
        await collection.EnsureCollectionExistsAsync(CT);

        await collection.UpsertAsync(
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Beach Resort",
                Stars = 4,
                Embedding = [1.0f, 0.0f, 0.0f, 0.0f],
            },
            CT
        );
        await collection.UpsertAsync(
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Mountain Lodge",
                Stars = 3,
                Embedding = [0.0f, 1.0f, 0.0f, 0.0f],
            },
            CT
        );

        var results = new List<VectorSearchResult<Hotel>>();
        await foreach (
            var result in collection.HybridSearchAsync(
                new float[] { 1.0f, 0.0f, 0.0f, 0.0f },
                ["Beach"],
                top: 2,
                cancellationToken: CT
            )
        )
        {
            results.Add(result);
        }

        Assert.NotEmpty(results);
        Assert.Equal("Beach Resort", results[0].Record.Name);
    }
}
