using System.Collections.Concurrent;
using Weaviate.Client.Models;

namespace Weaviate.Client.Cache;

/// <summary>
/// Simple cache for collection schemas to avoid redundant HTTP calls.
/// Not a performance optimization - just prevents fetching the same schema multiple times
/// within a session or when creating multiple typed collections for the same collection name.
/// </summary>
public class SchemaCache
{
    private readonly ConcurrentDictionary<string, CachedSchema> _cache = new();
    private readonly TimeSpan _ttl;

    /// <summary>
    /// Default shared instance with 5-minute TTL.
    /// </summary>
    public static SchemaCache Default { get; } = new SchemaCache(TimeSpan.FromMinutes(5));

    /// <summary>
    /// Creates a new schema cache with the specified TTL.
    /// </summary>
    /// <param name="ttl">Time-to-live for cached schemas. Default is 5 minutes.</param>
    public SchemaCache(TimeSpan? ttl = null)
    {
        _ttl = ttl ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Gets a cached schema or fetches it from the server if not cached or expired.
    /// </summary>
    /// <param name="collectionName">The collection name to get schema for.</param>
    /// <param name="fetcher">Function to fetch the schema from the server if not cached.</param>
    /// <returns>The collection configuration, or null if not found.</returns>
    public async Task<CollectionConfig?> GetOrFetch(
        string collectionName,
        Func<Task<CollectionConfig?>> fetcher
    )
    {
        // Check if we already have this schema cached
        if (_cache.TryGetValue(collectionName, out var cached))
        {
            // Check if cache entry is still fresh
            if (DateTime.UtcNow - cached.FetchedAt < _ttl)
            {
                return cached.Config;
            }

            // Cache expired - remove it
            _cache.TryRemove(collectionName, out _);
        }

        // Fetch from server
        var config = await fetcher();

        // Cache the result if found
        if (config != null)
        {
            _cache[collectionName] = new CachedSchema
            {
                Config = config,
                FetchedAt = DateTime.UtcNow,
            };
        }

        return config;
    }

    /// <summary>
    /// Invalidates (removes) the cached schema for a specific collection.
    /// Call this after modifying a collection's schema.
    /// </summary>
    /// <param name="collectionName">The collection name to invalidate.</param>
    /// <returns>True if the entry was found and removed, false otherwise.</returns>
    public bool Invalidate(string collectionName) => _cache.TryRemove(collectionName, out _);

    /// <summary>
    /// Clears all cached schemas.
    /// </summary>
    public void Clear() => _cache.Clear();

    /// <summary>
    /// Gets the number of schemas currently cached.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Checks if a schema is cached for the specified collection (regardless of expiration).
    /// </summary>
    public bool Contains(string collectionName) => _cache.ContainsKey(collectionName);

    /// <summary>
    /// Checks if a schema is cached and fresh (not expired) for the specified collection.
    /// </summary>
    public bool IsFresh(string collectionName)
    {
        if (!_cache.TryGetValue(collectionName, out var cached))
            return false;

        return DateTime.UtcNow - cached.FetchedAt < _ttl;
    }

    private record CachedSchema
    {
        public required CollectionConfig Config { get; init; }
        public DateTime FetchedAt { get; init; }
    }
}
