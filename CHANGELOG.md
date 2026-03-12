# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]



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

[Unreleased]: https://github.com/weaviate/weaviate-csharp-client/compare/1.0.1...HEAD
[1.0.1]: https://github.com/weaviate/weaviate-csharp-client/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/weaviate/weaviate-csharp-client/releases/tag/1.0.0
