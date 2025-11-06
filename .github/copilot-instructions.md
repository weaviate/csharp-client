# Weaviate C# Client - AI Coding Agent Instructions

## Project Overview

Official C# client for Weaviate vector database. Provides idiomatic .NET SDK with dual-protocol architecture (REST + gRPC) for managing collections, vectors, and queries. Beta release under active development.

**Multi-Targeting**: The library targets .NET 8.0 and .NET 9.0, allowing it to be used by applications running on either runtime while using C# 13 language features (via `LangVersion=latest`).

## Architecture

### Dual Protocol Design
- **REST API**: Collection management, CRUD operations, metadata (auto-generated via NSwag from OpenAPI spec at `src/Weaviate.Client/Rest/Dto/Models.g.cs`)
- **gRPC API**: High-performance queries, vector search, aggregations (generated from `.proto` files in `src/Weaviate.Client/gRPC/proto/`)
- Client automatically routes operations to optimal protocol based on operation type

### Core Client Hierarchy
```
WeaviateClient (entry point)
  ├── RestClient (internal, auto-generated)
  ├── GrpcClient (internal, protobuf-based)
  ├── Collections (CollectionsClient)
  │     └── Use<T>() → CollectionClient<T>
  │           ├── Data (DataClient<T>) - CRUD via REST
  │           ├── Query (QueryClient<T>) - Queries via gRPC
  │           ├── Aggregate (AggregateClient) - Stats via gRPC
  │           └── Tenants (TenantsClient) - Multi-tenancy
  ├── Backup (BackupClient)
  ├── Alias (AliasClient)
  ├── Users (UsersClient) - Database user management (create/delete/activate/deactivate, API key rotation)
  ├── Roles (RolesClient) - RBAC role lifecycle, permissions, assignments
  ├── Groups (GroupsClient) - List groups & inspect role assignments
  └── Nodes (NodesClient)
```

### Partial Classes Pattern
Core types split across files for separation of concerns:
- `WeaviateClient`: `WeaviateClient.cs` + `BackupClient.cs` (adds `Backup` property)
- `CollectionClient`: `CollectionClient.cs` + `TenantsClient.cs` + `AggregateClient.cs` + `CollectionAliasClient.cs` + `GenerateClient.cs`
- `WeaviateRestClient`: `Rest/Client.cs` + `Rest/Collection.cs` + `Rest/Object.cs` + `Rest/Backup.cs` + etc.

## Code Generation

### REST DTOs (NSwag)
```bash
# Regenerate REST DTOs from OpenAPI spec
./tools/gen_rest_dto.sh
# Runs: cd src/Weaviate.Client && dotnet nswag run nswag.json
# Output: src/Weaviate.Client/Rest/Dto/Models.g.cs
```
Config: `src/Weaviate.Client/nswag.json` with custom templates in `Rest/Schema/Templates/`
- **Never manually edit** `Models.g.cs` - regenerate instead
- DTOs use System.Text.Json with camelCase naming

### gRPC Protos
```bash
# Sync proto files from weaviate/weaviate repo
./tools/proto_sync.sh [branch-or-tag]  # defaults to 'main'
# Downloads protos to: src/Weaviate.Client/gRPC/proto/v1/
```
Protos auto-compile via `<Protobuf>` entries in `Weaviate.Client.csproj` with `Access="Internal"`

## Development Workflows

### Building
```bash
dotnet build Weaviate.sln
# Or use VS Code task: "dotnet: build"
```

### Running Tests
Integration tests require running Weaviate instance:
```bash
# Start test environment (Docker Compose)
./ci/start_weaviate.sh 1.27.0  # specify Weaviate version
# Run tests
dotnet test src/Weaviate.Client.Tests/Weaviate.Client.Tests.csproj
# Cleanup
./ci/stop_weaviate.sh
```

**Test Structure**:
- Tests use **TUnit** framework (Microsoft Testing Platform)
- `src/Weaviate.Client.Tests/Integration/_Integration.cs` - Base class with `CollectionFactory<T>()` helpers
- Tests inherit from `IntegrationTests`, auto-cleanup collections via `_deleteCollectionsAfterTest`
- Use `RequireVersion("1.25.0")` to skip tests for incompatible Weaviate versions
- Convention: Generate unique names via `MakeUniqueCollectionName<T>(suffix)`

### Authentication Setup
Tests read `src/Weaviate.Client.Tests/development.env` for credentials (gitignored):
```env
WEAVIATE_OPENAI_API_KEY=sk-...  # Optional for vectorizer tests
```

## Code Conventions

### File-Scoped Namespaces

Please ensure that all code adheres to file-scoped namespaces style. This allows for a more concise declaration of namespaces, improving code readability and maintainability.

### Enum String Conversion

