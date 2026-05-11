# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [1.1.0] — 2026-05-11

### Highlights

- Weaviate 1.37 support: tokenization endpoints, blobHash property type, collection export, TextAnalyzerConfig per-property
- New `Weaviate.Client.VectorData` package: `IVectorStore` / `IVectorStoreRecordCollection` for Semantic Kernel and compatible AI frameworks
- Server-side batching via `BatchContext` streaming API
- Query profiling, delete-vector-index, MCP RBAC permissions
- Authentication hardening: `CancellationToken` threading, scoped DI token providers, deadlock fix in `GetClient()`

### Added

#### Collection Export

- **Collection Export** ([#324](https://github.com/weaviate/weaviate-csharp-client/pull/324)): New `ExportClient` accessible via `client.Export` and `collection.Export`. Supports `Create()`, `CreateAndWait()`, `GetStatus()`, and `CancelExport()`. `ExportOperation` tracks export progress with the same polling pattern as backup operations. Requires Weaviate ≥ 1.37.0.

#### Server-Side Batching

- **`BatchContext` Streaming Batch API** ([#305](https://github.com/weaviate/weaviate-csharp-client/pull/305)): `collection.Batch.StreamAsync()` opens a server-side batch session that streams objects and cross-references directly to the server without buffering all data in memory. `BatchContext.AddReference` enqueues cross-references alongside objects in the same stream. Requires Weaviate ≥ 1.27.0.

#### Tokenization

- **Tokenize Endpoints** ([#329](https://github.com/weaviate/weaviate-csharp-client/pull/329)): Expose `POST /v1/tokenize` and `POST /v1/schema/{class}/properties/{prop}/tokenize` introduced in Weaviate 1.37.0. Inspect how text is tokenized for a given analyzer configuration, or for a specific collection property. Access via `client.Tokenize.Text(...)` and `collection.Tokenize.Property(...)`. `AsciiFoldConfig` is modeled as a nullable record so the invalid "ignore without fold" state is unrepresentable. See [TOKENIZE_API_USAGE.md](docs/TOKENIZE_API_USAGE.md). Requires Weaviate ≥ 1.37.0.
- **Property-Level `TextAnalyzerConfig`** ([#329](https://github.com/weaviate/weaviate-csharp-client/pull/329)): `Property.TextAnalyzer` (also applies to nested properties) pins ASCII folding and/or a stopword preset per property at index time. Reuses the same `TextAnalyzerConfig` record from the Tokenize endpoint so tokenize-at-query and index-at-insert stay aligned. Raises `WeaviateVersionMismatchException` on `CollectionsClient.Create` when the server is older than 1.37.0.
- **Collection-Level `StopwordPresets`** ([#329](https://github.com/weaviate/weaviate-csharp-client/pull/329)): `InvertedIndexConfig.StopwordPresets` and `InvertedIndexConfigUpdate.StopwordPresets` define named preset → word-list maps. Properties reference presets via `TextAnalyzer.StopwordPreset`. Changes flow through `CollectionClient.Config.Update(...)`. Requires Weaviate ≥ 1.37.0.

#### Microsoft.Extensions.VectorData Integration

- **`Weaviate.Client.VectorData` Package** ([#312](https://github.com/weaviate/weaviate-csharp-client/pull/312)): New NuGet package implementing `IVectorStore` and `IVectorStoreRecordCollection<TKey, TRecord>` from `Microsoft.Extensions.VectorData.Abstractions`. Enables drop-in use of Weaviate with AI frameworks built on the shared VectorData abstraction (e.g., Semantic Kernel). Install via `dotnet add package Weaviate.Client.VectorData`.

#### Vector Index Management

- **Delete Vector Index** ([#310](https://github.com/weaviate/weaviate-csharp-client/pull/310)): `CollectionConfigClient.DeleteVectorIndex(name)` removes a named vector index from an existing collection without dropping the collection or its data. Requires Weaviate ≥ 1.37.0.

#### RBAC Permissions

- **MCP Permission Type** ([#321](https://github.com/weaviate/weaviate-csharp-client/pull/321)): New `Permission.Mcp` permission for granting Model Context Protocol actions in RBAC role configurations.

#### Query Profiling

- **`MetadataOptions.QueryProfile`** ([#318](https://github.com/weaviate/weaviate-csharp-client/pull/318)): New flag that requests per-phase timing from the server. Profiling data is exposed as `WeaviateResult.QueryProfile`.

#### Client Integration Headers

- **`X-Weaviate-Client-Integration` Header** ([#306](https://github.com/weaviate/weaviate-csharp-client/pull/306)): `WeaviateOptions.AddIntegration(name)` and `WeaviateClientBuilder.WithIntegration(name)` append integration agent tokens to the `X-Weaviate-Client-Integration` header. `WeaviateDefaults.IntegrationAgent(name)` builds a `"name/assemblyVersion"` token automatically. Values containing whitespace are rejected (space is the token separator). The gRPC client now always sends the `X-Weaviate-Client` header regardless of whether custom headers are present.

#### Property Types

- **`blobHash` Property Type** ([#336](https://github.com/weaviate/weaviate-csharp-client/pull/336)): New `BlobHashPropertyConverter` for reading `blobHash`-type properties from search results. Register via `PropertyConverterRegistry`. Requires Weaviate ≥ 1.37.0.

#### Vectorizers

- **Audio Field Support** ([#302](https://github.com/weaviate/weaviate-csharp-client/pull/302)): `Multi2VecGoogle` and `Multi2VecGoogleGemini` vectorizers now support audio field configurations with configurable per-field weights.

#### API Ergonomics

- **Nullable `Alpha` in Hybrid Search** ([#304](https://github.com/weaviate/weaviate-csharp-client/pull/304)): `HybridInput.Alpha` and `HybridAggregateInput.Alpha` are now nullable. Omitting the parameter defers to the server's default (0.75), removing the need to specify it explicitly on every query.

### Fixed

- **Authentication / Concurrency / Resource Safety** ([#337](https://github.com/weaviate/weaviate-csharp-client/pull/337)):
  - `ITokenService.GetAccessTokenAsync` and `RefreshTokenAsync` now accept an optional `CancellationToken`; a cancelled gRPC call also cancels the in-flight token fetch.
  - New `AddWeaviate<TTokenService>` DI overloads resolve a fresh token service from a DI scope per call, enabling multi-tenant and token-forwarding scenarios.
  - `WeaviateClientFactory.GetClient()` marked `[Obsolete]`; internals now use `Task.Run()` to escape `SynchronizationContext` and prevent deadlocks in ASP.NET Core hosts.
  - `_disposed` fields in `BackupOperationBase`, `ExportOperationBase`, and `ReplicationOperationTracker` made `volatile` to prevent data races between `Dispose()` and `DisposeInternal()`.
  - Background polling loops now catch `Exception when (ex is not OutOfMemoryException)` so CLR fatal exceptions are not suppressed.
- **Backup Disposal Leak** ([#331](https://github.com/weaviate/weaviate-csharp-client/pull/331)): `BackupOperationBase` and `BackupClient` now correctly dispose background polling tasks and `CancellationTokenSource` instances.
- **`ObjectTTLConfig` Null-vs-Disabled Equality** ([#307](https://github.com/weaviate/weaviate-csharp-client/pull/307)): The server returns `objectTtlConfig` with `enabled=false` for collections without TTL; client-side equality now null-coalesces to `ObjectTTLConfig.Disabled` to prevent spurious mismatches.
- **Null `vectorIndexConfig` Crash** ([#321](https://github.com/weaviate/weaviate-csharp-client/pull/321)): `VectorIndexSerialization.Factory` returns `null` instead of throwing when `vectorIndexConfig` is `null`, fixing `ConnectToLocal` failures against Weaviate 1.37.1.

### Changed

- `Property.IndexInverted` `[Obsolete]` attribute now includes a migration message.

### Minimum Supported Weaviate Version

| Feature                                                                                                         | Minimum Weaviate Version |
|-----------------------------------------------------------------------------------------------------------------|--------------------------|
| Core client                                                                                                     | 1.32.0                   |
| Delete vector index, tokenize endpoints, `TextAnalyzerConfig`, `StopwordPresets`, `blobHash`, collection export | 1.37.0                   |

---

## [1.0.1] — 2026-03-10

### Highlights

- Weaviate 1.36 support: HFresh vector index, async replication config, property index deletion
- Critical fix: gRPC vector serialization no longer doubles dimensions for non-`float[]` vectors
- Opt-in structured logging via `ILoggerFactory`
- New vectorizers: `Multi2VecGoogleGemini` and `Multi2MultivecWeaviate`

### Added

#### Vector Index

- **HFresh Vector Index** ([#289](https://github.com/weaviate/weaviate-csharp-client/pull/289)): Support for the `hnsw-fresh` inverted-list-based ANN index introduced in Weaviate 1.36. Supports RQ quantization and multi-vector configurations. Requires Weaviate ≥ 1.36.0.

#### Replication

- **Async Replication Configuration** ([#294](https://github.com/weaviate/weaviate-csharp-client/pull/294)): New `ReplicationAsyncConfig` record with 14 optional `long?` fields for fine-grained tuning of Weaviate's async replication engine (worker counts, hashtree height, frequencies, timeouts, batch sizes, propagation limits). Exposed via `ReplicationConfig.AsyncConfig`. Requires Weaviate ≥ 1.36.0.

#### Vectorizers

- **Multi2VecGoogleGemini** ([#297](https://github.com/weaviate/weaviate-csharp-client/pull/297)): New vectorizer calling the Google Gemini API directly. Supports image, text, and video field weighting. No project ID or location required (unlike the Vertex AI variant). Defaults to `generativelanguage.googleapis.com`.
- **Multi2MultivecWeaviate** ([#291](https://github.com/weaviate/weaviate-csharp-client/pull/291)): Support for the `multi2multivec-weaviate` vectorizer, which produces multi-vector embeddings using Weaviate's built-in model.
- **Cohere Reranker `BaseURL`** ([#287](https://github.com/weaviate/weaviate-csharp-client/pull/287)): Added `BaseURL` property to `RerankerCohereConfig` and a corresponding parameter to `RerankerConfigFactory.Cohere()`, enabling self-hosted or regional Cohere endpoints.

#### Backup

- **Cancel Restore** ([#292](https://github.com/weaviate/weaviate-csharp-client/pull/292)): New `BackupClient.CancelRestore()` method cancels an in-progress restore via `DELETE /backups/{backend}/{id}/restore`.
- **Backup `Size` Field** ([#292](https://github.com/weaviate/weaviate-csharp-client/pull/292)): The `Backup` model now exposes a `Size` field from the create-status response.
- **New Backup Status Values** ([#280](https://github.com/weaviate/weaviate-csharp-client/pull/280)): Added `BackupStatus.Cancelling` and `BackupStatus.Finalizing` enum values to reflect Weaviate 1.35+ server states.

#### Property Management

- **Drop Property Inverted Index** ([#288](https://github.com/weaviate/weaviate-csharp-client/pull/288)): New `CollectionConfigClient.DeletePropertyIndex()` removes a specific inverted index from an existing property without deleting the property itself. Requires Weaviate ≥ 1.36.0.

#### Logging and Observability

- **`ILoggerFactory`-based Structured Logging** ([#93](https://github.com/weaviate/weaviate-csharp-client/issues/93)): Production-ready, opt-in logging. By default the client is silent (`NullLoggerFactory`). Enable with `WeaviateClientBuilder.WithLoggerFactory()` and `UseRequestLogging(LogLevel)`.
  - HTTP logging: method, URI, status code, elapsed time; Authorization header values are redacted.
  - gRPC logging: method name, status, elapsed time; warnings on `RpcException`.

#### Generative Providers

- **`StopSequences` Property** ([#278](https://github.com/weaviate/weaviate-csharp-client/pull/278)): Added `StopSequences` to generative provider configs.

#### Object TTL

- **Object TTL Configuration** ([#277](https://github.com/weaviate/weaviate-csharp-client/pull/277)): Added configuration support for automatic object expiry via time-to-live.

#### API Ergonomics

- Added `CancellationToken` parameters to various configuration methods.
- `Vectorizer` class is now `public`, enabling custom extension scenarios.
- Vectorizer methods now use generic type parameters for improved type inference.

### Fixed

- **Vector Dimension Doubling in gRPC Serialization** ([#295](https://github.com/weaviate/weaviate-csharp-client/pull/295)): Vectors provided as `double[]` or other non-`float[]` types were serialised at native byte width instead of being downcast to `float32`, causing the server to receive a vector with double the declared dimensions. All non-`float[]` vectors are now converted to `float32` before byte serialisation.
- **`IDictionary` Properties Dropped in gRPC Batch Inserts**: `BuildBatchProperties` now correctly handles `IDictionary<string, object?>` inputs (including `ExpandoObject`), matching the REST path behaviour.
- **Backup Restore Behaviour (Weaviate 1.36)**: In Weaviate 1.36, restoring over an existing collection returns `FAILED` status instead of raising an exception. Updated accordingly.

### Changed

- `NearText` and `NearVector` parameter renamed from `input` to `query` for clarity.
- Enum serialization refactored from `Newtonsoft.Json` attributes to `System.Text.Json`.

### Minimum Supported Weaviate Version

| Feature                                                   | Minimum Weaviate Version |
|-----------------------------------------------------------|--------------------------|
| Core client                                               | 1.32.0                   |
| `DeletePropertyIndex`, `ReplicationAsyncConfig`, `HFresh` | 1.36.0                   |

---

## [1.0.0] — 2026-01-12

Initial stable release of the Weaviate C# client.

### Key Features

- **Full REST and gRPC support** — All core Weaviate operations available over both transports, with automatic gRPC usage for batch inserts and vector search.
- **Collections API** — Create, configure, and manage collections with strongly-typed configuration objects for vectorizers, generative modules, rerankers, and vector indexes (HNSW, BQ, SQ, PQ, RQ).
- **Multi-tenancy** — Full tenant management with `AutoTenantCreation` and `AutoTenantActivation` support.
- **Generative AI** — Built-in support for generative queries and dynamic RAG across all major providers.
- **Backup and restore** — Complete backup lifecycle management with `BackupStorage` enum, compression levels, and async status polling.
- **Alias management** — Create, list, update, and delete collection aliases via `AliasClient`.
- **Typed property system** — `PropertyBag` with `PropertyConverterRegistry` for UUID, date, text, int, number, boolean, geo, and blob types.
- **`AutoArray<T>`** — Implicit single/array/list coercion for fluent query construction.
- **Version guardrails** — `[RequiresWeaviateVersion]` attribute + `WEAVIATE008` Roslyn analyzer enforce minimum server version requirements at compile time.
- **Roslyn analyzers** — WEAVIATE001–WEAVIATE008 covering API surface, vectorizer configuration, aggregate suffixes, and version guards.
- **Dependency injection** — `WeaviateClientFactory` with `IServiceCollection` integration.
- **Filter API** — `ContainsAny`, `ContainsNone`, `Filter.Not`, and nested filter composition.
- **Aggregate queries** — Strongly-typed aggregate results with `GroupBy` support.
- **Well-known endpoints** — Health check and liveness probe support.

---

[Unreleased]: https://github.com/weaviate/weaviate-csharp-client/compare/1.1.0...HEAD
[1.1.0]: https://github.com/weaviate/weaviate-csharp-client/compare/1.0.1...1.1.0
[1.0.1]: https://github.com/weaviate/weaviate-csharp-client/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/weaviate/weaviate-csharp-client/releases/tag/1.0.0
