````instructions
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
- Use `ToDto()` extension methods to convert Models → DTOs
- Use `ToModel()` extension methods to convert DTOs → Models
- **Important**: All `ToModel()` and `ToDto()` extension methods should be added to `src/Weaviate.Client/Rest/Dto/Extensions.cs` (not in model files)
- **ToModel() pattern**: Extend the partial `Rest.Dto` classes (e.g., `partial class Role { public Models.RoleInfo ToModel() => ... }`)
- **ToDto() pattern**: Use extension methods on Model types (e.g., `public static Dto.Role ToDto(this Models.RoleInfo model) => ...`)

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

### REST API Behavior and OpenAPI Spec Correlation

**Critical**: Always verify REST endpoint behavior against the OpenAPI specification before implementing REST client methods. The OpenAPI spec is the source of truth for:
- Expected HTTP status codes (success and error cases)
- Response schemas (including empty response bodies)
- Request/response content types
- Error response structures

**Common Patterns to Verify**:

1. **Empty Response Bodies**: Some mutation endpoints (POST/PUT/PATCH) return success status codes (200/201/204) with empty response bodies
   - Example: `POST /v1/authz/roles` returns `201 Created` with no body
   - Pattern: Re-fetch the created/updated resource with a subsequent GET request
   - Check OpenAPI spec for `responses.<status>.content` - if missing, expect empty body

2. **Idempotent Operations**: Some DELETE endpoints return success regardless of resource existence
   - Example: `DELETE /v1/authz/roles/{id}` returns `204 No Content` whether the role exists or not
   - Pattern: Don't throw exceptions on 404 for these endpoints
   - Check OpenAPI spec `responses` - if 404 is not listed as an error response, the operation is idempotent

3. **Lenient Validation**: Some endpoints return success (200) with specific body values instead of 404 for non-existent resources
   - Example: `POST /v1/authz/roles/{id}/has-permission` returns `200 OK` with `false` for non-existent roles
   - Pattern: Don't expect 404; handle the semantic meaning in the response body
   - Check OpenAPI spec - if only 200 is listed in successful responses, expect lenient behavior

4. **Error Status Codes**: Verify which error codes are documented (400, 404, 409, 422, 500, etc.)
   - Only throw specific exceptions (e.g., `WeaviateNotFoundException`, `WeaviateConflictException`) for documented error responses
   - Check OpenAPI spec `responses` section for all documented error statuses

**Workflow**:
1. Before implementing a REST method in `Rest/*.cs`, check the corresponding endpoint in the OpenAPI spec
2. Document expected status codes in code comments
3. Write tests that verify actual server behavior matches OpenAPI spec expectations
4. For unexpected behavior, file an issue rather than implementing workarounds

**Reference**: OpenAPI spec is regenerated via `./tools/gen_rest_dto.sh` from the `weaviate/weaviate` repository

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

## RBAC Permissions Modeling Pattern

The C# SDK models RBAC permissions using a type-safe, resource-centric pattern:

- Each permission scope (Alias, Data, Backups, etc.) is represented by a subclass of `PermissionsScope`.
- Each scope class holds a resource record (from `PermissionResource.cs`) and boolean properties for each supported action.
- The `GetEnumerator()` method yields `PermissionInfo` objects using the `RbacPermissionAction` enum for type safety.
- Each scope class provides a static `Parse(IEnumerable<PermissionInfo> infos)` method that groups and aggregates permissions by resource, using the enum for switch logic.
- The root `Permissions.Parse(IEnumerable<PermissionInfo> infos)` method delegates to all scope types and aggregates the results.
- All permission construction and parsing should use the `RbacPermissionAction` enum, not raw strings.

**Example:**

```csharp
public class Permissions
{
  public class Alias : PermissionsScope
  {
    public AliasesResource Resource { get; }
    public bool Create { get; set; }
    public bool Read { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }

    public Alias(AliasesResource resource) { Resource = resource; }

    public override IEnumerator<PermissionInfo> GetEnumerator()
    {
      var permResource = new PermissionResource(Aliases: Resource);
      if (Create) yield return new PermissionInfo(RbacPermissionAction.CreateAliases, permResource);
      if (Read) yield return new PermissionInfo(RbacPermissionAction.ReadAliases, permResource);
      if (Update) yield return new PermissionInfo(RbacPermissionAction.UpdateAliases, permResource);
      if (Delete) yield return new PermissionInfo(RbacPermissionAction.DeleteAliases, permResource);
    }

    public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
    {
      return infos
        .Where(i => i.Resources?.Aliases != null)
        .GroupBy(i => i.Resources!.Aliases!)
        .Select(group =>
        {
          var aliasPerm = new Alias(group.Key);
          foreach (var info in group)
          {
            switch (info.Action)
            {
              case RbacPermissionAction.CreateAliases: aliasPerm.Create = true; break;
              case RbacPermissionAction.ReadAliases: aliasPerm.Read = true; break;
              case RbacPermissionAction.UpdateAliases: aliasPerm.Update = true; break;
              case RbacPermissionAction.DeleteAliases: aliasPerm.Delete = true; break;
            }
          }
          return (PermissionsScope)aliasPerm;
        })
        .ToList();
    }
  }
  // Repeat for Data, Backups, etc.
  public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
  {
    var scopes = new List<PermissionsScope>();
    scopes.AddRange(Alias.Parse(infos));
    // scopes.AddRange(Data.Parse(infos));
    // scopes.AddRange(Backups.Parse(infos));
    // ...other scopes...
    return scopes;
  }
}
```

#### Guidelines

- Always use the enum for permission actions.
- Always group by the correct resource record for each scope.
- Extend the pattern for all supported resource types and actions.
- Add thorough unit tests for GroupBy and aggregation logic.
````
