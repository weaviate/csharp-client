# Aggregate Result Accessors

## Overview

The Weaviate C# client provides **strongly-typed accessor methods** for working with aggregation results. These accessors eliminate the need to manually cast from the `Properties` dictionary, providing compile-time type safety and improved developer experience.

### Benefits

- **Type Safety**: No more manual casting from `object` to specific aggregation types
- **Null Safety**: Methods return `null` when property doesn't exist or is wrong type
- **TryGet Pattern**: Familiar .NET pattern for safe access with output parameters
- **Pattern Matching**: Lambda-based matching for flexible handling of different aggregation types
- **IDE Support**: Full IntelliSense for all accessor methods

## Quick Start

### Before (Manual Casting)

```csharp
var result = await collection.Aggregate.OverAll(
    returnMetrics: [Metrics.ForProperty("price").Number()]
);

// Manual casting - error-prone
var priceAgg = result.Properties["price"] as Aggregate.Number;
if (priceAgg != null)
{
    Console.WriteLine($"Average price: {priceAgg.Mean}");
}
```

### After (Typed Accessors)

```csharp
var result = await collection.Aggregate.OverAll(
    returnMetrics: [Metrics.ForProperty("price").Number()]
);

// Option 1: Direct accessor
var priceAgg = result.Number("price");
if (priceAgg != null)
{
    Console.WriteLine($"Average price: {priceAgg.Mean}");
}

// Option 2: TryGet pattern
if (result.TryGetNumber("price", out var price))
{
    Console.WriteLine($"Average price: {price.Mean}");
}

// Option 3: Pattern matching
result.Match("price",
    number: n => Console.WriteLine($"Average price: {n.Mean}")
);
```

## Direct Accessor Methods

Simple methods that return the typed aggregation or `null`.

### Available Methods

| Method | Returns | Use Case |
|--------|---------|----------|
| `Text(propertyName)` | `Aggregate.Text?` | Text/string properties |
| `Integer(propertyName)` | `Aggregate.Integer?` | Int/long properties |
| `Number(propertyName)` | `Aggregate.Number?` | Float/double properties |
| `Boolean(propertyName)` | `Aggregate.Boolean?` | Boolean properties |
| `Date(propertyName)` | `Aggregate.Date?` | DateTime properties |

### Example

```csharp
var result = await collection.Aggregate.OverAll(
    returnMetrics: [
        Metrics.ForProperty("title").Text(),
        Metrics.ForProperty("price").Number(),
        Metrics.ForProperty("quantity").Integer(),
        Metrics.ForProperty("inStock").Boolean(),
        Metrics.ForProperty("createdAt").Date()
    ]
);

// Access text aggregation
var titleAgg = result.Text("title");
if (titleAgg != null)
{
    Console.WriteLine($"Title count: {titleAgg.Count}");
    foreach (var occurrence in titleAgg.TopOccurrences)
    {
        Console.WriteLine($"  {occurrence.Value}: {occurrence.Count}");
    }
}

// Access numeric aggregation
var priceAgg = result.Number("price");
if (priceAgg != null)
{
    Console.WriteLine($"Price - Min: {priceAgg.Minimum}, Max: {priceAgg.Maximum}, Mean: {priceAgg.Mean}");
}

// Access integer aggregation
var quantityAgg = result.Integer("quantity");
if (quantityAgg != null)
{
    Console.WriteLine($"Total quantity: {quantityAgg.Sum}");
}

// Access boolean aggregation
var inStockAgg = result.Boolean("inStock");
if (inStockAgg != null)
{
    Console.WriteLine($"In stock: {inStockAgg.PercentageTrue:P0}");
}

// Access date aggregation
var dateAgg = result.Date("createdAt");
if (dateAgg != null)
{
    Console.WriteLine($"Date range: {dateAgg.Minimum} to {dateAgg.Maximum}");
}
```

## TryGet Methods

Methods following the .NET `TryGet` pattern for safe access with output parameters.

### Available Methods

| Method | Output Parameter | Returns |
|--------|------------------|---------|
| `TryGetText(propertyName, out text)` | `Aggregate.Text` | `bool` |
| `TryGetInteger(propertyName, out integer)` | `Aggregate.Integer` | `bool` |
| `TryGetNumber(propertyName, out number)` | `Aggregate.Number` | `bool` |
| `TryGetBoolean(propertyName, out boolean)` | `Aggregate.Boolean` | `bool` |
| `TryGetDate(propertyName, out date)` | `Aggregate.Date` | `bool` |
| `TryGet<T>(propertyName, out aggregation)` | `T` | `bool` |

### Example

```csharp
// Specific TryGet methods
if (result.TryGetNumber("price", out var price))
{
    Console.WriteLine($"Average price: {price.Mean}");
    Console.WriteLine($"Price range: {price.Minimum} - {price.Maximum}");
}
else
{
    Console.WriteLine("Price aggregation not available");
}

if (result.TryGetText("category", out var category))
{
    Console.WriteLine($"Categories found: {category.Count}");
}

// Generic TryGet
if (result.TryGet<Aggregate.Boolean>("inStock", out var stockAgg))
{
    Console.WriteLine($"In stock: {stockAgg.TotalTrue}, Out of stock: {stockAgg.TotalFalse}");
}
```

