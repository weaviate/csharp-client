# Project Memory: weaviate/csharp-client

## Workflow Preferences
- **Always pause for code review before committing.** Stage changes, show a diff summary, and wait for user approval before running `git commit`.
- PRs target the current branch (`v1.0.1`) as base, not `main`.
- All work in a session creates PRs that merge to the current branch.
- **Do NOT commit intermediate docs or plans** (design docs, plan files). Only commit code changes.

## Project Structure
- `src/Weaviate.Client/` — main library
- `src/Weaviate.Client.Tests/` — tests (Unit/ and Integration/)
- DTOs are auto-generated via NSwag from `Rest/Schema/openapi.json` → `Rest/Dto/Models.g.cs`
- Public API surface tracked in `PublicAPI.Unshipped.txt` (Roslyn analyzer enforces this)
- Pre-commit hooks: dotnet build + CSharpier formatting

## Key Patterns
- REST layer: `Rest/Endpoints.cs` (paths) + `Rest/Collection.cs` / other partials (HTTP calls)
- Public API: `CollectionConfigClient.cs`, `CollectionConfigFactory`, etc.
- Enum → API string: use `ToEnumMemberString()` from `Extensions.cs` (supports both `[EnumMember]` and `[JsonStringEnumMemberName]`)
- Generated `Dto.*` enums (e.g. `Dto.IndexName`) are the canonical source for API string values
- Internal DTOs use `internal`, public models in `Models/`

## TDD Practice
- Write failing test first, confirm compile error, then implement
- Unit tests use `MockWeaviateClient.CreateWithMockHandler()` + `MockHttpMessageHandler`
- Path assertions: `ShouldHaveMethod(HttpMethod.Delete).ShouldHavePath("/v1/schema/...")`

## Tooling
- **Use csharp-lsp actively** for navigating types, finding usages, and validating changes — prefer LSP-driven analysis over re-reading files manually
