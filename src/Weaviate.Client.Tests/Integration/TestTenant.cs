using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

[Collection("TenantTests")]
public partial class TenantTests : IntegrationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10000)]
    [InlineData(20000)]
    [InlineData(20001)]
    [InlineData(100000)]
    public async Task CollectionLengthTenant(ulong howMany)
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            "",
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add(
            new[]
            {
                new Tenant { Name = "tenant1" },
                new Tenant { Name = "tenant2" },
                new Tenant { Name = "tenant3" },
            }
        );

        var tenant2Collection = collectionClient.WithTenant("tenant2");
        var items = Enumerable.Range(0, (int)(howMany * 2)).Select(x => new { }).ToArray();
        var result = await tenant2Collection.Data.InsertMany(items);

        Assert.Equal(0, result.Count(r => r.Error != null));

        // Act
        var tenant2Count = await tenant2Collection.Count();
        var tenant3Count = await collectionClient.WithTenant("tenant3").Count();

        // Assert
        Assert.Equal(howMany * 2, tenant2Count);
        Assert.Equal(0UL, tenant3Count);
    }

    [Theory]
    [InlineData("tenant1")]
    public async Task DeleteByIdTenant(string tenant)
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            "",
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        Tenant tenantObj = new() { Name = tenant };
        await collectionClient.Tenants.Add(tenantObj);

        var tenant1Collection = collectionClient.WithTenant(tenantObj.Name);
        var uuid = await tenant1Collection.Data.Insert(new { });

        var fetched = await tenant1Collection.Query.FetchObjectByID(uuid);
        Assert.NotNull(fetched);

        var ex = await Record.ExceptionAsync(() => tenant1Collection.Data.Delete(uuid));
        Assert.Null(ex);

        var fetchedAfterDelete = await tenant1Collection.Query.FetchObjectByID(uuid);
        Assert.Null(fetchedAfterDelete);
    }

    [Fact]
    public async Task InsertManyWithTenant()
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("Name")],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add("tenant1", "tenant2");

        var tenant1Collection = collectionClient.WithTenant("tenant1");
        var tenant2Collection = collectionClient.WithTenant("tenant2");

        var result = (
            await tenant1Collection.Data.InsertMany(add =>
            {
                add(new { Name = "some name" }, null, new float[] { 1, 2, 3 });
                add(new { Name = "some other name" }, _reusableUuids[0]);
            })
        ).ToList();

        Assert.Equal(0, result.Count(r => r.Error != null));

        var obj1 = await tenant1Collection.Query.FetchObjectByID(result[0].ID!.Value);
        var obj2 = await tenant1Collection.Query.FetchObjectByID(result[1].ID!.Value);

        Assert.NotNull(obj1);
        Assert.NotNull(obj2);
        Assert.Equal("some name", obj1.Properties["name"]);
        Assert.Equal("some other name", obj2.Properties["name"]);

        Assert.Null(await tenant2Collection.Query.FetchObjectByID(result[0].ID!.Value));
        Assert.Null(await tenant2Collection.Query.FetchObjectByID(result[1].ID!.Value));
    }

    [Fact]
    public async Task ReplaceWithTenant()
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("Name")],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add(
            new Tenant { Name = "tenant1" },
            new Tenant { Name = "tenant2" }
        );

        var tenant1Collection = collectionClient.WithTenant("tenant1");
        var tenant2Collection = collectionClient.WithTenant("tenant2");

        var uuid = await tenant1Collection.Data.Insert(new { Name = "some name" });

        await tenant1Collection.Data.Replace(uuid, new { Name = "other name" });

        var obj = await tenant1Collection.Query.FetchObjectByID(uuid);
        Assert.NotNull(obj);
        Assert.Equal("other name", obj.Properties["name"]);

        Assert.Null(await tenant2Collection.Query.FetchObjectByID(uuid));
    }

    [Fact]
    public async Task TenantsUpdate()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("Name")],
            vectorConfig: Configure.Vectors.SelfProvided()
        );

        var uuid = await collectionClient.Data.Insert(new { Name = "some name" });

        await collectionClient.Data.Replace(uuid, new { Name = "other name" });

        var obj = await collectionClient.Query.FetchObjectByID(uuid);
        Assert.NotNull(obj);
        Assert.Equal("other name", obj.Properties["name"]);
    }

    [Fact]
    public async Task UpdateWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("Name")],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add(
            new Tenant { Name = "tenant1" },
            new Tenant { Name = "tenant2" }
        );

        var tenant1Collection = collectionClient.WithTenant("tenant1");
        var tenant2Collection = collectionClient.WithTenant("tenant2");

        var uuid = await tenant1Collection.Data.Insert(new { Name = "some name" });

        await tenant1Collection.Data.Replace(uuid, new { Name = "other name" });

        var obj = await tenant1Collection.Query.FetchObjectByID(uuid);
        Assert.NotNull(obj);
        Assert.Equal("other name", obj.Properties["name"]);

        Assert.Null(await tenant2Collection.Query.FetchObjectByID(uuid));
    }

    [Fact]
    public async Task TenantsCrudAndGetByNames()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add("tenant1", "tenant2");

        var tenants = (await collectionClient.Tenants.List()).ToList();
        Assert.Equal(2, tenants.Count);
        Assert.Contains(tenants, t => t.Name == "tenant1");
        Assert.Contains(tenants, t => t.Name == "tenant2");

        var tenant2List = (await collectionClient.Tenants.List("tenant2")).ToList();
        Assert.Single(tenant2List);
        Assert.Equal("tenant2", tenant2List[0].Name);

        await collectionClient.Tenants.Delete(new[] { "tenant1", "tenant2" });

        tenants = (await collectionClient.Tenants.List()).ToList();
        Assert.Empty(tenants);
    }

    [Fact]
    public async Task SearchWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add("Tenant1", "Tenant2");

        var tenant1Collection = collectionClient.WithTenant("Tenant1");
        var tenant2Collection = collectionClient.WithTenant("Tenant2");

        var uuid1 = await tenant1Collection.Data.Insert(new { name = "some name" });

        var objects1 = (await tenant1Collection.Query.BM25(query: "some")).ToList();
        Assert.Single(objects1);
        Assert.Equal(uuid1, objects1[0].ID);

        var objects2 = (await tenant2Collection.Query.BM25(query: "some")).ToList();
        Assert.Empty(objects2);
    }

    [Fact]
    public async Task FetchObjectByIdWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add("Tenant1", "Tenant2");

        var tenant1Collection = collectionClient.WithTenant("Tenant1");
        var tenant2Collection = collectionClient.WithTenant("Tenant2");

        var uuid1 = await tenant1Collection.Data.Insert(new { name = "some name" });
        var obj1 = await tenant1Collection.Query.FetchObjectByID(uuid1);
        Assert.NotNull(obj1);
        Assert.Equal("some name", obj1.Properties["name"]);

        var obj2 = await tenant2Collection.Query.FetchObjectByID(uuid1);
        Assert.Null(obj2);

        var uuid2 = await tenant2Collection.Data.Insert(new { name = "some other name" });
        var obj3 = await tenant2Collection.Query.FetchObjectByID(uuid2);
        Assert.NotNull(obj3);
        Assert.Equal("some other name", obj3.Properties["name"]);

        var obj4 = await tenant1Collection.Query.FetchObjectByID(uuid2);
        Assert.Null(obj4);
    }

    [Fact]
    public async Task FetchObjectsWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add("Tenant1", "Tenant2");

        var tenant1Collection = collectionClient.WithTenant("Tenant1");
        var tenant2Collection = collectionClient.WithTenant("Tenant2");

        await tenant1Collection.Data.Insert(new { name = "some name" });

        var objects1 = (await tenant1Collection.Query.FetchObjects()).ToList();
        Assert.Single(objects1);
        Assert.Equal("some name", objects1[0].Properties["name"]);

        var objects2 = (await tenant2Collection.Query.FetchObjects()).ToList();
        Assert.Empty(objects2);

        await tenant2Collection.Data.Insert(new { name = "some other name" });

        var objects3 = (await tenant2Collection.Query.FetchObjects()).ToList();
        Assert.Single(objects3);
        Assert.Equal("some other name", objects3[0].Properties["name"]);
    }

    [Fact]
    public async Task ExistWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add("Tenant1", "Tenant2");

        var tenant1Collection = collectionClient.WithTenant("Tenant1");
        var tenant2Collection = collectionClient.WithTenant("Tenant2");

        var uuid1 = await tenant1Collection.Data.Insert(new { });
        var uuid2 = await tenant2Collection.Data.Insert(new { });

        Assert.NotNull(await tenant1Collection.Query.FetchObjectByID(uuid1));
        Assert.Null(await tenant2Collection.Query.FetchObjectByID(uuid1));
        Assert.NotNull(await tenant2Collection.Query.FetchObjectByID(uuid2));
        Assert.Null(await tenant1Collection.Query.FetchObjectByID(uuid2));
    }

    [Fact]
    public async Task UpdateTenantActivityStatus()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        // Create with HOT (deprecated)
