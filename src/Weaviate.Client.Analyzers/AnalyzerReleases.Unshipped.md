
### New Rules

Rule ID     | Category | Severity | Notes
------------|----------|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------
WEAVIATE004 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Wrong metrics attribute for aggregate type. Triggers when using mismatched attribute (e.g., NumberMetrics on Aggregate.Text).
WEAVIATE005 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Aggregate property missing suffix. Triggers when ToTyped<T>() or MetricsExtractor.FromType<T>() is called with the type.
WEAVIATE006 | Usage    | Warning  | AggregatePropertySuffixAnalyzer: Invalid type for aggregate suffix. Triggers when ToTyped<T>() or MetricsExtractor.FromType<T>() is called with the type.
