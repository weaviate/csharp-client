
### New Rules

Rule ID     | Category | Severity | Notes
------------|----------|----------|-----------------------------------------------------
WEAVIATE002 | Usage    | Warning  | Aggregate property missing suffix. Triggers when ToTyped<T>() or MetricsExtractor.FromType<T>() is called with the type.
WEAVIATE003 | Usage    | Warning  | Invalid type for aggregate suffix. Triggers when ToTyped<T>() or MetricsExtractor.FromType<T>() is called with the type.
WEAVIATE004 | Usage | Warning | Wrong metrics attribute for aggregate type. Triggers when using mismatched attribute (e.g., NumberMetrics on Aggregate.Text).