#pragma warning disable CS0612 // Type or member is obsolete
        await collectionClient.Tenants.Add(
            new Tenant { Name = "1", Status = TenantActivityStatus.Hot }
        );
#pragma warning restore CS0612 // Type or member is obsolete

        var tenants = (await collectionClient.Tenants.List()).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Active, tenants["1"].Status);

        // Update to COLD (deprecated)
#pragma warning disable CS0612 // Type or member is obsolete
        await collectionClient.Tenants.Update(
            new Tenant { Name = "1", Status = TenantActivityStatus.Cold }
        );
#pragma warning restore CS0612 // Type or member is obsolete

        tenants = (await collectionClient.Tenants.List()).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Inactive, tenants["1"].Status);

        // Update to ACTIVE
        await collectionClient.Tenants.Update(
            new Tenant { Name = "1", Status = TenantActivityStatus.Active }
        );
        tenants = (await collectionClient.Tenants.List()).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Active, tenants["1"].Status);

        // Update to INACTIVE
        await collectionClient.Tenants.Update(
            new Tenant { Name = "1", Status = TenantActivityStatus.Inactive }
        );
        tenants = (await collectionClient.Tenants.List()).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Inactive, tenants["1"].Status);
    }

    [Fact]
    public async Task TenantWithActivity()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        // Add tenants with various activity statuses, including deprecated ones