- Use the extension methods `ToEnumMemberString()` and `FromEnumMemberString<T>()` (defined in `Extensions.cs`) to convert enums to and from their wire-format string values.
  - `ToEnumMemberString()` returns the string specified by the `[EnumMember(Value = ...)]` attribute, or the enum name if not present.
  - `FromEnumMemberString<T>()` parses a string to the enum value using the `[EnumMember]` attribute.
- Prefer these utilities over manual switch/case or pattern matching for enum <-> string conversions, especially for REST/gRPC wire formats.
- Example:
  ```csharp
  // Enum definition
  public enum BackupStorage {
    [EnumMember(Value = "filesystem")] Filesystem,
    [EnumMember(Value = "s3")] S3,
    // ...
  }

  // Convert to string for API
  var backendString = BackupStorage.S3.ToEnumMemberString(); // "s3"

  // Parse from string
  var backend = "filesystem".FromEnumMemberString<BackupStorage>(); // BackupStorage.Filesystem
  ```
### Models and DTOs
- **User-facing Models**: `src/Weaviate.Client/Models/` - C# records with strong typing
- **REST DTOs**: `src/Weaviate.Client/Rest/Dto/` - Auto-generated, internal mapping only
- Use `ToDto()` extension methods to convert Models → DTOs (in model files)
- Use `ToModel()` extension methods to convert DTOs → Models (in model files)

### Type Safety
- Generic collections: `CollectionClient<TData>` where `TData` is user's data class
- Non-generic: `CollectionClient` (base class, `dynamic` data)
- Properties auto-inferred from C# classes via `Property.FromClass<T>()`

### Builder Pattern
```csharp
// Typical client initialization
var client = WeaviateClientBuilder
    .Local(hostname: "localhost", restPort: 8080, grpcPort: 50051)
    .Build();

// Cloud shorthand
var client = WeaviateClientBuilder.Cloud("cluster-url.weaviate.cloud", apiKey: "secret");
```

### Async Patterns
- All I/O operations are async/await
- Use `IAsyncEnumerable<T>` for iterators (e.g., `CollectionClient.Iterator()`)
- Tests use TUnit

### Consistency
- Fluent API: `collection.WithTenant("tenant1").WithConsistencyLevel(ConsistencyLevels.One)`
- Method chaining returns new instances (immutable pattern)
- Enums: Use C# enums with `EnumMember` attributes for wire format

## Common Pitfalls

1. **Don't modify generated files**: `Models.g.cs` or `*.cs` in gRPC `proto/` output
2. **Protocol mismatch**: Data operations (Insert/Update/Delete) use REST; queries use gRPC
3. **Partial class coordination**: When adding methods to `WeaviateClient` or `CollectionClient`, consider if they belong in a separate partial file
4. **DTO conversion**: Always use `ToDto()`/`ToModel()` extensions - don't manually map
5. **Test isolation**: Call `CollectionFactory<T>()` in tests - handles cleanup automatically
6. **Version checks**: Prefer `RequireVersion("<min>")` (and optional maximum) at the start of a test method to gate features. It handles skip messaging automatically. Only fall back to `ServerVersionIsInRange()` for complex branching logic inside a test.
7. **RBAC permission actions**: Use enum member strings (e.g. `read_roles`, `create_users`) when constructing permissions; rely on `ToEnumMemberString()`/`FromEnumMemberString()` helpers.
8. **RBAC tests**: Unit tests should stub the `/v1/meta` endpoint to avoid constructor failures.

## Key Files Reference

- `src/Weaviate.Client/WeaviateClient.cs` - Main client, auth, connection
- `src/Weaviate.Client/CollectionClient.cs` - Collection operations, generic base
- `src/Weaviate.Client/Models/Collection.cs` - Collection configuration model
- `src/Weaviate.Client/Models/Filter.cs` - Type-safe filter DSL
- `src/Weaviate.Client/gRPC/Client.cs` - gRPC client initialization
- `src/Weaviate.Client.Tests/Integration/_Integration.cs` - Test base class
- `src/Weaviate.Client/Rest/Authz.cs` - REST RBAC (roles, assignments, groups)
- `src/Weaviate.Client/Rest/Users.cs` - REST Users endpoints
- `src/Weaviate.Client/UsersClient.cs` / `RolesClient.cs` / `GroupsClient.cs` - High-level RBAC clients
- `src/Weaviate.Client/Models/Rbac.cs` - RBAC user/role/permission model records

## Mock Testing Infrastructure

Unit tests that exercise REST client behavior without a live Weaviate server should rely on the in‑repo mock utilities rather than rolling custom stubs.

