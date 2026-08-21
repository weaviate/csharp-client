# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [1.2.0] — 2026-08-21

### Highlights

- **Boost**: a query-time soft-ranking rescorer available on every search and generate method (Weaviate ≥ 1.38)
- **Diversity selection (MMR)**: on hybrid search (Weaviate ≥ 1.38.6) and on every `near*` search (Weaviate ≥ 1.37.0)
- **`BM25Operator.AndCross`**: cross-property keyword AND, with a client-side server-version guard
- New modules: `multi2vec-twelvelabs` vectorizer and `generative-deepseek`
- New `Namespaces` RBAC permission scope for the 1.38 `manage_namespaces` action
- Incremental backups, plus new `text2vec-aws` / `text2vec-google` / `text2vec-openai` / `text2vec-morph` / `generative-google` settings
- Multimodal vectorizer weights now actually reach the wire — and the two Google multimodal factories were transposing video and audio fields
- **Not binary-compatible with 1.1.x.** 125 public signatures gained trailing optional parameters; recompile against 1.2.0 (see *Changed*)

### Added

#### Query and Generate

- **Boost — query-time soft ranking** ([#355](https://github.com/weaviate/weaviate-csharp-client/pull/355)) — New `Models.Boost` and an optional `boost` parameter on every BM25, Hybrid and `near*` search and generate method. A boost re-scores rather than filters: non-matching objects rank lower but are never dropped. Build one with the `Boost.*` factories. gRPC only. Requires Weaviate ≥ 1.38.
- **Diversity selection via MMR** ([#366](https://github.com/weaviate/weaviate-csharp-client/pull/366)) — New `Models.Diversity` with `Diversity.MMR(uint? Limit = null, float? Balance = null)`, passed as the optional `diversitySelection` parameter; `Balance` is the trade-off in `[0.0, 1.0]` (`1.0` pure relevance, `0.0` pure diversity). Not offered on aggregation. **Floors differ by surface**: `near*` searches require Weaviate ≥ **1.37.0**, hybrid search ≥ **1.38.6**.
- **`BM25Operator.AndCross`** ([#365](https://github.com/weaviate/weaviate-csharp-client/pull/365)) — New keyword operator requiring every query token to appear across the searched properties taken together rather than within one; all of them must share tokenization and analyzer settings. Landed in Weaviate **1.39.0**, backported to **1.37.15** and **1.38.8**, so the guard is per minor branch: **1.37.15** passes, **1.38.7** is refused with `WeaviateVersionMismatchException`.
- **`searchOperator` on `Generate.BM25`** ([#365](https://github.com/weaviate/weaviate-csharp-client/pull/365)) — All four `GenerateClient` / `TypedGenerateClient<T>` BM25 overloads now take `searchOperator`, which was previously accepted everywhere except here, leaving `AndCross` unreachable from generative keyword search. Same position and version guard as on `Query.BM25`.

#### Vectorizers

- **`multi2vec-twelvelabs`** ([#367](https://github.com/weaviate/weaviate-csharp-client/pull/367)) — New `Models.Vectorizer.Multi2VecTwelveLabs` config record and `VectorizerFactory.Multi2VecTwelveLabs(...)` overloads taking `string[]` or `WeightedFields` field lists. Requires Weaviate ≥ 1.38.9 on the 1.38 branch, or ≥ 1.39.0.
- **`dimensions` on `text2vec-aws`** ([#354](https://github.com/weaviate/weaviate-csharp-client/pull/354)) — `Text2VecAWS.Dimensions` and an optional `dimensions` on the `Text2VecAWSBedrock(...)` and `Text2VecAWSSagemaker(...)` factories, for models with a configurable embedding size.
- **`location` on `text2vec-google`** ([#353](https://github.com/weaviate/weaviate-csharp-client/pull/353)) — `Vectorizer.Text2VecGoogle.Location` and an optional `location` parameter on `VectorizerFactory.Text2VecGoogleVertex(...)`, selecting the Vertex AI region.
- **`endpoint` on `text2vec-openai` and `text2vec-morph`** ([#359](https://github.com/weaviate/weaviate-csharp-client/pull/359)) — `Endpoint` on both config records and an optional `endpoint` on the `Text2VecOpenAI(...)` and `Text2VecMorph(...)` factories, for proxied or self-hosted deployments.
- **`apiEndpoint` on `Multi2VecGoogle`** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — `VectorizerFactory.Multi2VecGoogle(...)` takes an optional `apiEndpoint` selecting the Gemini API (`generativelanguage.googleapis.com`) rather than Vertex AI; `Multi2VecGoogleGemini(...)` also gained an optional `dimensions`.

#### Generative

- **`generative-deepseek`** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — New `GenerativeConfigFactory.Deepseek(...)` collection config and `GenerativeProviderFactory.Deepseek(...)` runtime provider for the `generative-deepseek` module. Requires Weaviate ≥ 1.36.19.
- **`location` on `generative-google`** ([#359](https://github.com/weaviate/weaviate-csharp-client/pull/359)) — `Location` on `GenerativeConfig.GoogleVertex` and `Generative.Providers.GoogleVertex`, with an optional `location` on the matching factory, selecting the Vertex AI region.

#### RBAC

- **`Namespaces` Permission Scope** ([#356](https://github.com/weaviate/weaviate-csharp-client/pull/356)) — New `Models.Permissions.Namespaces` scope with a `Manage` flag and a `Models.NamespacesResource` filter (`Namespace`, default `"*"`, an exact name or a regex), mapping the Weaviate 1.38 RBAC action `manage_namespaces`. Requires Weaviate ≥ 1.38.0.

#### Backup

- **Incremental Backups** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — `BackupCreateRequest.IncrementalBaseBackupId` names an existing backup to build on, so unchanged files are not copied again, and is surfaced on `List()` and `GetStatus()`. `Create` throws `WeaviateVersionMismatchException` below 1.37.0; the field is accepted from 1.34.18 but only read back from 1.37.6.

### Fixed

- **Multimodal Vectorizer Weights Never Reached the Wire** ([#359](https://github.com/weaviate/weaviate-csharp-client/pull/359)) — Weighted `multi2vec` factories assigned `VectorizerWeights`, but it never serialized, so per-modality weights were silently dropped and collections were created with uniform weighting. Weights now reach the wire, and are omitted when no modality carries any.
- **Google Multimodal Factories Transposed Video and Audio** ([#359](https://github.com/weaviate/weaviate-csharp-client/pull/359)) — `Multi2VecGoogle` and `Multi2VecGoogleGemini` passed their video and audio field lists into the `audioFields` and `depthFields` parameters, so video weights were labelled as audio and audio weights landed under a modality the module does not have, which can fail collection creation.
- **Empty Multimodal Modalities Sent as Empty Arrays** ([#359](https://github.com/weaviate/weaviate-csharp-client/pull/359)) — An empty `WeightedFields` or `string[]` serialized as e.g. `"textFields": []`, which the server rejects, so an image-only config failed collection creation and `Multi2VecBind` was unusable with a subset of its seven modalities. Empty is now sent as null.
- **`generative-google` Sent an Empty `location`** ([#359](https://github.com/weaviate/weaviate-csharp-client/pull/359)) — An empty `location` was sent as present-but-empty, suppressing the server-side default. Vertex now sets it only when non-empty; Gemini leaves it unset.
- **`multi2vec-twelvelabs` Sent `vectorizeCollectionName`** ([#367](https://github.com/weaviate/weaviate-csharp-client/pull/367)) — No `multi2vec` module reads the setting, but the client sent it, so it read back as though it had taken effect. Removed from `Multi2VecTwelveLabs` and its factories; the other `multi2vec` records keep theirs for now.
- **`AndCross` Version Guard Threw a Server-Side Exception Type** ([#365](https://github.com/weaviate/weaviate-csharp-client/pull/365)) — The client-side guard threw `WeaviateFeatureNotSupportedException`, a `WeaviateServerException`. It now throws `WeaviateVersionMismatchException`, matching every other client-side version gate; update catch blocks written for the old type.
- **Backup `Size` Dropped on List** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — `BackupClient.List()` discarded the size and incremental base id it had already parsed from the response, so `Backup.Size` was null for every listed backup. Both fields are now mapped.
- **Aggregate Zeros for Absent Values** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — Aggregation scalars, `Count` included, reported `0`, `0.0` or `false` where the server had sent no value. They are now null when unset, so absence is distinguishable from a genuine zero.
- **`Multi2VecGoogleGemini` Emitted a Non-Existent Module** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — The vectorizer declared the module name `multi2vec-google-gemini`, which no server provides, so such a collection could never be created. It now emits `multi2vec-google` with the Gemini API endpoint.

### Changed

- **`Multi2VecGoogleGemini` is now an `[Obsolete]` shim over `Multi2VecGoogle`** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — The factory now returns a `Multi2VecGoogle`, so `is`/`as`/pattern matches on `Multi2VecGoogleGemini` silently stop matching and a `switch` with arms for both types no longer compiles (`CS8120`). Bind the result as `Multi2VecGoogle` or `VectorizerConfig`.
- **`Multi2VecGoogle.ProjectId` and `.Location` are no longer `required`** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — Both are now `string?`, so a Gemini config can omit them. Callers with nullable reference types enabled may see CS8600/CS8601.
- **`Aggregate.Boolean` counts are nullable** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — `PercentageTrue`, `PercentageFalse`, `TotalTrue` and `TotalFalse` changed from `double`/`long` to `double?`/`long?`.
- **`BackupCreateRequest` gained a trailing optional parameter** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — Its primary constructor and `Deconstruct` went from six parameters to seven, so six-element positional deconstruction no longer compiles — deconstruct seven.
- **Binary compatibility** — Source-compatible for callers using named arguments, but not binary-compatible: 125 public members gained a trailing optional parameter or a nullability change, so assemblies compiled against 1.1.0 or 1.1.1 throw `MissingMethodException` until recompiled against 1.2.0. **Migration: recompile.** No source change is needed unless you used positional arguments past the insertion point, deconstructed `BackupCreateRequest` positionally, or pattern-matched on `Multi2VecGoogleGemini` (see above).
- **`Weaviate.Client.VectorData` republished at 1.2.0** — Ships under the same tag and version as the core client but has **no API or behavior changes**: its public surface is byte-identical to 1.1.0 and 1.1.1. The 1.2.0 build references the 1.2.0 core client, so upgrade the two together.

### Removed

- **`Multi2VecGoogleGemini`'s own members** ([#368](https://github.com/weaviate/weaviate-csharp-client/pull/368)) — Now that the type is a shim over `Multi2VecGoogle`, its seven own properties are gone, replaced by the base type's equivalents. Migrate `.Model` → `.ModelId`; the rest keep their names.

### Minimum Supported Weaviate Version

| Feature                                                                                  | Minimum Weaviate Version                 |
|------------------------------------------------------------------------------------------|------------------------------------------|
| Core client                                                                              | 1.32.0                                   |
| Diversity selection on `near-vector` / `near-object` / `near-text` / `near-media`        | 1.37.0                                   |
| Incremental backup create (`IncrementalBaseBackupId`)                                     | 1.37.0 (read-back from 1.37.6)           |
| `generative-deepseek`                                                                    | 1.36.19                                  |
| Boost (all search and generate methods)                                                   | 1.38.0 (silently ignored below)          |
| `Namespaces` RBAC permission scope                                                        | 1.38.0                                   |
| Diversity selection on hybrid search                                                      | 1.38.6                                   |
| `BM25Operator.AndCross`                                                                   | 1.39.0, backported to 1.37.15 and 1.38.8 |
| `multi2vec-twelvelabs`                                                                    | 1.38.9 or 1.39.0                         |

---

## [1.1.1] — 2026-05-27

### Added

- **`text2vec-digitalocean` Vectorizer** ([#339](https://github.com/weaviate/weaviate-csharp-client/issues/339)) — New `Models.Vectorizer.Text2VecDigitalOcean` config record and `VectorizerFactory.Text2VecDigitalOcean(...)`. `model` (e.g. `qwen3-embedding-0.6b`) is required and comes first; `baseURL` is optional.

### Fixed

- **Vector Index Type Defaulting** ([#341](https://github.com/weaviate/weaviate-csharp-client/pull/341)) — The client wrote `"hnsw"` into every empty `vectorIndexType`, defeating `DEFAULT_VECTOR_INDEX`, added in Weaviate 1.37.5. The field is now left unset on servers ≥ 1.37.5, and still set to `"hnsw"` on servers below 1.37.5 or of undetermined version.

### Removed

- **`ReplicationAsyncConfig.MaxWorkers` and `ReplicationAsyncConfig.AliveNodesCheckingFrequency`** — Both have been no-ops on the server since Weaviate 1.37.3 and are now removed. Code that sets them will not compile after upgrading; behavior is unchanged.

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

- **Audio Field Support** ([#302](https://github.com/weaviate/weaviate-csharp-client/pull/302)): `Multi2VecGoogle` and `Multi2VecGoogleGemini` vectorizers now support audio field configurations with configurable per-field weights. (`Multi2VecGoogleGemini` is deprecated in favour of `Multi2VecGoogle` — see Unreleased.)

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
- New vectorizers: `Multi2VecGoogleGemini` (never functional — see Unreleased) and `Multi2MultivecWeaviate`

### Added

#### Vector Index

- **HFresh Vector Index** ([#289](https://github.com/weaviate/weaviate-csharp-client/pull/289)): Support for the `hnsw-fresh` inverted-list-based ANN index introduced in Weaviate 1.36. Supports RQ quantization and multi-vector configurations. Requires Weaviate ≥ 1.36.0.

#### Replication

- **Async Replication Configuration** ([#294](https://github.com/weaviate/weaviate-csharp-client/pull/294)): New `ReplicationAsyncConfig` record with 14 optional `long?` fields for fine-grained tuning of Weaviate's async replication engine (worker counts, hashtree height, frequencies, timeouts, batch sizes, propagation limits). Exposed via `ReplicationConfig.AsyncConfig`. Requires Weaviate ≥ 1.36.0.

#### Vectorizers

- **Multi2VecGoogleGemini** ([#297](https://github.com/weaviate/weaviate-csharp-client/pull/297)): New vectorizer calling the Google Gemini API directly. Supports image, text, and video field weighting. No project ID or location required (unlike the Vertex AI variant). Defaults to `generativelanguage.googleapis.com`. **Correction:** this vectorizer declared the module name `multi2vec-google-gemini`, which no Weaviate server provides, so it could never create a collection; fixed in Unreleased.
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

[Unreleased]: https://github.com/weaviate/weaviate-csharp-client/compare/1.2.0...HEAD
[1.2.0]: https://github.com/weaviate/weaviate-csharp-client/compare/1.1.1...1.2.0
[1.1.1]: https://github.com/weaviate/weaviate-csharp-client/compare/1.1.0...1.1.1
[1.1.0]: https://github.com/weaviate/weaviate-csharp-client/compare/1.0.1...1.1.0
[1.0.1]: https://github.com/weaviate/weaviate-csharp-client/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/weaviate/weaviate-csharp-client/releases/tag/1.0.0