## Pattern Matching Methods

Lambda-based methods for flexible handling of aggregation types.

### Property<T> Method

Execute an action or function on a specific type:

```csharp
// Action variant - execute if matches
result.Property<Aggregate.Number>("price", price =>
{
    Console.WriteLine($"Min: {price.Minimum}");
    Console.WriteLine($"Max: {price.Maximum}");
    Console.WriteLine($"Mean: {price.Mean}");
});

// Func variant - return a value
var summary = result.Property<Aggregate.Number, string>("price",
    price => $"Price range: ${price.Minimum} - ${price.Maximum}"
);
```

### Match Method

Handle multiple types with a single call:

```csharp
// Action variant - execute the matching handler
result.Match("field",
    text: t => Console.WriteLine($"Text: {t.Count} occurrences"),
    integer: i => Console.WriteLine($"Integer: sum={i.Sum}"),
    number: n => Console.WriteLine($"Number: mean={n.Mean}"),
    boolean: b => Console.WriteLine($"Boolean: {b.PercentageTrue:P0} true"),
    date: d => Console.WriteLine($"Date range: {d.Minimum} to {d.Maximum}")
);

// Func variant - return a value based on type
var description = result.Match<string>("field",
    text: t => $"Text with {t.TopOccurrences.Count} unique values",
    integer: i => $"Integer ranging from {i.Minimum} to {i.Maximum}",
    number: n => $"Number with mean {n.Mean:F2}",
    boolean: b => $"Boolean: {b.PercentageTrue:P0} true",
    date: d => $"Dates from {d.Minimum:d} to {d.Maximum:d}"
);

Console.WriteLine(description ?? "Unknown type");
```

### Handling All Properties

Iterate through all properties with type-safe handling:

```csharp
foreach (var (name, _) in result.Properties)
{
    var description = result.Match<string>(name,
        text: t => $"{name}: {t.Count} items, top: {t.TopOccurrences.FirstOrDefault()?.Value}",
        integer: i => $"{name}: range [{i.Minimum}, {i.Maximum}], sum: {i.Sum}",
        number: n => $"{name}: range [{n.Minimum:F2}, {n.Maximum:F2}], mean: {n.Mean:F2}",
        boolean: b => $"{name}: {b.TotalTrue} true, {b.TotalFalse} false",
        date: d => $"{name}: {d.Minimum:d} to {d.Maximum:d}"
    );

    Console.WriteLine(description ?? $"{name}: unsupported type");
}
```

## GroupBy Results

All accessor methods are also available on `AggregateGroupByResult.Group`:

```csharp
var result = await collection.Aggregate.OverAll(
    groupBy: "category",
    returnMetrics: [Metrics.ForProperty("price").Number()]
);

foreach (var group in result.Groups)
{
    Console.WriteLine($"Category: {group.GroupedBy.Value}");
    Console.WriteLine($"  Count: {group.TotalCount}");

    // Direct accessor
    var priceAgg = group.Number("price");
    if (priceAgg != null)
    {
        Console.WriteLine($"  Avg Price: {priceAgg.Mean:C}");
    }

    // Or use TryGet
    if (group.TryGetNumber("price", out var price))
    {
        Console.WriteLine($"  Price Range: {price.Minimum:C} - {price.Maximum:C}");
    }

    // Or use Match
    group.Match("price",
        number: n => Console.WriteLine($"  Median Price: {n.Median:C}")
    );
}
```

## Aggregation Property Types

### Aggregate.Text

For text/string properties:

| Property | Type | Description |
|----------|------|-------------|
| `Count` | `long?` | Number of values |
| `TopOccurrences` | `List<TopOccurrence<string>>` | Most frequent values |

### Aggregate.Integer

For integer/long properties:

| Property | Type | Description |
|----------|------|-------------|
| `Count` | `long?` | Number of values |
| `Minimum` | `long?` | Minimum value |
| `Maximum` | `long?` | Maximum value |
| `Mean` | `double?` | Average value |
| `Median` | `double?` | Median value |
| `Mode` | `long?` | Most common value |
| `Sum` | `long?` | Sum of all values |

### Aggregate.Number

For float/double properties:

| Property | Type | Description |
|----------|------|-------------|
| `Count` | `long?` | Number of values |
| `Minimum` | `double?` | Minimum value |
| `Maximum` | `double?` | Maximum value |
| `Mean` | `double?` | Average value |
| `Median` | `double?` | Median value |
| `Mode` | `double?` | Most common value |
| `Sum` | `double?` | Sum of all values |

### Aggregate.Boolean

For boolean properties:

| Property | Type | Description |
|----------|------|-------------|
| `Count` | `long?` | Number of values |
| `TotalTrue` | `long` | Count of true values |
| `TotalFalse` | `long` | Count of false values |
| `PercentageTrue` | `double` | Percentage of true values (0-1) |
| `PercentageFalse` | `double` | Percentage of false values (0-1) |

