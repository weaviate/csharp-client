# Weaviate C# Client – Copilot Coding Agent Quick Reference

## Project Overview
- Official C# SDK for Weaviate vector DB
- REST (NSwag auto-gen) for CRUD/metadata, gRPC for queries/vectors
- Targets .NET 8.0 and .NET 9.0, and it's using .NET 9.0 SDK.

## Architecture & Conventions
- Entry: `WeaviateClient` (REST/gRPC, collections, backup, RBAC, etc.)
- Partial classes for separation; never edit auto-generated files
- File-scoped namespaces, CSharpier formatting enforced
- Use builder pattern for client setup
- All I/O is async/await

## Code Generation & DTOs
- REST DTOs: auto-gen via NSwag from OpenAPI spec
- gRPC: auto-gen from proto files
- User models: `src/Weaviate.Client/Models/`
- REST DTOs: `src/Weaviate.Client/Rest/Dto/`
- Use `ToModel()`/`ToDto()` in `Rest/Dto/Extensions.cs` for mapping

## Enum & Wire Format
- Use `ToEnumMemberString()`/`FromEnumMemberString<T>()` for conveting enums to and from strings
- Always prefer enums for permission actions and resource types

## Testing
- Unit: xUnit, mock HTTP handler for REST
- Integration: Docker Compose for Weaviate instance

## RBAC Modeling
- Permissions modeled by resource-centric pattern
- Use enums for actions, group by resource
- Parse/aggregate permissions using static methods

## REST/OpenAPI Alignment
- Refer to the OpenAPI spec for understanding endpoint behavior, status codes, error handling
- Handle default values (e.g., "*" for collections, "minimal" for verbosity)
- Only throw documented exceptions

## Common Pitfalls
- Don’t edit generated files
- Use enum helpers wherever possible
- Test isolation: use provided helpers
- Version checks: use `RequireVersion()`

---

