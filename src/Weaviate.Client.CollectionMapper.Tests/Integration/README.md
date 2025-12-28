# CollectionMapper Integration Tests

This directory contains integration tests that run against a real Weaviate instance using Docker.

## Prerequisites

- Docker Desktop installed and running
- .NET 8.0 or .NET 9.0 SDK
- At least 2GB of free RAM for Docker containers

## Quick Start

### 1. Start Weaviate

```bash
cd src/Weaviate.Client.CollectionMapper.Tests
docker-compose -f docker-compose.integration.yml up -d
```

This will start:
- Weaviate database on `http://localhost:8080`
- Text2Vec Transformers model on `http://localhost:8000`

### 2. Wait for Weaviate to be Ready

Check the health endpoint:
```bash
curl http://localhost:8080/v1/.well-known/ready
```

Or watch the logs:
```bash
docker-compose -f docker-compose.integration.yml logs -f weaviate
```

Wait for: `"msg":"started data-driven schema manager"`

### 3. Run Integration Tests

```bash
# Run only integration tests
dotnet test --filter "Category=Integration"

# Run all tests (unit + integration)
dotnet test
```

### 4. Stop Weaviate

```bash
docker-compose -f docker-compose.integration.yml down
```

To also remove volumes (clean slate):
```bash
docker-compose -f docker-compose.integration.yml down -v
```

---

## Test Structure

### IntegrationTestBase.cs

Base class for all integration tests:
- Connects to local Weaviate
- Verifies connection on startup
- Cleans up test collections after each test
- Uses unique collection name prefixes to avoid conflicts

### BasicIntegrationTests.cs

End-to-end tests covering:

| Test | Description |
|------|-------------|
| CreateCollection_FromClass | Schema creation from C# attributes |
| Insert_SingleObject | Single object insertion |
| InsertMany_MultipleObjects | Batch insertion |
| Query_WithFilter | LINQ-style filtering |
| Query_NearText | Semantic search with vectors |
| Query_NearTextWithFilter | Combined semantic + filter |
| Update_ModifyObject | Object updates |
| Delete_RemoveObject | Object deletion |
| Sort_OrderByProperty | Sorting results |
| Hybrid_CombinesKeywordAndVector | Hybrid search (BM25 + vector) |

---

## Test Patterns

### Basic CRUD

```csharp
[Fact]
public async Task MyTest()
{
    // Arrange
    await Client.Collections.CreateFromClass<MyEntity>();
    var collection = Client.Collections.Get(GetTestCollectionName(nameof(MyEntity)));

    // Act
    await collection.Data.Insert(new MyEntity { ... });

    // Assert
    var result = await collection.Query.FetchObjects(limit: 1);
    Assert.Single(result.Objects);
}
```

### Semantic Search

```csharp
[Fact]
public async Task VectorSearch()
{
    // Arrange
    await Client.Collections.CreateFromClass<Article>();
    var collection = Client.Collections.Get(GetTestCollectionName(nameof(Article)));
    await collection.Data.InsertMany(articles);

    // Wait for vectors to be generated
    await Task.Delay(2000);

    // Act
    var result = await collection.Query<Article>()
        .NearText("search query")
        .Limit(5)
        .ExecuteAsync();

    // Assert
    Assert.NotEmpty(result.Objects);
}
```

### Filtered Queries

```csharp
[Fact]
public async Task FilteredQuery()
{
    // Act
    var result = await collection.Query<Article>()
        .Where(a => a.WordCount > 100)
        .Where(a => a.IsPublished == true)
        .Limit(10)
        .ExecuteAsync();

    // Assert
    Assert.All(result.Objects, obj =>
    {
        Assert.True(obj.Properties.WordCount > 100);
        Assert.True(obj.Properties.IsPublished);
    });
}
```

---

## Troubleshooting

### "Weaviate is not ready" Error

**Cause**: Docker containers haven't finished starting

**Solution**:
```bash
# Check if containers are running
docker-compose -f docker-compose.integration.yml ps

# Check Weaviate logs
docker-compose -f docker-compose.integration.yml logs weaviate

# Wait for ready message
curl http://localhost:8080/v1/.well-known/ready
```

### "Connection refused" Error

**Cause**: Weaviate isn't running or port is blocked