### Aggregate.Date

For date/datetime properties:

| Property | Type | Description |
|----------|------|-------------|
| `Count` | `long?` | Number of values |
| `Minimum` | `DateTime?` | Earliest date |
| `Maximum` | `DateTime?` | Latest date |
| `Median` | `DateTime?` | Median date |
| `Mode` | `DateTime?` | Most common date |

## Best Practices

### 1. Choose the Right Accessor

```csharp
// Use direct accessor when you just need the value
var price = result.Number("price");

// Use TryGet when you need to handle missing properties
if (result.TryGetNumber("price", out var p))
{
    // Use p
}

// Use Match when handling multiple possible types
result.Match("unknownField",
    text: t => /* handle text */,
    number: n => /* handle number */
);
```

### 2. Handle Null Gracefully

```csharp
// Direct accessor returns null if not found or wrong type
var price = result.Number("price");
var average = price?.Mean ?? 0;

// Or use null-conditional operator
Console.WriteLine($"Mean: {result.Number("price")?.Mean}");
```

### 3. Use Pattern Matching for Unknown Types

```csharp
// When you don't know the type at compile time
foreach (var propertyName in propertyNames)
{
    result.Match(propertyName,
        text: t => ProcessText(t),
        integer: i => ProcessInteger(i),
        number: n => ProcessNumber(n),
        boolean: b => ProcessBoolean(b),
        date: d => ProcessDate(d)
    );
}
```

## API Reference

### AggregateResult Methods

```csharp
// Direct accessors
Aggregate.Text? Text(string propertyName)
Aggregate.Integer? Integer(string propertyName)
Aggregate.Number? Number(string propertyName)
Aggregate.Boolean? Boolean(string propertyName)
Aggregate.Date? Date(string propertyName)

// TryGet methods
bool TryGetText(string propertyName, out Aggregate.Text text)
bool TryGetInteger(string propertyName, out Aggregate.Integer integer)
bool TryGetNumber(string propertyName, out Aggregate.Number number)
bool TryGetBoolean(string propertyName, out Aggregate.Boolean boolean)
bool TryGetDate(string propertyName, out Aggregate.Date date)
bool TryGet<T>(string propertyName, out T aggregation) where T : Aggregate.Property

// Pattern matching
bool Property<T>(string propertyName, Action<T> action) where T : Aggregate.Property
TResult? Property<T, TResult>(string propertyName, Func<T, TResult> func) where T : Aggregate.Property
bool Match(string propertyName,
    Action<Aggregate.Text>? text = null,
    Action<Aggregate.Integer>? integer = null,
    Action<Aggregate.Number>? number = null,
    Action<Aggregate.Boolean>? boolean = null,
    Action<Aggregate.Date>? date = null)
TResult? Match<TResult>(string propertyName,
    Func<Aggregate.Text, TResult>? text = null,
    Func<Aggregate.Integer, TResult>? integer = null,
    Func<Aggregate.Number, TResult>? number = null,
    Func<Aggregate.Boolean, TResult>? boolean = null,
    Func<Aggregate.Date, TResult>? date = null)
```

### AggregateGroupByResult.Group Methods

All the same methods are available on `AggregateGroupByResult.Group`.

## Typed Aggregate Results with ToTyped&lt;T&gt;()

In addition to the accessor methods described above, you can use the `ToTyped<T>()` extension method to automatically map aggregation results to a strongly-typed object. This is particularly useful when you want to work with aggregation data in a structured way.

### How It Works

1. Execute an aggregation query using the `AggregateClient`
2. Call `ToTyped<T>()` on the result to map it to your custom type
3. Access the mapped properties directly

The mapper supports two approaches for property definitions:

1. **Full Aggregate Types**: Use `Aggregate.Text`, `Aggregate.Integer`, etc. as property types to get the complete aggregation data
2. **Suffix-Based Extraction**: Use primitive types with specific suffixes to extract individual values (e.g., `PriceSum`, `RatingMean`, `TitleCount`)

### Example: Full Aggregation Types

When you want access to all aggregation properties, use the full `Aggregate.*` types:

```csharp
public class ArticleAggregations
{
    public Aggregate.Text? Title { get; set; }
    public Aggregate.Integer? WordCount { get; set; }
    public Aggregate.Number? Rating { get; set; }
    public Aggregate.Boolean? IsPublished { get; set; }
    public Aggregate.Date? PublishedDate { get; set; }
}

// Execute aggregation and convert to typed result
var collection = weaviate.UseCollection("Articles");
var result = await collection.Aggregate.OverAll(
    returnMetrics: [
        Metrics.ForProperty("title").Text(),
        Metrics.ForProperty("wordCount").Integer(),
        Metrics.ForProperty("rating").Number(),
        Metrics.ForProperty("isPublished").Boolean(),
        Metrics.ForProperty("publishedDate").Date()
    ]
);

// Convert to typed result
var typed = result.ToTyped<ArticleAggregations>();

// Access typed properties directly
Console.WriteLine($"Title count: {typed.Properties.Title?.Count}");
Console.WriteLine($"Average rating: {typed.Properties.Rating?.Mean}");
Console.WriteLine($"Word count range: {typed.Properties.WordCount?.Minimum} - {typed.Properties.WordCount?.Maximum}");
Console.WriteLine($"Published: {typed.Properties.IsPublished?.PercentageTrue:P0}");
```

