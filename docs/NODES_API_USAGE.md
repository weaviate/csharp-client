# Nodes API Usage Guide

This guide covers the Weaviate C# client's cluster nodes functionality. It provides examples and best practices for querying node information and monitoring cluster health.

## Table of Contents

- [Overview](#overview)
- [Listing Nodes](#listing-nodes)
  - [Basic Usage](#basic-usage)
  - [Verbose Information](#verbose-information)
  - [Filtering by Collection](#filtering-by-collection)
- [Node Information Details](#node-information-details)
- [Monitoring Cluster Health](#monitoring-cluster-health)
- [Advanced Usage](#advanced-usage)

## Overview

The Nodes API allows you to query information about the nodes in your Weaviate cluster. This is useful for monitoring cluster health, understanding shard distribution, and tracking indexing status.

Key features include:

- **Minimal queries**: Get basic node information (name, version, status, git hash).
- **Verbose queries**: Get detailed information including shard distribution and statistics.
- **Collection filtering**: Query verbose information for specific collections.
- **Type-safe results**: Strongly-typed `ClusterNode` and `ClusterNodeVerbose` records.

Access the Nodes API through the cluster client:

```csharp
var nodes = client.Cluster.Nodes;
```

## Listing Nodes

### Basic Usage

Use the `List()` method to query basic node information:

```csharp
// Get minimal information
var nodes = await client.Cluster.Nodes.List();

foreach (var node in nodes)
{
    Console.WriteLine(node);
}

// Or access properties individually
foreach (var node in nodes)
{
    Console.WriteLine($"Node: {node.Name}");
    Console.WriteLine($"  Version: {node.Version}");
    Console.WriteLine($"  Status: {node.Status}");
    Console.WriteLine($"  Git Hash: {node.GitHash}");
}
```

**Example Output:**

```text
Node: weaviate-0
  Version: 1.27.0
  Status: Healthy
  Git Hash: abc123def
Node: weaviate-1
  Version: 1.27.0
  Status: Healthy
  Git Hash: abc123def
```

### Verbose Information

To get detailed information including shard details and statistics, use the `ListVerbose()` method:

```csharp
// Get verbose information
var nodes = await client.Cluster.Nodes.ListVerbose();

// Simply print the node with all details
foreach (var node in nodes)
{
    Console.WriteLine(node);
    Console.WriteLine(); // Empty line between nodes
}

// Or access properties individually
foreach (var node in nodes)
{
    Console.WriteLine($"Node: {node.Name}");
    Console.WriteLine($"  Version: {node.Version}");
    Console.WriteLine($"  Status: {node.Status}");
    Console.WriteLine($"  Total Objects: {node.Stats.ObjectCount}");
    Console.WriteLine($"  Total Shards: {node.Stats.ShardCount}");
    
    if (node.Shards != null)
    {
        Console.WriteLine($"  Shards:");
        foreach (var shard in node.Shards)
        {
            Console.WriteLine($"    - {shard.Collection}/{shard.Name}");
    
    foreach (var shard in node.Shards)
    {
        Console.WriteLine($"  Shard: {shard.Collection}/{shard.Name}");
        Console.WriteLine($"      Objects: {shard.ObjectCount}");
        Console.WriteLine($"      Status: {shard.VectorIndexingStatus}");
        Console.WriteLine($"      Compressed: {shard.Compressed}");
        Console.WriteLine($"      Queue Length: {shard.VectorQueueLength}");
    }
}
```

**Example Output:**

```text
Node: weaviate-0
  Version: 1.27.0
  Status: Healthy
  Total Objects: 10000
  Total Shards: 4
  Shards:
    - Article/shard-0
      Objects: 2500
      Status: Ready
      Compressed: false
      Queue Length: 0
    - Product/shard-0
      Objects: 7500
      Status: Ready
      Compressed: true
      Queue Length: 0
```

### Filtering by Collection

Query verbose information for a specific collection:

```csharp
var nodes = await client.Cluster.Nodes.ListVerbose(collection: "Article");

foreach (var node in nodes)
{
    Console.WriteLine($"Node: {node.Name}");
    foreach (var shard in node.Shards)
    {
        Console.WriteLine($"  Shard: {shard.Name}");
        Console.WriteLine($"    Objects: {shard.ObjectCount}");
        Console.WriteLine($"    Indexing Status: {shard.VectorIndexingStatus}");
        }
    }
}
```

## Node Information Details

### Printing Node Information

All cluster model classes include `ToString()` methods that provide formatted output:

```csharp
var nodes = await client.Cluster.Nodes.ListVerbose();

// Print complete node information
foreach (var node in nodes)
{
    Console.WriteLine(node);
}

// Print just statistics
Console.WriteLine(nodes[0].Stats); // "Objects: 10000, Shards: 4"

// Print individual shards
foreach (var shard in nodes[0].Shards)
{
    Console.WriteLine(shard);
}
```

### ClusterNode Properties

| Property   | Type     | Description                             |
|------------|----------|-----------------------------------------|
| `Name`     | `string` | The unique name of the node             |
| `Version`  | `string` | Weaviate version running on the node    |
| `Status`   | `string` | Current status (e.g., "Healthy", "Unknown") |
| `GitHash`  | `string` | Git commit hash of the Weaviate build   |

### ClusterNodeVerbose Properties

`ClusterNodeVerbose` inherits all properties from `ClusterNode` and adds:

| Property | Type                            | Description                     |
|----------|---------------------------------|---------------------------------|
| `Stats`  | `ClusterNodeVerbose.NodeStats`  | Aggregate statistics for the node |
| `Shards` | `ClusterNodeVerbose.Shard[]`    | Array of shard information      |

### ClusterNodeVerbose.NodeStats Properties

| Property      | Type  | Description                         |
|---------------|-------|-------------------------------------|
| `ObjectCount` | `int` | Total number of objects on the node |
| `ShardCount`  | `int` | Total number of shards on the node  |

### ClusterNodeVerbose.Shard Properties

| Property               | Type                   | Description                                                    |
|------------------------|------------------------|----------------------------------------------------------------|
| `Collection`           | `string`               | Name of the collection this shard belongs to                   |
| `Name`                 | `string`               | Name of the shard                                              |
| `Node`                 | `string`               | Name of the node hosting this shard                            |
| `ObjectCount`          | `int`                  | Number of objects in this shard                                |
| `VectorIndexingStatus` | `VectorIndexingStatus` | Indexing status (Ready, Indexing, ReadOnly)                    |
| `VectorQueueLength`    | `int`                  | Number of vectors waiting to be indexed                        |
| `Compressed`           | `bool`                 | Whether the shard is compressed                                |
| `Loaded`               | `bool?`                | Whether the shard is loaded (not present in versions < 1.24.x) |

### VectorIndexingStatus Enum

| Value      | Description                           |
|------------|---------------------------------------|
| `Ready`    | Vector index is ready for queries     |
| `Indexing` | Vector index is currently being built |
| `ReadOnly` | Vector index is in read-only mode     |

## Monitoring Cluster Health

### Check All Nodes Are Healthy

```csharp
var nodes = await client.Cluster.Nodes.List();
var allHealthy = nodes.All(n => n.Status == "Healthy");

if (allHealthy)
{
    Console.WriteLine("All nodes are healthy!");
}
else
{
    var unhealthyNodes = nodes.Where(n => n.Status != "Healthy");
    foreach (var node in unhealthyNodes)
    {
        Console.WriteLine($"Warning: Node {node.Name} is {node.Status}");
    }
}
```

### Monitor Indexing Progress

```csharp
var nodes = await client.Cluster.Nodes.ListVerbose();

foreach (var node in nodes)
{
    var indexingShards = node.Shards?
        .Where(s => s.VectorIndexingStatus == VectorIndexingStatus.Indexing)
        .ToList() ?? new List<Shard>();
    
    if (indexingShards.Any())
    {
    Console.WriteLine($"Node {node.Name}:");
    
    var indexingShards = node.Shards
        .Where(s => s.VectorIndexingStatus == VectorIndexingStatus.Indexing)
        .ToList();
    
    if (indexingShards.Count > 0)
    {
        Console.WriteLine($"Node {node.Name} has {indexingShards.Count} shards indexing:");
        foreach (var shard in indexingShards)
        {
            Console.WriteLine($"  {shard.Collection}/{shard.Name}: {shard.VectorQueueLength} vectors queued");
        }
    }
}
```

### Check Shard Distribution

```csharp
var nodes = await client.Cluster.Nodes.ListVerbose();

var shardDistribution = nodes
    .SelectMany(n => n.Shards.Select(s => new { Node = n.Name, s.Collection, s.Name }))
    .GroupBy(x => x.Collection)
    .Select(g => new
    {
        Collection = g.Key,
        ShardCount = g.Count(),
        NodeDistribution = g.GroupBy(x => x.Node).Select(ng => new
        {
            Node = ng.Key,
            Shards = ng.Count()
        })
    });

foreach (var dist in shardDistribution)
{
    Console.WriteLine($"Collection: {dist.Collection}");
    Console.WriteLine($"  Total Shards: {dist.ShardCount}");
    foreach (var node in dist.NodeDistribution)
    {
        Console.WriteLine($"    {node.Node}: {node.Shards} shards");
    }
}
```

## Advanced Usage

### Detect Version Mismatches

```csharp
var nodes = await client.Cluster.Nodes.List();
var versions = nodes.Select(n => n.Version).Distinct().ToList();

if (versions.Count > 1)
{
    Console.WriteLine("Warning: Nodes are running different versions!");
    foreach (var version in versions)
    {
        var nodesWithVersion = nodes.Where(n => n.Version == version);
        Console.WriteLine($"  Version {version}: {string.Join(", ", nodesWithVersion.Select(n => n.Name))}");
    }
}
else
{
    Console.WriteLine($"All nodes running version {versions.First()}");
}
```

### Monitor Compression Status

```csharp
var nodes = await client.Cluster.Nodes.ListVerbose(collection: "LargeCollection");

foreach (var node in nodes)
{
    var compressedShards = node.Shards.Count(s => s.Compressed);
    var totalShards = node.Shards.Length;
    var compressionRate = (double)compressedShards / totalShards * 100;
    
    Console.WriteLine($"Node {node.Name}: {compressionRate:F1}% shards compressed ({compressedShards}/{totalShards})");
}
```

### Wait for Indexing to Complete

```csharp
async Task WaitForIndexingComplete(string collectionName, TimeSpan timeout)
{
    var startTime = DateTime.UtcNow;
    
    while (DateTime.UtcNow - startTime < timeout)
    {
        var nodes = await client.Cluster.Nodes.ListVerbose(collection: collectionName);
        
        var allReady = nodes.All(n => 
            n.Shards.All(s => s.VectorIndexingStatus == VectorIndexingStatus.Ready));
        
        if (allReady)
        {
            Console.WriteLine($"All shards for {collectionName} are ready!");
            return;
        }
        
        var totalQueued = nodes.Sum(n => n.Shards.Sum(s => s.VectorQueueLength));
        Console.WriteLine($"Waiting for indexing... {totalQueued} vectors queued");
        
        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    throw new TimeoutException($"Indexing did not complete within {timeout}");
}

// Usage
await WaitForIndexingComplete("Article", TimeSpan.FromMinutes(10));
```

### Calculate Cluster Statistics

```csharp
var nodes = await client.Cluster.Nodes.ListVerbose();

var clusterStats = new
{
    TotalNodes = nodes.Length,
    TotalObjects = nodes.Sum(n => n.Stats.ObjectCount),
    TotalShards = nodes.Sum(n => n.Stats.ShardCount),
    HealthyNodes = nodes.Count(n => n.Status == "Healthy"),
    AverageObjectsPerNode = nodes.Average(n => n.Stats.ObjectCount),
    AverageShardsPerNode = nodes.Average(n => n.Stats.ShardCount)
};

Console.WriteLine($"Cluster Statistics:");
Console.WriteLine($"  Nodes: {clusterStats.TotalNodes} ({clusterStats.HealthyNodes} healthy)");
Console.WriteLine($"  Total Objects: {clusterStats.TotalObjects:N0}");
Console.WriteLine($"  Total Shards: {clusterStats.TotalShards}");
Console.WriteLine($"  Avg Objects/Node: {clusterStats.AverageObjectsPerNode:N0}");
Console.WriteLine($"  Avg Shards/Node: {clusterStats.AverageShardsPerNode:F1}");
```
