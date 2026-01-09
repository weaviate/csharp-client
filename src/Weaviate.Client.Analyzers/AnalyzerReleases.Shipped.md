## Release 0.1.0

### New Rules

Rule ID     | Category | Severity | Notes
------------|----------|----------|-----------------------------------------------------------------
WEAVIATE001 | Usage    | Warning  | AutoArrayUsageAnalyzer - AutoArray should only be used as method parameter
WEAVIATE002 | Usage    | Error    | VectorizerFactoryAnalyzer - Missing property initialization
WEAVIATE003 | Usage    | Error    | VectorizerFactoryAnalyzer - Missing field in Weights calculation
WEAVIATE004 | Usage    | Error    | Hybrid search requires at least one of 'query' or 'vectors' parameters
WEAVIATE005 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Aggregate property missing suffix. Triggers when ToTyped<T>() or MetricsExtractor.FromType<T>() is called with the type.
WEAVIATE006 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Invalid type for aggregate suffix. Triggers when ToTyped<T>() or MetricsExtractor.FromType<T>() is called with the type.
WEAVIATE007 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Wrong metrics attribute for aggregate type. Triggers when using mismatched attribute (e.g., NumberMetrics on Aggregate.Text).