### Example: Suffix-Based Extraction

Use property name suffixes to explicitly indicate which value to extract:

```csharp
public class ArticleSummary
{
    // Field "title" + suffix "Count" = TitleCount
    public long? TitleCount { get; set; }

    // Field "rating" + suffix "Mean" = RatingMean
    public double? RatingMean { get; set; }

    // Field "wordCount" + suffix "Sum" = WordCountSum
    public long? WordCountSum { get; set; }

    // Field "isPublished" + suffix "PercentageTrue" = IsPublishedPercentageTrue
    public double? IsPublishedPercentageTrue { get; set; }

    // Field "publishedDate" + suffix "Minimum" = PublishedDateMinimum
    public DateTime? PublishedDateMinimum { get; set; }
}

var result = await collection.Aggregate.OverAll(
    returnMetrics: [
        Metrics.ForProperty("title").Text(),
        Metrics.ForProperty("rating").Number(),
        Metrics.ForProperty("wordCount").Integer(),
        Metrics.ForProperty("isPublished").Boolean(),
        Metrics.ForProperty("publishedDate").Date()
    ]
);

// Convert to typed result with suffix-based extraction
var typed = result.ToTyped<ArticleSummary>();

// Access extracted values directly
Console.WriteLine($"Articles with titles: {typed.Properties.TitleCount}");
Console.WriteLine($"Average rating: {typed.Properties.RatingMean}");
Console.WriteLine($"Total words: {typed.Properties.WordCountSum}");
Console.WriteLine($"Percentage published: {typed.Properties.IsPublishedPercentageTrue:P0}");
Console.WriteLine($"Earliest: {typed.Properties.PublishedDateMinimum}");
```

### Available Suffixes

| Suffix | Applicable Types | Expected Property Type | Description |
|--------|-----------------|------------------------|-------------|
| `Count` | All | `long`, `int` | Number of values |
| `Sum` | Integer, Number | `long`, `int`, `double`, `float` | Sum of all values |
| `Mean`, `Average` | Integer, Number | `double`, `float` | Average value |
| `Min`, `Minimum` | Integer, Number, Date | `long`, `int`, `double`, `DateTime` | Minimum value |
| `Max`, `Maximum` | Integer, Number, Date | `long`, `int`, `double`, `DateTime` | Maximum value |
| `Median` | Integer, Number, Date | `double`, `DateTime` | Median value |
| `Mode` | Integer, Number, Date | `long`, `int`, `double`, `DateTime` | Most common value |
| `TotalTrue` | Boolean | `long`, `int` | Count of true values |
| `TotalFalse` | Boolean | `long`, `int` | Count of false values |
| `PercentageTrue` | Boolean | `double`, `float` | Percentage of true (0.0-1.0) |
| `PercentageFalse` | Boolean | `double`, `float` | Percentage of false (0.0-1.0) |
| `TopOccurrence` | Text | `string` | Most frequent value |
| `TopOccurrences` | Text | `List<Aggregate.TopOccurrence<string>>` | All top occurrences |

### Mixed Mapping

You can mix full types and suffix-based extraction in the same class:

```csharp
public class ProductStats
{
    // Full type for complete access
    public Aggregate.Number? Price { get; set; }

    // Extract specific values with suffixes
    public double? PriceMean { get; set; }
    public double? PriceSum { get; set; }
    public long? QuantitySum { get; set; }
    public long? CategoryCount { get; set; }
}
```

### Extracting Metrics from Types

Instead of manually specifying the `returnMetrics` parameter, you can use `MetricsExtractor.FromType<T>()` to automatically generate the appropriate metrics from your type definition:

```csharp
using Weaviate.Client.Models.Typed;

public class ProductStats
{
    public double? PriceMean { get; set; }
    public double? PriceMin { get; set; }
    public double? PriceMax { get; set; }
    public long? QuantitySum { get; set; }
    public Aggregate.Text? Category { get; set; }
}

// Automatically extract metrics from the type
var metrics = MetricsExtractor.FromType<ProductStats>();

// Use the extracted metrics in the query
var result = await collection.Aggregate.OverAll(returnMetrics: metrics);

// Convert to typed result
var typed = result.ToTyped<ProductStats>();
```

The `MetricsExtractor` analyzes your type and:

1. For **Aggregate.* types** (e.g., `Aggregate.Text`, `Aggregate.Number`), it enables all metric flags for that property
2. For **suffix-based properties** (e.g., `PriceMean`, `QuantitySum`), it enables only the specific metric flags needed

This approach ensures the query only requests the metrics you actually need, and the type definition serves as the single source of truth for both the query and the result mapping.

#### Example: Multiple Suffixes for Same Field

When you have multiple properties with different suffixes for the same field, the metrics are combined:

