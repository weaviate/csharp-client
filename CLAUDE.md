# Weaviate C# Client — Claude Development Guide

## Project Architecture

### Layer Overview

```text
QueryClient.*.cs          ← Public API, one partial file per search type (BM25, NearText, etc.)
    ↓ delegates to
gRPC/Search.cs            ← Executes gRPC requests (SearchBM25, SearchNearVector, etc.)
    ↓ builds via
gRPC/Search.Builders.cs   ← BaseSearchRequest() constructs V1.SearchRequest proto
    ↓ transforms via
gRPC/Result.cs            ← BuildResult*() maps V1.SearchReply → C# model types
    ↓ surfaces as
Models/Results.cs         ← WeaviateResult<TObject>, GroupByResult<TObject, TGroup>
```

### Result Type Hierarchy

- **Generic bases:** `WeaviateResult<TObject>` and `GroupByResult<TObject, TGroup>` hold all shared properties.
- **Concrete types:** `WeaviateResult`, `GroupByResult` extend the generics with typed objects.
- **Feature variants:** `GenerativeWeaviateResult`, `GenerativeGroupByResult` add feature-specific properties.
- **Typed wrappers:** `WeaviateResult<WeaviateObject<T>>` via `ToTyped<T>()` in `TypedResultConverter.cs`.
- **Pattern:** Add result-level features (like `Generative`, `QueryProfile`) to the generic base so all variants inherit.

### Adding a New Result-Level Feature

1. Add C# model to `Models/`
2. Add `Feature? Feature { get; init; }` to `WeaviateResult<TObject>` (and/or `GroupByResult<TObject, TGroup>`)
3. Map proto → model in `gRPC/Result.cs` `BuildResult*` methods
4. Propagate in `Models/Typed/TypedResultConverter.cs` `ToTyped` methods

### Adding a New Request Parameter

1. Add flag to `MetadataOptions` enum / computed property on `MetadataQuery` in `Models/MetadataQuery.cs`
2. Wire into `BaseSearchRequest()` → `V1.MetadataRequest` in `gRPC/Search.Builders.cs`
3. If it requires a new proto field: add to `gRPC/proto/v1/search_get.proto` (Grpc.Tools auto-generates C# on build)

### Proto Compilation

`Grpc.Tools` compiles `*.proto` → C# on every `dotnet build`. No manual step needed. Generated classes land in namespace `Weaviate.Client.Grpc.Protobuf.V1`.

Nested proto message `Foo.Bar` becomes `Foo.Types.Bar` in C#.

For `optional` proto3 message fields, the generated C# has a `HasXxx` bool property for presence detection.

### Implicit Conversions

`gRPC/Extensions.cs` adds `implicit operator` from `SearchReply` to all four result types. The operators delegate to `WeaviateGrpcClient.BuildResult*()`. Unit tests can use proto JSON deserialization + the implicit cast as a clean roundtrip pattern.

## Testing Strategy

### Unit Tests (`--filter "FullyQualifiedName~Unit"`)

- Proto JSON → model roundtrip: use `JsonParser` from `Google.Protobuf` to parse JSON into a proto message, then use the implicit conversion operator to get the C# result.
- Model construction tests: directly instantiate model types to verify properties are accessible.
- Flag tests: verify `MetadataOptions` flags map correctly through `MetadataQuery` computed properties.

```csharp
var jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
var reply = jsonParser.Parse<Grpc.Protobuf.V1.SearchReply>(json);
WeaviateResult result = reply;  // implicit conversion
```

### Integration Tests

- Use `CollectionFactory(name, description, properties, vectorConfig?)` helper from `IntegrationTests` base.
- Join `[Collection("SearchTests")]` + `public partial class SearchTests : IntegrationTests` — no new fixture needed.
- Use `TestContext.Current.CancellationToken` for all async calls.
- No `RequiresWeaviateVersion` guard needed for gRPC fields: older servers silently ignore unknown proto fields.

#### Running Integration Tests Locally

A live Weaviate server must be running. Use the CI scripts to start/stop it:

```bash
./ci/start_weaviate.sh 1.36.9   # start (replace version as needed)
dotnet test src/Weaviate.Client.Tests --filter "FullyQualifiedName~Integration"
./ci/stop_weaviate.sh 1.36.9    # stop when done
```

## REST DTO Generation

`src/Weaviate.Client/Rest/Dto/Models.g.cs` is **auto-generated** — never edit it manually. To update it:

1. Run `./tools/openapi_sync.sh` followed by the desired branch or tag to target from the github.com/weaviate/weaviate repo
2. Run `./tools/gen_rest_dto.sh` to regenerate `Models.g.cs` via NSwag

To customize the generated output (e.g., access modifiers), edit the Liquid templates in `src/Weaviate.Client/Rest/Schema/Templates/`. These override NSwag's built-in templates. `File.liquid` overrides the file-level template (controls `FileResponse` visibility, etc.).

## Public API Tracking

After adding public types/members, run:

```bash
dotnet build src/Weaviate.Client/Weaviate.Client.csproj 2>&1 | grep "RS0016"
```

Copy the RS0016 error messages (minus the file/error prefix) into `PublicAPI.Unshipped.txt`. C# records auto-generate many symbols (`<Clone>$`, `EqualityContract`, equality operators, `PrintMembers`, etc.) — all must be listed.

## Version Guard Pattern

For methods that require a minimum Weaviate server version:

```csharp
[RequiresWeaviateVersion(1, 36, 0)]
public async Task<T> MethodName(...)
{
    await _client.EnsureVersion<ContainingClass>();
    // ...
}
```

The WEAVIATE008 Roslyn analyzer enforces that the `EnsureVersion` call is present. **Only needed for standalone methods**, not for feature flags added as optional parameters to existing query methods.

## Worktree Workflow

Feature branches use isolated git worktrees under `.worktrees/`. The worktree's working directory is separate but shares the same `.git` repo. Run all commands from the worktree directory, not the main repo root.