**Solution**:
```bash
# Ensure Docker is running
docker ps

# Restart containers
docker-compose -f docker-compose.integration.yml restart

# Check port availability
lsof -i :8080
```

### Vector Search Returns Wrong Results

**Cause**: Vectors haven't been generated yet

**Solution**:
Add a delay after inserting data:
```csharp
await collection.Data.InsertMany(articles);
await Task.Delay(2000); // Wait for vectorization
```

### Tests Fail with "Collection already exists"

**Cause**: Previous test didn't clean up

**Solution**:
Each test uses unique collection names via `GetTestCollectionName()`. If issues persist:
```bash
# Remove all data
docker-compose -f docker-compose.integration.yml down -v
docker-compose -f docker-compose.integration.yml up -d
```

### Out of Memory Errors

**Cause**: Not enough RAM allocated to Docker

**Solution**:
1. Open Docker Desktop
2. Go to Settings → Resources
3. Increase Memory to at least 4GB
4. Apply & Restart

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  integration-tests:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'

    - name: Start Weaviate
      run: |
        cd src/Weaviate.Client.CollectionMapper.Tests
        docker-compose -f docker-compose.integration.yml up -d

    - name: Wait for Weaviate
      run: |
        timeout 60 bash -c 'until curl -f http://localhost:8080/v1/.well-known/ready; do sleep 2; done'

    - name: Run Integration Tests
      run: dotnet test --filter "Category=Integration"

    - name: Stop Weaviate
      if: always()
      run: |
        cd src/Weaviate.Client.CollectionMapper.Tests
        docker-compose -f docker-compose.integration.yml down
```

---

## Performance Considerations

### Test Speed

- **Unit Tests**: ~40ms total
- **Integration Tests**: ~30-60 seconds total (includes Docker startup)

### Parallelization

Integration tests are isolated and can run in parallel:
```bash
dotnet test --filter "Category=Integration" --parallel
```

### Resource Usage

| Component | CPU | RAM | Disk |
|-----------|-----|-----|------|
| Weaviate | ~10% | ~500MB | ~100MB |
| Transformers | ~5% | ~1GB | ~200MB |

---

## Writing New Integration Tests

### 1. Create Test Class

```csharp
[Trait("Category", "Integration")]
public class MyIntegrationTests : IntegrationTestBase
{
    // Your tests here
}
```

### 2. Define Test Model

```csharp
[WeaviateCollection]
public class MyEntity
{
    [Property(DataType.Text)]
    public string Name { get; set; } = "";

    [Vector<Vectorizer.Text2VecTransformers>()]
    public float[]? Embedding { get; set; }
}
```

### 3. Write Test

```csharp
[Fact]
public async Task MyTest_Scenario_ExpectedBehavior()
{
    // Arrange
    await Client.Collections.CreateFromClass<MyEntity>();
    var collection = Client.Collections.Get(GetTestCollectionName(nameof(MyEntity)));

    // Act
    // ... perform actions

    // Assert
    // ... verify results
}
```

### 4. Run Test

```bash
dotnet test --filter "MyTest_Scenario_ExpectedBehavior"
```

---

## Best Practices

✅ **Do:**
- Use unique collection names via `GetTestCollectionName()`
- Clean up test data in `DisposeAsync()` (automatic)
- Wait for vectorization after inserts (`await Task.Delay(2000)`)
- Test one scenario per test method
- Use descriptive test names: `Method_Scenario_ExpectedBehavior`

❌ **Don't:**
- Hard-code collection names (causes conflicts)
- Forget to wait for vectors before searching
- Test multiple scenarios in one test (makes debugging harder)
- Assume test execution order (tests should be independent)
- Leave Docker running when not testing (wastes resources)

---

## Version Compatibility

These tests are compatible with:

| Component | Version |
|-----------|---------|
| Weaviate | 1.27.8+ |
| .NET | 8.0, 9.0 |
| Docker | 20.10+ |

---

## Additional Resources

- [Weaviate Documentation](https://weaviate.io/developers/weaviate)
- [CollectionMapper Guide](../../../docs/collection_mapper_getting_started.md)
- [xUnit Documentation](https://xunit.net/)
- [Docker Compose Reference](https://docs.docker.com/compose/)