```csharp
public class PriceAnalysis
{
    public double? PriceMean { get; set; }
    public double? PriceMin { get; set; }
    public double? PriceMax { get; set; }
    public long? PriceCount { get; set; }
}

// This extracts a single Number metric for "price" with Mean, Minimum, Maximum, and Count enabled
var metrics = MetricsExtractor.FromType<PriceAnalysis>();
```

### GroupBy with Typed Results

Typed mapping also works with grouped aggregations using `ToTyped<T>()`:

```csharp
public class CategoryStats
{
    public Aggregate.Number? Rating { get; set; }
    public double? RatingMean { get; set; }
    public long? RatingCount { get; set; }
}

var result = await collection.Aggregate.OverAll(
    groupBy: new Aggregate.GroupBy("category"),
    returnMetrics: [Metrics.ForProperty("rating").Number()]
);

// Convert grouped result to typed
var typed = result.ToTyped<CategoryStats>();

foreach (var group in typed.Groups)
{
    Console.WriteLine($"Category: {group.GroupedBy.Value}");
    Console.WriteLine($"  Count: {group.TotalCount}");
    Console.WriteLine($"  Avg Rating: {group.Properties.Rating?.Mean}");
    // Or using suffix extraction:
    Console.WriteLine($"  Avg Rating: {group.Properties.RatingMean}");
}
```

### GroupBy with Multiple Properties

When grouping, you can aggregate multiple properties and access them all in a typed manner:

```csharp
public class ProductCategoryAnalysis
{
    // Price statistics
    public Aggregate.Number? Price { get; set; }
    public double? PriceMean { get; set; }
    public double? PriceMin { get; set; }
    public double? PriceMax { get; set; }

    // Stock statistics
    public Aggregate.Integer? StockQuantity { get; set; }
    public long? StockQuantitySum { get; set; }

    // Availability
    public double? InStockPercentageTrue { get; set; }
}

var result = await collection.Aggregate.OverAll(
    groupBy: new Aggregate.GroupBy("category"),
    returnMetrics: [
        Metrics.ForProperty("price").Number(),
        Metrics.ForProperty("stockQuantity").Integer(),
        Metrics.ForProperty("inStock").Boolean()
    ]
);

var typed = result.ToTyped<ProductCategoryAnalysis>();

// Generate a report for each category
foreach (var group in typed.Groups)
{
    Console.WriteLine($"=== {group.GroupedBy.Value} ===");
    Console.WriteLine($"Products in category: {group.TotalCount}");
    Console.WriteLine($"Price range: ${group.Properties.PriceMin:F2} - ${group.Properties.PriceMax:F2}");
    Console.WriteLine($"Average price: ${group.Properties.PriceMean:F2}");
    Console.WriteLine($"Total stock: {group.Properties.StockQuantitySum}");
    Console.WriteLine($"In stock: {group.Properties.InStockPercentageTrue:P0}");
    Console.WriteLine();
}
```

### Comparing Groups with LINQ

The typed groups work seamlessly with LINQ for analysis:

```csharp
public class SalesRegionStats
{
    public double? RevenueMean { get; set; }
    public double? RevenueSum { get; set; }
    public long? OrdersCount { get; set; }
}

var result = await collection.Aggregate.OverAll(
    groupBy: new Aggregate.GroupBy("region"),
    returnMetrics: [
        Metrics.ForProperty("revenue").Number(),
        Metrics.ForProperty("orders").Integer()
    ]
);

var typed = result.ToTyped<SalesRegionStats>();

// Find top performing regions
var topRegions = typed.Groups
    .Where(g => g.Properties.RevenueSum.HasValue)
    .OrderByDescending(g => g.Properties.RevenueSum)
    .Take(5);

foreach (var region in topRegions)
{
    Console.WriteLine($"{region.GroupedBy.Value}: ${region.Properties.RevenueSum:N0} total revenue");
}

// Calculate overall statistics
var totalRevenue = typed.Groups.Sum(g => g.Properties.RevenueSum ?? 0);
var avgRevenuePerRegion = typed.Groups.Average(g => g.Properties.RevenueMean ?? 0);

Console.WriteLine($"Total revenue across all regions: ${totalRevenue:N0}");
Console.WriteLine($"Average revenue per region: ${avgRevenuePerRegion:N0}");
```

### Nested GroupBy Analysis

Access group metadata alongside typed properties:

```csharp
public class AuthorStats
{
    public long? ArticlesCount { get; set; }
    public double? WordCountMean { get; set; }
    public long? WordCountSum { get; set; }
    public double? RatingMean { get; set; }
}

var result = await collection.Aggregate.OverAll(
    groupBy: new Aggregate.GroupBy("author"),
    returnMetrics: [
        Metrics.ForProperty("title").Text(),
        Metrics.ForProperty("wordCount").Integer(),
        Metrics.ForProperty("rating").Number()
    ]
);

var typed = result.ToTyped<AuthorStats>();

// Build a dictionary for quick lookups
var authorLookup = typed.Groups.ToDictionary(
    g => g.GroupedBy.Value?.ToString() ?? "Unknown",
    g => g.Properties
);

// Access specific author's stats
if (authorLookup.TryGetValue("John Doe", out var johnStats))
{
    Console.WriteLine($"John Doe has written {johnStats.ArticlesCount} articles");
    Console.WriteLine($"Total words: {johnStats.WordCountSum:N0}");
    Console.WriteLine($"Average rating: {johnStats.RatingMean:F1}/5");
}

// Find the most prolific author
var mostProlific = typed.Groups
    .OrderByDescending(g => g.Properties.ArticlesCount ?? 0)
    .FirstOrDefault();

if (mostProlific != null)
{
    Console.WriteLine($"Most prolific author: {mostProlific.GroupedBy.Value} " +
                      $"with {mostProlific.Properties.ArticlesCount} articles");
}
```