### Components
- `MockHttpMessageHandler` (`src/Weaviate.Client.Tests/Unit/Mocks/MockHttpMessageHandler.cs`)
  - FIFO queue of `MockHttpResponse` objects via `AddResponse()` / `AddJsonResponse<T>()`
  - Optional dynamic handling via `SetHandler(...)` (sync or async) for request‑dependent responses
  - Automatically records non-`/v1/meta` requests for assertions (`Requests`, `LastRequest`)
  - `Reset()` clears queued responses, recorded requests, and custom handler
- `MockWeaviateClient` (`MockHelpers.cs` – static class named `MockWeaviateClient`) creates a fully initialized `WeaviateClient` using `WeaviateClientBuilder.Local(httpMessageHandler: handler)` and injects a meta endpoint responder so client construction succeeds.
- `MockResponses` provides common canned responses (`MetaInfo()`, `CollectionCreated()`, `Error()`, etc.).
- `HttpRequestAssertions` adds fluent assertion helpers for request validation (method/path/header/body JSON).

### Typical Usage Pattern
```csharp
var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
handler
    .AddJsonResponse(new { /* first REST call mock */ }, expectedEndpoint: "/v1/users/db")
    .AddJsonResponse(new { /* second REST call mock */ }, expectedEndpoint: "/v1/authz/roles");

// Execute code under test
var users = await client.Users.ListAsync();

// Assert on captured request
handler.LastRequest!
    .ShouldHaveMethod(HttpMethod.Get)
    .ShouldHavePath("/v1/users/db");
```

The `expectedEndpoint` parameter is optional but recommended for clarity and to catch routing bugs. If provided, the handler validates that the incoming request path contains the expected endpoint string before returning the response.

### Dynamic Responses
If test logic needs to branch on request URL or method:
```csharp
handler.SetHandler(req =>
{
    if (req.RequestUri!.PathAndQuery.Contains("/v1/users/db") && req.Method == HttpMethod.Post)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ \"apiKey\": \"abc123\" }", Encoding.UTF8, "application/json")
        };
    }
    return null!; // fall back to queued responses for other requests
});
```
Returning `null!` defers to the queued responses; throw an exception for unexpected requests to hard‑fail.

### RBAC Test Guidance
- Always let `MockWeaviateClient.CreateWithMockHandler()` supply meta info; do NOT queue `/v1/meta` responses manually.
- Use the proper permission action enum strings (e.g. `read_roles`, `update_roles`, `create_users`) via `ToEnumMemberString()` when constructing mock payloads.
- For negative cases (permission check failures) return `MockResponses.Error(HttpStatusCode.Forbidden, "...", code: "forbidden")`.
- Keep each test self‑contained: call `handler.Reset()` before re‑using the same handler instance or create a fresh `(client, handler)` tuple per test.

### Common Pitfalls
1. Forgetting to queue enough responses: if the queue empties, the handler throws `InvalidOperationException`.
2. Serializing objects with wrong casing: `AddJsonResponse<T>()` uses camelCase; ensure assertions expect camelCase field names.
3. Accidentally asserting on `/v1/meta`: those requests are intentionally not added to `Requests`.
4. Mixing custom handler and queue unintentionally: if `SetHandler` returns a non-null `HttpResponseMessage`, the queued responses are bypassed for that request.
5. Endpoint mismatch: if `expectedEndpoint` is provided but the actual request doesn't match, the handler throws `InvalidOperationException` with a clear error message.

### When to Prefer Integration Tests
Use Docker‑backed integration tests (see scripts in `ci/`) when validating end‑to‑end behavior (auth flows, actual permission enforcement). Reserve the mock handler for:
- DTO ↔ Model mapping
- Endpoint path/query construction
- Error handling and status code branching

## External Dependencies

- **Weaviate Server**: Requires matching server version for tests (managed via Docker Compose)
- **OpenAPI Spec**: REST DTOs track `weaviate/weaviate` OpenAPI changes
- **Protobuf Definitions**: gRPC client tracks `weaviate/weaviate` proto definitions

## Package Publishing

Version managed by MinVer based on Git tags. Debug builds skip versioning (`MinVerSkip=true`).

## Formatting

All code formatting is strictly enforced using CSharpier (https://csharpier.com/). Any code contributions, examples, or generated code must adhere to CSharpier's opinionated style. Pre-commit hooks automatically run CSharpier to reformat code to prevent committing code that doesn't conform to the style.

## Git Pre-Commit Hooks

The repository has pre-commit hooks that automatically:
1. **Run CSharpier** - Reformats all code to match the opinionated style before commit
2. **Run `dotnet build`** - Validates that all code compiles successfully before allowing the commit

These hooks ensure code quality and consistency without manual intervention. If a build fails, the commit will be blocked until compilation errors are resolved.

### Note on File-Scoped Namespaces

Please ensure that all code adheres to file-scoped namespaces style. This allows for a more concise declaration of namespaces, improving code readability and maintainability.
