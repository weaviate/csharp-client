
### New Rules

Rule ID     | Category | Severity | Notes
------------|----------|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------
WEAVIATE002 | Usage    | Error    | VectorizerFactoryAnalyzer: Missing property in VectorizerFactory call
WEAVIATE003 | Usage    | Error    | VectorizerFactoryAnalyzer: Missing weight field in multi-vector configuration
WEAVIATE004 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Wrong metrics attribute for aggregate type. Triggers when using mismatched attribute (e.g., NumberMetrics on Aggregate.Text).
WEAVIATE005 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Aggregate property missing suffix. Triggers when ToTyped<T>() or MetricsExtractor.FromType<T>() is called with the type.
WEAVIATE006 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Invalid type for aggregate suffix. Triggers when ToTyped<T>() or MetricsExtractor.FromType<T>() is called with the type.