### Accessing the Untyped Result

The typed result always preserves access to the underlying untyped result:

```csharp
var result = await collection.Aggregate.OverAll(
    returnMetrics: [Metrics.ForProperty("rating").Number()]
);

var typed = result.ToTyped<ArticleAggregations>();

// Access untyped result when needed
var untypedResult = typed.Untyped;
Console.WriteLine($"Total count: {untypedResult.TotalCount}");

// Use accessor methods on untyped result
if (untypedResult.TryGetNumber("rating", out var rating))
{
    Console.WriteLine($"Rating median: {rating.Median}");
}
```

### Complete Example

Here's a complete example showing different ways to work with aggregate results:

```csharp
// Define your aggregation result type
public class ProductStats
{
    // Full aggregate types for complete access
    public Aggregate.Number? Price { get; set; }
    public Aggregate.Integer? Quantity { get; set; }

    // Suffix-based extraction for specific values
    public double? PriceMean { get; set; }
    public double? PriceMin { get; set; }
    public double? PriceMax { get; set; }
    public long? QuantitySum { get; set; }
    public long? CategoryCount { get; set; }
}

// Execute aggregation
var result = await collection.Aggregate.OverAll(
    returnMetrics: [
        Metrics.ForProperty("price").Number(),
        Metrics.ForProperty("quantity").Integer(),
        Metrics.ForProperty("category").Text()
    ]
);

// Option 1: Use accessor methods directly
var avgPrice = result.Number("price")?.Mean;

// Option 2: Use TryGet pattern
if (result.TryGetNumber("price", out var priceAgg))
{
    Console.WriteLine($"Price range: {priceAgg.Minimum} - {priceAgg.Maximum}");
}

// Option 3: Convert to typed object
var stats = result.ToTyped<ProductStats>();
Console.WriteLine($"Average price: {stats.Properties.PriceMean}");
Console.WriteLine($"Price range: {stats.Properties.PriceMin} - {stats.Properties.PriceMax}");
Console.WriteLine($"Total quantity: {stats.Properties.QuantitySum}");

// Full aggregate access is also available
Console.WriteLine($"Price median: {stats.Properties.Price?.Median}");
```

## Real-World Usage Scenarios

### Scenario 1: E-Commerce Dashboard

Build a product analytics dashboard with category breakdowns:

```csharp
public class CategoryDashboard
{
    public long? ProductCount { get; set; }
    public double? PriceMean { get; set; }
    public double? PriceMin { get; set; }
    public double? PriceMax { get; set; }
    public long? StockSum { get; set; }
    public double? InStockPercentageTrue { get; set; }
    public double? RatingMean { get; set; }
}

public async Task<Dictionary<string, CategoryDashboard>> GetCategoryDashboard(
    CollectionClient collection)
{
    var result = await collection.Aggregate.OverAll(
        groupBy: new Aggregate.GroupBy("category"),
        returnMetrics: [
            Metrics.ForProperty("name").Text(),
            Metrics.ForProperty("price").Number(),
            Metrics.ForProperty("stockLevel").Integer(),
            Metrics.ForProperty("inStock").Boolean(),
            Metrics.ForProperty("rating").Number()
        ]
    );

    var typed = result.ToTyped<CategoryDashboard>();

    return typed.Groups.ToDictionary(
        g => g.GroupedBy.Value?.ToString() ?? "Unknown",
        g => g.Properties
    );
}

// Usage
var dashboard = await GetCategoryDashboard(collection);
foreach (var (category, stats) in dashboard)
{
    Console.WriteLine($"Category: {category}");
    Console.WriteLine($"  Products: {stats.ProductCount}");
    Console.WriteLine($"  Price Range: ${stats.PriceMin:F2} - ${stats.PriceMax:F2}");
    Console.WriteLine($"  Avg Price: ${stats.PriceMean:F2}");
    Console.WriteLine($"  Total Stock: {stats.StockSum}");
    Console.WriteLine($"  Availability: {stats.InStockPercentageTrue:P0}");
    Console.WriteLine($"  Avg Rating: {stats.RatingMean:F1}/5");
}
```

### Scenario 2: Content Analytics

Analyze blog posts or articles by publication status and author:

