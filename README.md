# Weaviate C# Client

[![NuGet](https://badgen.net/nuget/v/Weaviate.Client?icon=nuget)](https://www.nuget.org/packages/Weaviate.Client)

Welcome to the official C# client for Weaviate, the open-source vector database. This library provides a convenient and idiomatic way for .NET developers to interact with a Weaviate instance.

---

## 🚀 Installation

You can install the Weaviate C# client via the NuGet Package Manager or the `dotnet` CLI.

```bash
dotnet add package Weaviate.Client --version 1.0.0
```

Alternatively, you can add the `PackageReference` to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Weaviate.Client" Version="1.0.0" />
</ItemGroup>
```

---

## ✨ Getting Started

The best way to get started is by following our quickstart guide. It will walk you through setting up the client, connecting to Weaviate, creating a collection, and performing your first vector search.

- **[➡️ Quickstart Guide](https://docs.weaviate.io/weaviate/quickstart)**


### Quickstart example

```csharp
string weaviateUrl = Environment.GetEnvironmentVariable("WEAVIATE_URL");
string weaviateApiKey = Environment.GetEnvironmentVariable("WEAVIATE_API_KEY");

// 1. Connect to Weaviate Cloud
var client = await Connect.Cloud(weaviateUrl, weaviateApiKey);

// 2. Prepare data
var dataObjects = new List<object>
{
    new { title = "The Matrix", description = "A computer hacker learns about the true nature of reality and his role in the war against its controllers.", genre = "Science Fiction", },
    new { title = "Spirited Away", description = "A young girl becomes trapped in a mysterious world of spirits and must find a way to save her parents and return home.", genre = "Animation", },
    new { title = "The Lord of the Rings: The Fellowship of the Ring", description = "A meek Hobbit and his companions set out on a perilous journey to destroy a powerful ring and save Middle-earth.", genre = "Fantasy", },
};

var CollectionName = "Movie";

// 3. Create the collection
var movies = await client.Collections.Create(
    new CollectionCreateParams
    {
        Name = CollectionName,
        VectorConfig = Configure.Vector("default", v => v.Text2VecWeaviate()),
    }
);

// 4. Import the data
var result = await movies.Data.InsertMany(dataObjects);

// 5. Run the query
var response = await movies.Query.NearText("sci-fi", limit: 2);

// 6. Inspect the results
foreach (var obj in response.Objects)
{
    Console.WriteLine(JsonSerializer.Serialize(obj.Properties));
}
```

---

## 📚 Documentation

For more detailed information on specific features, please refer to the official documentation and the how-to guides.

- **[Client library overview](https://docs.weaviate.io/weaviate/client-libraries/csharp)**
- **[How-to: Configure the client](https://docs.weaviate.io/weaviate/configuration)**
- **[How-to: Manage collections](https://docs.weaviate.io/weaviate/manage-collections)**
- **[How-to: Manage data objects](https://docs.weaviate.io/weaviate/manage-objects)**
- **[How-to: Query & search data](https://docs.weaviate.io/weaviate/search)**


### Additional Guides

- **[Batch API Usage](docs/BATCH_API_USAGE.md)**: Server-side streaming batch operations
    **Note:** Server-side batch requires Weaviate v1.36.0 or newer (or v1.35 with the experimental flag enabled). Earlier versions will not work.
- **[Error Handling](docs/ERRORS.md)**: Exception types and error handling patterns
- **[RBAC API Usage](docs/RBAC_API_USAGE.md)**: Managing users, roles, permissions, and groups
- **[Backup API Usage](docs/BACKUP_API_USAGE.md)**: Creating and restoring backups
- **[Nodes API Usage](docs/NODES_API_USAGE.md)**: Querying cluster node information
- **[Aggregate Result Accessors](docs/AGGREGATE_RESULT_ACCESSORS.md)**: Type-safe access to aggregation results

---

## 🤝 Community

Connect with the Weaviate community and the team through our online channels. We would love to hear your feedback!

- **[GitHub Issues](https://github.com/weaviate/csharp-client/issues)**: For specific feature requests, bug reports, or issues with the client.
- **[Weaviate Forum](https://forum.weaviate.io/)**: For questions and discussions.
- **[Weaviate Slack](https://weaviate.io/slack)**: For live chats with the community and team.

## 🧪 Integration Testing

To run the integration test suite locally you need a Weaviate server at version >= **1.31.0**.

Start a local instance with the helper script (defaults to the minimum supported version):

```bash
./ci/start_weaviate.sh            # uses 1.31.0
# or explicitly
./ci/start_weaviate.sh 1.32.7     # any version >= 1.31.0
```

Run the tests:

```bash
dotnet test src/Weaviate.Client.Tests/Weaviate.Client.Tests.csproj
```

### Filtering Slow Tests

Some tests (backups and replication) are marked as slow and can take several minutes. You can control which tests run using the `--filter` option:

```bash
# Run all tests including slow ones (default)
dotnet test src/Weaviate.Client.Tests/Weaviate.Client.Tests.csproj

# Exclude slow tests (recommended for quick feedback during development)
dotnet test src/Weaviate.Client.Tests/Weaviate.Client.Tests.csproj --filter "Category!=Slow"

# Run ONLY slow tests
dotnet test src/Weaviate.Client.Tests/Weaviate.Client.Tests.csproj --filter "Category=Slow"
```

Stop the environment when finished:

```bash
./ci/stop_weaviate.sh
```

If the server version is below 1.31.0 the integration tests will be skipped automatically.
