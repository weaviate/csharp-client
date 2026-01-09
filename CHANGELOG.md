# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Typed Aggregate Result Accessors**: Added strongly-typed accessor methods to `AggregateResult` and `AggregateGroupByResult.Group` for safer and more convenient access to aggregation properties. See [Aggregate Result Accessors documentation](docs/AGGREGATE_RESULT_ACCESSORS.md) for details.
  - Direct accessor methods: `Text()`, `Integer()`, `Number()`, `Boolean()`, `Date()`
  - TryGet methods: `TryGetText()`, `TryGetInteger()`, `TryGetNumber()`, `TryGetBoolean()`, `TryGetDate()`, `TryGet<T>()`
  - Lambda pattern methods: `Property<T>()`, `Match()`, `Match<TResult>()`

- **Typed Aggregate Results**: Added `ToTyped<T>()` extension method to map aggregate results to strongly-typed objects.
  - `AggregateResult<T>` and `AggregateGroupByResult<T>` typed result wrappers
  - Suffix-based value extraction: use property names like `PriceSum`, `RatingMean`, `TitleCount` to extract specific values
  - Full aggregate type mapping: use `Aggregate.Text`, `Aggregate.Number`, etc. for complete access
  - Case-insensitive property name matching

- **MetricsExtractor**: Added `MetricsExtractor.FromType<T>()` to automatically extract `returnMetrics` from a type definition
  - Analyzes property types and suffixes to generate the appropriate `Aggregate.Metric[]`
  - Combines multiple suffixes for the same field into a single metric
  - Works with both full `Aggregate.*` types and suffix-based properties

- **Aggregate Property Analyzer**: Added Roslyn analyzers that validate types used with `ToTyped<T>()`
  - WEAVIATE002: Warns when a primitive property lacks a recognized suffix
  - WEAVIATE003: Warns when a suffix is used with an incompatible type