```csharp
public class ContentMetrics
{
    // Publication stats
    public double? IsPublishedPercentageTrue { get; set; }
    public long? ViewCountSum { get; set; }
    public double? ViewCountMean { get; set; }

    // Content stats
    public long? WordCountSum { get; set; }
    public double? WordCountMean { get; set; }

    // Engagement
    public long? CommentCountSum { get; set; }
    public double? RatingMean { get; set; }

    // Categories
    public Aggregate.Text? Category { get; set; }
}

// Overall content metrics
var overallResult = await collection.Aggregate.OverAll(
    returnMetrics: [
        Metrics.ForProperty("isPublished").Boolean(),
        Metrics.ForProperty("viewCount").Integer(),
        Metrics.ForProperty("wordCount").Integer(),
        Metrics.ForProperty("commentCount").Integer(),
        Metrics.ForProperty("rating").Number(),
        Metrics.ForProperty("category").Text(topOccurrencesCount: 10)
    ]
);

var overall = overallResult.ToTyped<ContentMetrics>();
Console.WriteLine("=== Overall Content Metrics ===");
Console.WriteLine($"Published: {overall.Properties.IsPublishedPercentageTrue:P0}");
Console.WriteLine($"Total views: {overall.Properties.ViewCountSum:N0}");
Console.WriteLine($"Total words written: {overall.Properties.WordCountSum:N0}");
Console.WriteLine($"Average article length: {overall.Properties.WordCountMean:N0} words");
Console.WriteLine($"Average rating: {overall.Properties.RatingMean:F1}/5");

// Top categories
Console.WriteLine("\nTop Categories:");
foreach (var cat in overall.Properties.Category?.TopOccurrences ?? [])
{
    Console.WriteLine($"  {cat.Value}: {cat.Count} articles");
}

// Per-author breakdown
var byAuthorResult = await collection.Aggregate.OverAll(
    groupBy: new Aggregate.GroupBy("author"),
    returnMetrics: [
        Metrics.ForProperty("isPublished").Boolean(),
        Metrics.ForProperty("viewCount").Integer(),
        Metrics.ForProperty("wordCount").Integer(),
        Metrics.ForProperty("rating").Number()
    ]
);

var byAuthor = byAuthorResult.ToTyped<ContentMetrics>();
Console.WriteLine("\n=== Top Authors by Views ===");
var topAuthors = byAuthor.Groups
    .OrderByDescending(g => g.Properties.ViewCountSum ?? 0)
    .Take(5);

foreach (var author in topAuthors)
{
    Console.WriteLine($"{author.GroupedBy.Value}:");
    Console.WriteLine($"  Articles: {author.TotalCount}");
    Console.WriteLine($"  Total views: {author.Properties.ViewCountSum:N0}");
    Console.WriteLine($"  Avg views: {author.Properties.ViewCountMean:N0}");
}
```

### Scenario 3: Time-Based Analysis

Analyze data grouped by time periods:

```csharp
public class TimeSeriesStats
{
    public long? OrderCount { get; set; }
    public double? RevenueSum { get; set; }
    public double? RevenueMean { get; set; }
    public long? ItemsSoldSum { get; set; }
    public double? DiscountAppliedPercentageTrue { get; set; }
}

// Monthly revenue analysis
var monthlyResult = await collection.Aggregate.OverAll(
    groupBy: new Aggregate.GroupBy("orderMonth"),  // Assuming pre-computed month field
    returnMetrics: [
        Metrics.ForProperty("orderId").Text(),
        Metrics.ForProperty("revenue").Number(),
        Metrics.ForProperty("itemCount").Integer(),
        Metrics.ForProperty("discountApplied").Boolean()
    ]
);

var monthly = monthlyResult.ToTyped<TimeSeriesStats>();

// Sort by month and display
var sortedMonths = monthly.Groups
    .OrderBy(g => g.GroupedBy.Value)
    .ToList();

Console.WriteLine("Monthly Revenue Report:");
Console.WriteLine("Month\t\tOrders\t\tRevenue\t\tAvg Order");
Console.WriteLine(new string('-', 60));

foreach (var month in sortedMonths)
{
    Console.WriteLine($"{month.GroupedBy.Value}\t" +
                      $"{month.Properties.OrderCount}\t\t" +
                      $"${month.Properties.RevenueSum:N0}\t\t" +
                      $"${month.Properties.RevenueMean:N2}");
}

// Calculate growth
if (sortedMonths.Count >= 2)
{
    var lastMonth = sortedMonths[^1].Properties.RevenueSum ?? 0;
    var prevMonth = sortedMonths[^2].Properties.RevenueSum ?? 0;
    var growth = prevMonth > 0 ? (lastMonth - prevMonth) / prevMonth : 0;
    Console.WriteLine($"\nMonth-over-month growth: {growth:P1}");
}
```

### Scenario 4: Inventory Management

Track inventory levels and identify issues:

