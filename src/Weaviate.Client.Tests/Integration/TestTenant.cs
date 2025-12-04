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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            new[]
            {
                new Tenant { Name = "tenant1" },
                new Tenant { Name = "tenant2" },
                new Tenant { Name = "tenant3" },
            },
            TestContext.Current.CancellationToken
        );

        var tenant2Collection = collectionClient.WithTenant("tenant2");
        var items = Enumerable.Range(0, (int)(howMany * 2)).Select(x => new { }).ToArray();
        var result = await tenant2Collection.Data.InsertMany(
            BatchInsertRequest.Create(items),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(0, result.Count(r => r.Error != null));

        // Act
        var tenant2Count = await tenant2Collection.Count(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var tenant3Count = await collectionClient
            .WithTenant("tenant3")
            .Count(cancellationToken: TestContext.Current.CancellationToken);

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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        Tenant tenantObj = new() { Name = tenant };
        await collectionClient.Tenants.Create(
            new[] { tenantObj },
            TestContext.Current.CancellationToken
        );

        var tenant1Collection = collectionClient.WithTenant(tenantObj.Name);
        var uuid = await tenant1Collection.Data.Insert(
            new { },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var fetched = await tenant1Collection.Query.FetchObjectByID(
            uuid,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(fetched);

        var ex = await Record.ExceptionAsync(() =>
            tenant1Collection.Data.DeleteByID(
                uuid,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        Assert.Null(ex);

        var fetchedAfterDelete = await tenant1Collection.Query.FetchObjectByID(
            uuid,
            cancellationToken: TestContext.Current.CancellationToken
        );
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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            new[] { "tenant1", "tenant2" },
            TestContext.Current.CancellationToken
        );

        var tenant1Collection = collectionClient.WithTenant("tenant1");
        var tenant2Collection = collectionClient.WithTenant("tenant2");

        var result = (
            await tenant1Collection.Data.InsertMany(
                [
                    BatchInsertRequest.Create(
                        new { Name = "some name" },
                        null,
                        new float[] { 1, 2, 3 }
                    ),
                    BatchInsertRequest.Create(new { Name = "some other name" }, _reusableUuids[0]),
                ],
                TestContext.Current.CancellationToken
            )
        ).ToList();

        Assert.Equal(0, result.Count(r => r.Error != null));

        var obj1 = await tenant1Collection.Query.FetchObjectByID(
            result[0].ID!.Value,
            cancellationToken: TestContext.Current.CancellationToken
        );
        var obj2 = await tenant1Collection.Query.FetchObjectByID(
            result[1].ID!.Value,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(obj1);
        Assert.NotNull(obj2);
        Assert.Equal("some name", obj1.Properties["name"]);
        Assert.Equal("some other name", obj2.Properties["name"]);

        Assert.Null(
            await tenant2Collection.Query.FetchObjectByID(
                result[0].ID!.Value,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        Assert.Null(
            await tenant2Collection.Query.FetchObjectByID(
                result[1].ID!.Value,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task ReplaceWithTenant()
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("Name"), Property.Text("Name2")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            new[]
            {
                new Tenant { Name = "tenant1" },
                new Tenant { Name = "tenant2" },
            },
            TestContext.Current.CancellationToken
        );

        var tenant1Collection = collectionClient.WithTenant("tenant1");
        var tenant2Collection = collectionClient.WithTenant("tenant2");

        var uuid = await tenant1Collection.Data.Insert(
            new { Name = "some name", Name2 = "some name2" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await tenant1Collection.Data.Replace(
            uuid,
            new { Name = "other name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var obj = await tenant1Collection.Query.FetchObjectByID(
            uuid,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj);
        Assert.Equal("other name", obj.Properties["name"]);
        Assert.DoesNotContain("name2", obj.Properties.Keys);

        Assert.Null(
            await tenant2Collection.Query.FetchObjectByID(
                uuid,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task TenantsUpdate()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("Name")],
            vectorConfig: Configure.Vectors.SelfProvided().New()
        );

        var uuid = await collectionClient.Data.Insert(
            new { Name = "some name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await collectionClient.Data.Replace(
            uuid,
            new { Name = "other name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var obj = await collectionClient.Query.FetchObjectByID(
            uuid,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj);
        Assert.Equal("other name", obj.Properties["name"]);
    }

    [Fact]
    public async Task UpdateWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("Name"), Property.Text("Name2")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            new[]
            {
                new Tenant { Name = "tenant1" },
                new Tenant { Name = "tenant2" },
            },
            TestContext.Current.CancellationToken
        );

        var tenant1Collection = collectionClient.WithTenant("tenant1");
        var tenant2Collection = collectionClient.WithTenant("tenant2");

        var uuid = await tenant1Collection.Data.Insert(
            new { Name = "some name", Name2 = "some name2" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await tenant1Collection.Data.Update(
            uuid,
            new { Name = "other name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var obj = await tenant1Collection.Query.FetchObjectByID(
            uuid,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj);
        Assert.Equal("other name", obj.Properties["name"]);
        Assert.Equal("some name2", obj.Properties["name2"]); // was not replaced

        Assert.Null(
            await tenant2Collection.Query.FetchObjectByID(
                uuid,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task TenantsCrudAndGetByNames()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            new[] { "tenant1", "tenant2" },
            TestContext.Current.CancellationToken
        );

        var tenants = (
            await collectionClient.Tenants.List(
                Array.Empty<string>(),
                TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Equal(2, tenants.Count);
        Assert.Contains(tenants, t => t.Name == "tenant1");
        Assert.Contains(tenants, t => t.Name == "tenant2");

        var tenant2List = (
            await collectionClient.Tenants.List(
                new[] { "tenant2" },
                TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Single(tenant2List);
        Assert.Equal("tenant2", tenant2List[0].Name);

        await collectionClient.Tenants.Delete(
            new[] { "tenant1", "tenant2" },
            TestContext.Current.CancellationToken
        );

        tenants = (
            await collectionClient.Tenants.List(
                Array.Empty<string>(),
                TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Empty(tenants);
    }

    [Fact]
    public async Task SearchWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            new[] { "Tenant1", "Tenant2" },
            TestContext.Current.CancellationToken
        );

        var tenant1Collection = collectionClient.WithTenant("Tenant1");
        var tenant2Collection = collectionClient.WithTenant("Tenant2");

        var uuid1 = await tenant1Collection.Data.Insert(
            new { name = "some name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objects1 = (
            await tenant1Collection.Query.BM25(
                query: "some",
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Single(objects1);
        Assert.Equal(uuid1, objects1[0].ID);

        var objects2 = (
            await tenant2Collection.Query.BM25(
                query: "some",
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Empty(objects2);
    }

    [Fact]
    public async Task FetchObjectByIdWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            ["Tenant1", "Tenant2"],
            TestContext.Current.CancellationToken
        );

        var tenant1Collection = collectionClient.WithTenant("Tenant1");
        var tenant2Collection = collectionClient.WithTenant("Tenant2");

        var uuid1 = await tenant1Collection.Data.Insert(
            new { name = "some name" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var obj1 = await tenant1Collection.Query.FetchObjectByID(
            uuid1,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj1);
        Assert.Equal("some name", obj1.Properties["name"]);

        var obj2 = await tenant2Collection.Query.FetchObjectByID(
            uuid1,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Null(obj2);

        var uuid2 = await tenant2Collection.Data.Insert(
            new { name = "some other name" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var obj3 = await tenant2Collection.Query.FetchObjectByID(
            uuid2,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj3);
        Assert.Equal("some other name", obj3.Properties["name"]);

        var obj4 = await tenant1Collection.Query.FetchObjectByID(
            uuid2,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Null(obj4);
    }

    [Fact]
    public async Task FetchObjectsWithTenant()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            ["Tenant1", "Tenant2"],
            TestContext.Current.CancellationToken
        );

        var tenant1Collection = collectionClient.WithTenant("Tenant1");
        var tenant2Collection = collectionClient.WithTenant("Tenant2");

        await tenant1Collection.Data.Insert(
            new { name = "some name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objects1 = (
            await tenant1Collection.Query.FetchObjects(
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Single(objects1);
        Assert.Equal("some name", objects1[0].Properties["name"]);

        var objects2 = (
            await tenant2Collection.Query.FetchObjects(
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Empty(objects2);

        await tenant2Collection.Data.Insert(
            new { name = "some other name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objects3 = (
            await tenant2Collection.Query.FetchObjects(
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            ["Tenant1", "Tenant2"],
            TestContext.Current.CancellationToken
        );

        var tenant1Collection = collectionClient.WithTenant("Tenant1");
        var tenant2Collection = collectionClient.WithTenant("Tenant2");

        var uuid1 = await tenant1Collection.Data.Insert(
            new { },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await tenant2Collection.Data.Insert(
            new { },
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(
            await tenant1Collection.Query.FetchObjectByID(
                uuid1,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        Assert.Null(
            await tenant2Collection.Query.FetchObjectByID(
                uuid1,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        Assert.NotNull(
            await tenant2Collection.Query.FetchObjectByID(
                uuid2,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        Assert.Null(
            await tenant1Collection.Query.FetchObjectByID(
                uuid2,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task UpdateTenantActivityStatus()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        // Create with HOT (deprecated)
#pragma warning disable CS0612 // Type or member is obsolete
        await collectionClient.Tenants.Create(
            [new Tenant { Name = "1", Status = TenantActivityStatus.Hot }],
            TestContext.Current.CancellationToken
        );
#pragma warning restore CS0612 // Type or member is obsolete

        var tenants = (
            await collectionClient.Tenants.List(TestContext.Current.CancellationToken)
        ).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Active, tenants["1"].Status);

        // Update to COLD (deprecated)
#pragma warning disable CS0612 // Type or member is obsolete
        await collectionClient.Tenants.Update(
            [new Tenant { Name = "1", Status = TenantActivityStatus.Cold }],
            TestContext.Current.CancellationToken
        );
#pragma warning restore CS0612 // Type or member is obsolete

        tenants = (
            await collectionClient.Tenants.List(TestContext.Current.CancellationToken)
        ).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Inactive, tenants["1"].Status);

        // Update to ACTIVE
        await collectionClient.Tenants.Update(
            [new Tenant { Name = "1", Status = TenantActivityStatus.Active }],
            TestContext.Current.CancellationToken
        );
        tenants = (
            await collectionClient.Tenants.List(TestContext.Current.CancellationToken)
        ).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Active, tenants["1"].Status);

        // Update to INACTIVE
        await collectionClient.Tenants.Update(
            [new Tenant { Name = "1", Status = TenantActivityStatus.Inactive }],
            TestContext.Current.CancellationToken
        );
        tenants = (
            await collectionClient.Tenants.List(TestContext.Current.CancellationToken)
        ).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Inactive, tenants["1"].Status);
    }

    [Fact]
    public async Task TenantWithActivity()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        // Add tenants with various activity statuses, including deprecated ones
#pragma warning disable CS0612 // Type or member is obsolete
        await collectionClient.Tenants.Create(
            [
                new Tenant { Name = "1", Status = TenantActivityStatus.Hot },
                new Tenant { Name = "2", Status = TenantActivityStatus.Cold },
                new Tenant { Name = "3", Status = TenantActivityStatus.Active },
                new Tenant { Name = "4", Status = TenantActivityStatus.Inactive },
                new Tenant { Name = "5" },
            ],
            TestContext.Current.CancellationToken
        );
#pragma warning restore CS0612 // Type or member is obsolete

        var tenants = (
            await collectionClient.Tenants.List(TestContext.Current.CancellationToken)
        ).ToDictionary(t => t.Name);

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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        var tenant = new Tenant { Name = "1" };
        await collectionClient.Tenants.Create([tenant], TestContext.Current.CancellationToken);

        Assert.True(
            await collectionClient.Tenants.Exists(
                tenant.Name,
                TestContext.Current.CancellationToken
            )
        );
        Assert.False(
            await collectionClient.Tenants.Exists("2", TestContext.Current.CancellationToken)
        );
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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create([tenantCase], TestContext.Current.CancellationToken);

        var tenant = await collectionClient.Tenants.Get(
            tenantCase,
            TestContext.Current.CancellationToken
        );

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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        var collection = await collectionClient.Config.Get(TestContext.Current.CancellationToken);

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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
        {
            if (tenants is Tenant t)
                await collectionClient.Tenants.Create([t], TestContext.Current.CancellationToken);
            else if (tenants is List<Tenant> list)
                await collectionClient.Tenants.Create(
                    list.ToArray(),
                    TestContext.Current.CancellationToken
                );
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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
        {
            if (tenants is Tenant t)
                await collectionClient.Tenants.Update([t], TestContext.Current.CancellationToken);
            else if (tenants is List<Tenant> list)
                await collectionClient.Tenants.Update(
                    list.ToArray(),
                    TestContext.Current.CancellationToken
                );
        });
    }

    [Fact]
    public async Task TenantsCreateAndUpdate1001Tenants()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        // Create 1001 tenants
        var tenantsToCreate = Enumerable
            .Range(0, 1001)
            .Select(i => new Tenant { Name = $"tenant{i}" })
            .ToArray();

        await collectionClient.Tenants.Create(
            tenantsToCreate,
            TestContext.Current.CancellationToken
        );

        var tenants = (
            await collectionClient.Tenants.List(TestContext.Current.CancellationToken)
        ).ToList();
        Assert.Equal(1001, tenants.Count);
        Assert.All(tenants, t => Assert.Equal(TenantActivityStatus.Active, t.Status));

        // Update all tenants to INACTIVE
        var tenantsToUpdate = tenants
            .Select(t => new Tenant { Name = t.Name, Status = TenantActivityStatus.Inactive })
            .ToArray();

        foreach (var tenant in tenantsToUpdate)
            await collectionClient.Tenants.Update([tenant], TestContext.Current.CancellationToken);

        tenants = (
            await collectionClient.Tenants.List(TestContext.Current.CancellationToken)
        ).ToList();
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
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        // Create collection with auto-tenant creation enabled
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true, autoTenantCreation: true)
        );

        // Insert 101 objects for tenant "tenant"
        var tenantCollection = collectionClient.WithTenant("tenant");
        var objects = Enumerable
            .Range(0, 101)
            .Select(_ => BatchInsertRequest.Create(new { name = "some name" }));
        var result = await tenantCollection.Data.InsertMany(
            objects,
            TestContext.Current.CancellationToken
        );
        Assert.Equal(0, result.Count(r => r.Error != null));

        // Batch insert 101 objects for tenants "tenant-0" to "tenant-100"
        var batchResult = await collectionClient.Data.InsertMany(
            Enumerable
                .Range(0, 101)
                .Select(i =>
                    BatchInsertRequest.Create(new { name = "some name" }, tenant: $"tenant-{i}")
                ),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(0, batchResult.Count(r => r.Error != null));
    }

    [Fact]
    public async Task TenantsDeactivateThenActivate()
    {
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        var tenant = new Tenant { Name = "tenant1" };
        await collectionClient.Tenants.Create([tenant], TestContext.Current.CancellationToken);

        var tenant1Collection = collectionClient.WithTenant(tenant.Name);

        await tenant1Collection.Data.Insert(
            new { name = "some name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objects = (
            await tenant1Collection.Query.FetchObjects(
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Single(objects);
        Assert.Equal("some name", objects[0].Properties["name"]);

        await collectionClient.Tenants.Deactivate([tenant], TestContext.Current.CancellationToken);

        var tenants = (
            await collectionClient.Tenants.List(TestContext.Current.CancellationToken)
        ).ToDictionary(t => t.Name);
        Assert.Equal(TenantActivityStatus.Inactive, tenants["tenant1"].Status);

        await collectionClient.Tenants.Activate([tenant], TestContext.Current.CancellationToken);

        objects = (
            await tenant1Collection.Query.FetchObjects(
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Single(objects);
        Assert.Equal("some name", objects[0].Properties["name"]);
    }

    [Fact]
    public async Task WithTenantOnDifferentClients()
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            null,
            "Test collection with tenants",
            [Property.Text("name")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            multiTenancyConfig: Configure.MultiTenancy(enabled: true)
        );

        await collectionClient.Tenants.Create(
            new[]
            {
                new Tenant { Name = "TenantA" },
                new Tenant { Name = "TenantB" },
            },
            TestContext.Current.CancellationToken
        );

        // Act
        var tenantAClient = collectionClient.WithTenant("TenantA");
        await tenantAClient.Data.Insert(
            new { name = "some A name" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await tenantAClient.Data.Insert(
            new { name = "another A name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var tenantBClient = tenantAClient.WithTenant("TenantB");
        await tenantBClient.Data.Insert(
            new { name = "some B name" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objectsA = (
            await tenantAClient.Query.FetchObjects(
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Equal(2, objectsA.Count);

        var objectsB = (
            await tenantBClient.Query.FetchObjects(
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Single(objectsB);
        Assert.Equal("some B name", objectsB[0].Properties["name"]);

        // Assert
        Assert.NotNull(tenantAClient);
        Assert.NotNull(tenantBClient);
        Assert.NotSame(tenantAClient, tenantBClient);

        Assert.Equal("TenantA", tenantAClient.Tenant);
        Assert.Equal("TenantB", tenantBClient.Tenant);
    }
}