#pragma warning disable CS0612 // Type or member is obsolete
        await collectionClient.Tenants.Add(
            new Tenant { Name = "1", Status = TenantActivityStatus.Hot },
            new Tenant { Name = "2", Status = TenantActivityStatus.Cold },
            new Tenant { Name = "3", Status = TenantActivityStatus.Active },
            new Tenant { Name = "4", Status = TenantActivityStatus.Inactive },
            new Tenant { Name = "5" }
        );
#pragma warning restore CS0612 // Type or member is obsolete

        var tenants = (await collectionClient.Tenants.List()).ToDictionary(t => t.Name);

        Assert.Equal(TenantActivityStatus.Active, tenants["1"].Status); // HOT → ACTIVE
        Assert.Equal(TenantActivityStatus.Inactive, tenants["2"].Status); // COLD → INACTIVE
        Assert.Equal(TenantActivityStatus.Active, tenants["3"].Status);
        Assert.Equal(TenantActivityStatus.Inactive, tenants["4"].Status);
        Assert.Equal(TenantActivityStatus.Active, tenants["5"].Status); // Default is ACTIVE
    }

    [Fact]
    public async Task TenantExists()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        var tenant = new Tenant { Name = "1" };
        await collectionClient.Tenants.Add(tenant);

        Assert.True(await collectionClient.Tenants.Exists(tenant.Name));
        Assert.False(await collectionClient.Tenants.Exists("2"));
    }

    public static IEnumerable<object[]> TenantCases()
    {
        yield return new object[] { "tenant1" };
        yield return new object[] { "tenant2" };
        yield return new object[] { "tenant3" };
    }

    [Theory]
    [MemberData(nameof(TenantCases))]
    public async Task TenantGetByName(string tenantCase)
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Add(tenantCase);

        var tenant = await collectionClient.Tenants.Get(tenantCase);

        Assert.NotNull(tenant);
        Assert.Equal(tenantCase, tenant.Name);
    }

    [Fact]
    public async Task AutoTenantToggling()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        var collection = await collectionClient.Get();

        Assert.NotNull(collection);

        Assert.False(collection.MultiTenancyConfig?.AutoTenantCreation);

        collection = await collectionClient.Config.Update(c =>
            c.MultiTenancyConfig.AutoTenantCreation = true
        );

        Assert.True(collection.MultiTenancyConfig?.AutoTenantCreation);

        collection = await collectionClient.Config.Update(c =>
            c.MultiTenancyConfig.AutoTenantCreation = false
        );
        Assert.False(collection.MultiTenancyConfig?.AutoTenantCreation);
    }

    public static IEnumerable<object[]> FrozenTenantCases()
    {
        yield return new object[]
        {
            new Tenant { Name = "1", Status = TenantActivityStatus.Frozen },
        };
        yield return new object[]
        {
            new List<Tenant>
            {
                new Tenant { Name = "4", Status = TenantActivityStatus.Frozen },
            },
        };
    }

    [Theory]
    [MemberData(nameof(FrozenTenantCases))]
    public async Task TenantsCreateWithReadOnlyActivityStatus(object tenants)
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await Assert.ThrowsAsync<WeaviateException>(async () =>
        {
            if (tenants is Tenant t)
                await collectionClient.Tenants.Add(t);
            else if (tenants is List<Tenant> list)
                await collectionClient.Tenants.Add(list.ToArray());
        });
    }

    public static IEnumerable<object[]> ReadOnlyUpdateTenantCases()
    {
        yield return new object[]
        {
            new Tenant { Name = "1", Status = TenantActivityStatus.Offloading },
        };
        yield return new object[]
        {
            new Tenant { Name = "1", Status = TenantActivityStatus.Onloading },
        };
        yield return new object[]
        {
            new List<Tenant>
            {
                new Tenant { Name = "1", Status = TenantActivityStatus.Offloading },
                new Tenant { Name = "2", Status = TenantActivityStatus.Onloading },
            },
        };
    }

    [Theory]
    [MemberData(nameof(ReadOnlyUpdateTenantCases))]
    public async Task TenantsUpdateWithReadOnlyActivityStatus(object tenants)
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            if (tenants is Tenant t)
                await collectionClient.Tenants.Update(t);
            else if (tenants is List<Tenant> list)
                await collectionClient.Tenants.Update(list.ToArray());
        });
    }

    [Fact]
    public async Task TenantsCreateAndUpdate1001Tenants()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        // Create 1001 tenants
        var tenantsToCreate = Enumerable
            .Range(0, 1001)
            .Select(i => new Tenant { Name = $"tenant{i}" })
            .ToArray();

        await collectionClient.Tenants.Add(tenantsToCreate);

        var tenants = (await collectionClient.Tenants.List()).ToList();
        Assert.Equal(1001, tenants.Count);
        Assert.All(tenants, t => Assert.Equal(TenantActivityStatus.Active, t.Status));

        // Update all tenants to INACTIVE
        var tenantsToUpdate = tenants
            .Select(t => new Tenant { Name = t.Name, Status = TenantActivityStatus.Inactive })
            .ToArray();

        foreach (var tenant in tenantsToUpdate)
            await collectionClient.Tenants.Update(tenant);

        tenants = (await collectionClient.Tenants.List()).ToList();
        Assert.Equal(1001, tenants.Count);
        Assert.All(tenants, t => Assert.Equal(TenantActivityStatus.Inactive, t.Status));
    }

    [Fact]
    public async Task TenantsAutoTenantCreation()
    {
        // Create a dummy collection to check version support
        var dummyCollectionClient = await CollectionFactory(
            "dummy",
            "dummy",
            [],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        // Create collection with auto-tenant creation enabled
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true, autoTenantCreation: true)
        );

        // Insert 101 objects for tenant "tenant"
        var tenantCollection = collectionClient.WithTenant("tenant");
        var objects = Enumerable.Range(0, 101).Select(_ => new { name = "some name" }).ToArray();
        var result = await tenantCollection.Data.InsertMany(objects);
        Assert.Equal(0, result.Count(r => r.Error != null));

        // Batch insert 101 objects for tenants "tenant-0" to "tenant-100"
        var batchResult = await collectionClient.Data.InsertMany(add =>
        {
            for (int i = 0; i < 101; i++)
            {
                add(new { Name = "some name" }, tenant: $"tenant-{i}");
            }
        });

        Assert.Equal(0, batchResult.Count(r => r.Error != null));
    }

    [Fact]
    public async Task TenantsDeactivateThenActivate()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        var tenant = new Tenant { Name = "tenant1" };
        await collectionClient.Tenants.Add(tenant);

        var tenant1Collection = collectionClient.WithTenant(tenant.Name);

        await tenant1Collection.Data.Insert(new { name = "some name" });

        var objects = (await tenant1Collection.Query.FetchObjects()).ToList();
        Assert.Single(objects);
        Assert.Equal("some name", objects[0].Properties["name"]);

        await collectionClient.Tenants.Deactivate(tenant);

        var tenants = (await collectionClient.Tenants.List()).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Inactive, tenants["tenant1"].Status);

        await collectionClient.Tenants.Activate(tenant);

        objects = (await tenant1Collection.Query.FetchObjects()).ToList();
        Assert.Single(objects);
        Assert.Equal("some name", objects[0].Properties["name"]);
    }
}