```csharp
public class InventoryStats
{
    public long? ProductCount { get; set; }
    public long? StockLevelSum { get; set; }
    public long? StockLevelMin { get; set; }
    public double? StockLevelMean { get; set; }
    public double? InStockPercentageTrue { get; set; }
    public double? NeedsReorderPercentageTrue { get; set; }
}

var warehouseResult = await collection.Aggregate.OverAll(
    groupBy: new Aggregate.GroupBy("warehouse"),
    returnMetrics: [
        Metrics.ForProperty("sku").Text(),
        Metrics.ForProperty("stockLevel").Integer(),
        Metrics.ForProperty("inStock").Boolean(),
        Metrics.ForProperty("needsReorder").Boolean()
    ]
);

var byWarehouse = warehouseResult.ToTyped<InventoryStats>();

Console.WriteLine("Warehouse Inventory Summary:");
foreach (var warehouse in byWarehouse.Groups.OrderBy(g => g.GroupedBy.Value))
{
    var stats = warehouse.Properties;
    Console.WriteLine($"\n{warehouse.GroupedBy.Value}:");
    Console.WriteLine($"  SKUs: {stats.ProductCount}");
    Console.WriteLine($"  Total units: {stats.StockLevelSum:N0}");
    Console.WriteLine($"  Avg stock per SKU: {stats.StockLevelMean:F0}");
    Console.WriteLine($"  In stock rate: {stats.InStockPercentageTrue:P0}");

    if (stats.NeedsReorderPercentageTrue > 0.2)
    {
        Console.WriteLine($"  ⚠️ {stats.NeedsReorderPercentageTrue:P0} of items need reorder!");
    }
}

// Identify warehouses with issues
var lowStockWarehouses = byWarehouse.Groups
    .Where(g => g.Properties.InStockPercentageTrue < 0.8)
    .ToList();

if (lowStockWarehouses.Any())
{
    Console.WriteLine("\n⚠️ Warehouses needing attention:");
    foreach (var wh in lowStockWarehouses)
    {
        Console.WriteLine($"  - {wh.GroupedBy.Value}: only {wh.Properties.InStockPercentageTrue:P0} in stock");
    }
}
```

### Scenario 5: Survey/Feedback Analysis

Analyze customer feedback data:

```csharp
public class FeedbackStats
{
    public long? ResponseCount { get; set; }
    public double? SatisfactionMean { get; set; }
    public double? NpsMean { get; set; }
    public double? WouldRecommendPercentageTrue { get; set; }
    public Aggregate.Text? TopIssues { get; set; }
}

var feedbackResult = await collection.Aggregate.OverAll(
    groupBy: new Aggregate.GroupBy("productLine"),
    returnMetrics: [
        Metrics.ForProperty("responseId").Text(),
        Metrics.ForProperty("satisfactionScore").Number(),
        Metrics.ForProperty("npsScore").Number(),
        Metrics.ForProperty("wouldRecommend").Boolean(),
        Metrics.ForProperty("issueCategory").Text(topOccurrencesCount: 5)
    ]
);

var feedback = feedbackResult.ToTyped<FeedbackStats>();

Console.WriteLine("Customer Feedback Analysis by Product Line:");
foreach (var product in feedback.Groups.OrderByDescending(g => g.Properties.SatisfactionMean))
{
    var stats = product.Properties;
    Console.WriteLine($"\n{product.GroupedBy.Value}:");
    Console.WriteLine($"  Responses: {stats.ResponseCount}");
    Console.WriteLine($"  Satisfaction: {stats.SatisfactionMean:F1}/5");
    Console.WriteLine($"  NPS: {stats.NpsMean:F0}");
    Console.WriteLine($"  Would recommend: {stats.WouldRecommendPercentageTrue:P0}");

    if (stats.TopIssues?.TopOccurrences.Any() == true)
    {
        Console.WriteLine("  Top reported issues:");
        foreach (var issue in stats.TopIssues.TopOccurrences.Take(3))
        {
            Console.WriteLine($"    - {issue.Value}: {issue.Count} reports");
        }
    }
}
```

### Analyzer Support

The Weaviate client includes a Roslyn analyzer that validates types used with `ToTyped<T>()` and `MetricsExtractor.FromType<T>()`. When you call either of these methods, the analyzer checks the type `T` for correct suffix usage:

```csharp
public class ProductStats
{
    public double? PriceMean { get; set; }      // Valid - suffix with correct type
    public long? QuantitySum { get; set; }      // Valid - suffix with correct type
    public Aggregate.Text? Category { get; set; } // Valid - full aggregate type
}

// The analyzer validates ProductStats when ToTyped<T>() or MetricsExtractor.FromType<T>() is called:
var stats = result.ToTyped<ProductStats>();
var metrics = MetricsExtractor.FromType<ProductStats>();

// If ProductStats had invalid properties, you'd see warnings:
// - Property 'Price' with no suffix: WEAVIATE002
// - Property 'PriceSum' with wrong type (string instead of numeric): WEAVIATE003
```

The analyzer produces these diagnostics at the `ToTyped<T>()` or `MetricsExtractor.FromType<T>()` call site:

- **WEAVIATE002**: Warning when a property has a primitive type but no recognized suffix
- **WEAVIATE003**: Warning when a suffix is used with an incompatible type (e.g., `TitleSum` for a text field)

## See Also

- [Weaviate Aggregation Documentation](https://weaviate.io/developers/weaviate/search/aggregate)
- [Typed Client Wrappers](TYPED_CLIENT_WRAPPER.md)
