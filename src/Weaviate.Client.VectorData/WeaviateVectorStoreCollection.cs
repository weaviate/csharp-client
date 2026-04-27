using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.VectorData;
using Weaviate.Client.Models;
using Weaviate.Client.VectorData.Filters;
using Weaviate.Client.VectorData.Mapping;

namespace Weaviate.Client.VectorData;

/// <summary>
/// Weaviate implementation of <see cref="VectorStoreCollection{TKey, TRecord}"/>.
/// Delegates all operations to the Weaviate C# client.
/// </summary>
/// <typeparam name="TKey">The key type. Must be <see cref="Guid"/> or <see cref="string"/>.</typeparam>
/// <typeparam name="TRecord">The record type.</typeparam>
public class WeaviateVectorStoreCollection<TKey, TRecord> : VectorStoreCollection<TKey, TRecord>
    where TKey : notnull
    where TRecord : class
{
    private readonly WeaviateClient _client;
    private readonly string _collectionName;
    private readonly WeaviateVectorStoreCollectionOptions? _options;
    private readonly VectorStoreCollectionDefinition? _definition;
    private readonly IWeaviateRecordMapper<TRecord> _mapper;
    private readonly Lazy<CollectionClient> _collectionClient;

    /// <summary>
    /// Initializes a new instance of <see cref="WeaviateVectorStoreCollection{TKey, TRecord}"/>.
    /// </summary>
    public WeaviateVectorStoreCollection(
        WeaviateClient client,
        string name,
        VectorStoreCollectionDefinition? definition = null,
        WeaviateVectorStoreCollectionOptions? options = null
    )
    {
        ValidateKeyType();

        _client = client ?? throw new ArgumentNullException(nameof(client));
        _collectionName = name ?? throw new ArgumentNullException(nameof(name));
        _definition = definition;
        _options = options;
        _mapper = CreateMapper(definition);
        _collectionClient = new Lazy<CollectionClient>(() => BuildCollectionClient());
    }

    /// <inheritdoc />
    public override string Name => _collectionName;

    /// <inheritdoc />
    public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        return _client.Collections.Exists(_collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task EnsureCollectionExistsAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (!await CollectionExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            var schema =
                _definition != null
                    ? VectorDataSchemaBuilder.BuildSchema(_collectionName, _definition)
                    : VectorDataSchemaBuilder.BuildSchema<TRecord>(_collectionName);

            await _client.Collections.Create(schema, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override async Task EnsureCollectionDeletedAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (await CollectionExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            await _client
                .Collections.Delete(_collectionName, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override async Task<TRecord?> GetAsync(
        TKey key,
        RecordRetrievalOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var guid = ConvertToGuid(key);
        var collection = _collectionClient.Value;
        var includeVectors = options?.IncludeVectors ?? false;

        var obj = await collection
            .Query.FetchObjectByID(
                guid,
                returnMetadata: new MetadataQuery(MetadataOptions.Distance),
                includeVectors: includeVectors,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        return obj == null ? default : _mapper.MapFromWeaviate(obj);
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<TRecord> GetAsync(
        IEnumerable<TKey> keys,
        RecordRetrievalOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var guids = keys.Select(ConvertToGuid).ToHashSet();
        var collection = _collectionClient.Value;
        var includeVectors = options?.IncludeVectors ?? false;

        var result = await collection
            .Query.FetchObjectsByIDs(
                guids,
                returnMetadata: new MetadataQuery(MetadataOptions.Distance),
                includeVectors: includeVectors,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        foreach (var obj in result.Objects)
        {
            yield return _mapper.MapFromWeaviate(obj);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<TRecord> GetAsync(
        Expression<Func<TRecord, bool>> filter,
        int top,
        FilteredRecordRetrievalOptions<TRecord>? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var weaviateFilter = WeaviateFilterTranslator.Translate(filter);
        var collection = _collectionClient.Value;
        var includeVectors = options?.IncludeVectors ?? false;
        var skip = options?.Skip ?? 0;

        var result = await collection
            .Query.FetchObjects(
                filters: weaviateFilter,
                limit: (uint)top,
                offset: skip > 0 ? (uint)skip : null,
                returnMetadata: new MetadataQuery(MetadataOptions.Distance),
                includeVectors: includeVectors,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        foreach (var obj in result.Objects)
        {
            yield return _mapper.MapFromWeaviate(obj);
        }
    }

    /// <inheritdoc />
    public override async Task UpsertAsync(
        TRecord record,
        CancellationToken cancellationToken = default
    )
    {
        var (key, properties, vectors) = _mapper.MapToWeaviate(record);
        var request = new BatchInsertRequest(properties, key ?? Guid.NewGuid(), vectors);

        var response = await _collectionClient
            .Value.Data.InsertMany([request], cancellationToken)
            .ConfigureAwait(false);

        var errors = response.Errors.ToList();
        if (errors.Count > 0)
        {
            var messages = string.Join("; ", errors.Select(e => e.Message));
            throw new VectorStoreException($"Upsert failed for 1 of 1 object(s): {messages}");
        }
    }

    /// <inheritdoc />
    public override async Task UpsertAsync(
        IEnumerable<TRecord> records,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(records);

        var requests = records
            .Select(record =>
            {
                var (key, properties, vectors) = _mapper.MapToWeaviate(record);
                return new BatchInsertRequest(properties, key ?? Guid.NewGuid(), vectors);
            })
            .ToList();

        if (requests.Count == 0)
            return;

        var response = await _collectionClient
            .Value.Data.InsertMany(requests, cancellationToken)
            .ConfigureAwait(false);

        var errors = response.Errors.ToList();
        if (errors.Count > 0)
        {
            var messages = string.Join("; ", errors.Select(e => e.Message));
            throw new VectorStoreException(
                $"Upsert failed for {errors.Count} of {requests.Count} object(s): {messages}"
            );
        }
    }

    /// <inheritdoc />
    public override async Task DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var guid = ConvertToGuid(key);
        var collection = _collectionClient.Value;
        await collection.Data.DeleteByID(guid, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task DeleteAsync(
        IEnumerable<TKey> keys,
        CancellationToken cancellationToken = default
    )
    {
        var guids = keys.Select(ConvertToGuid).ToList();
        if (guids.Count == 0)
            return;

        var collection = _collectionClient.Value;
        await collection
            .Data.DeleteMany(Filter.UUID.ContainsAny(guids), cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<VectorSearchResult<TRecord>> SearchAsync<TInput>(
        TInput searchValue,
        int top,
        VectorSearchOptions<TRecord>? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        WarnIfOldFilterUsed(options);

        var floats = ConvertSearchInputToFloats(searchValue);
        var collection = _collectionClient.Value;
        var filter = WeaviateFilterTranslator.Translate(options?.Filter);
        var includeVectors = options?.IncludeVectors ?? false;
        var skip = options?.Skip ?? 0;
        var scoreThreshold = options?.ScoreThreshold;

        VectorSearchInput searchInput;
        var vectorPropName = ExtractPropertyName(options?.VectorProperty);
        if (vectorPropName == null)
        {
            // Auto-detect: if there's exactly one vector property, use its name
            var vectorNames = _mapper.GetVectorPropertyNames();
            if (vectorNames.Count == 1)
                vectorPropName = vectorNames[0];
        }

        if (vectorPropName != null)
        {
            searchInput = (vectorPropName, floats);
        }
        else
        {
            searchInput = floats;
        }

        var result = await collection
            .Query.NearVector(
                searchInput,
                filters: filter,
                distance: scoreThreshold.HasValue ? (float)scoreThreshold.Value : null,
                limit: (uint)top,
                offset: skip > 0 ? (uint)skip : null,
                returnMetadata: new MetadataQuery(MetadataOptions.Distance),
                includeVectors: includeVectors,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        foreach (var obj in result.Objects)
        {
            var record = _mapper.MapFromWeaviate(obj);
            var score = obj.Metadata.Distance;
            yield return new VectorSearchResult<TRecord>(record, score);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VectorSearchResult<TRecord>> HybridSearchAsync<TInput>(
        TInput searchValue,
        ICollection<string> keywords,
        int top,
        HybridSearchOptions<TRecord>? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var floats = ConvertSearchInputToFloats(searchValue);
        var collection = _collectionClient.Value;
        var filter = WeaviateFilterTranslator.Translate(options?.Filter);
        var includeVectors = options?.IncludeVectors ?? false;
        var skip = options?.Skip ?? 0;
        var query = string.Join(" ", keywords);

        // Build the vector input for hybrid search, targeting the correct named vector
        VectorSearchInput vectorSearchInput;
        var vectorPropName = ExtractPropertyName(options?.VectorProperty);
        if (vectorPropName == null)
        {
            var vectorNames = _mapper.GetVectorPropertyNames();
            if (vectorNames.Count == 1)
                vectorPropName = vectorNames[0];
        }

        if (vectorPropName != null)
        {
            vectorSearchInput = (vectorPropName, floats);
        }
        else
        {
            vectorSearchInput = floats;
        }

        var hybridVectors = HybridVectorInput.FromVectorSearch(vectorSearchInput);

        var result = await collection
            .Query.Hybrid(
                query,
                hybridVectors,
                filters: filter,
                limit: (uint)top,
                offset: skip > 0 ? (uint)skip : null,
                returnMetadata: new MetadataQuery(MetadataOptions.Distance),
                includeVectors: includeVectors,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        foreach (var obj in result.Objects)
        {
            var record = _mapper.MapFromWeaviate(obj);
            var score = obj.Metadata.Distance;
            yield return new VectorSearchResult<TRecord>(record, score);
        }
    }

    /// <inheritdoc />
    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey != null)
            return null;

        if (serviceType == typeof(VectorStoreCollectionMetadata))
        {
            return new VectorStoreCollectionMetadata
            {
                VectorStoreSystemName = "weaviate",
                CollectionName = _collectionName,
            };
        }

        return null;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // No unmanaged resources to dispose — the underlying WeaviateClient is owned by the caller.
    }

    private CollectionClient BuildCollectionClient()
    {
        var client = _client.Collections.Use(_collectionName);

        if (_options?.Tenant != null)
            client = client.WithTenant(_options.Tenant);

        if (_options?.ConsistencyLevel != null)
            client = client.WithConsistencyLevel(_options.ConsistencyLevel.Value);

        return client;
    }

    private static IWeaviateRecordMapper<TRecord> CreateMapper(
        VectorStoreCollectionDefinition? definition
    )
    {
        if (typeof(TRecord) == typeof(Dictionary<string, object?>))
        {
            var dataNames = new List<string>();
            var vectorNames = new List<string>();

            if (definition != null)
            {
                foreach (var prop in definition.Properties)
                {
                    if (prop is VectorStoreDataProperty dataProp)
                        dataNames.Add(
                            dataProp.StorageName ?? RecordPropertyModel.Decapitalize(dataProp.Name)
                        );
                    else if (prop is VectorStoreVectorProperty vectorProp)
                        vectorNames.Add(
                            vectorProp.StorageName
                                ?? RecordPropertyModel.Decapitalize(vectorProp.Name)
                        );
                }
            }

            return (IWeaviateRecordMapper<TRecord>)
                (object)new DynamicRecordMapper(dataNames, vectorNames);
        }

        return new AttributeBasedRecordMapper<TRecord>(definition);
    }

    private static void WarnIfOldFilterUsed(VectorSearchOptions<TRecord>? options)
    {
        if (options == null)
            return;

#pragma warning disable CS0618 // OldFilter is obsolete
        if (options.OldFilter != null && options.Filter == null)
        {
            throw new NotSupportedException(
                "VectorSearchOptions.OldFilter is deprecated and not supported by the Weaviate VectorData connector. "
                    + "Use VectorSearchOptions.Filter with a LINQ expression instead."
            );
        }
#pragma warning restore CS0618
    }

    private static Guid ConvertToGuid(TKey key)
    {
        return key switch
        {
            Guid g => g,
            string s => Guid.Parse(s),
            _ => throw new NotSupportedException(
                $"Key type '{typeof(TKey).Name}' is not supported. Use Guid or string."
            ),
        };
    }

    private static void ValidateKeyType()
    {
        if (typeof(TKey) != typeof(Guid) && typeof(TKey) != typeof(string))
        {
            throw new NotSupportedException(
                $"Key type '{typeof(TKey).Name}' is not supported by Weaviate. "
                    + "Use Guid or string."
            );
        }
    }

    private static string? ExtractPropertyName(Expression<Func<TRecord, object?>>? expression)
    {
        if (expression == null)
            return null;

        var body = expression.Body;
        if (body is UnaryExpression unary)
            body = unary.Operand;

        if (body is MemberExpression member)
            return Mapping.RecordPropertyModel.Decapitalize(member.Member.Name);

        return null;
    }

    private static float[] ConvertSearchInputToFloats<TInput>(TInput searchValue)
    {
        return searchValue switch
        {
            float[] arr => arr,
            ReadOnlyMemory<float> rom => rom.ToArray(),
            double[] darr => Array.ConvertAll(darr, d => (float)d),
            _ => throw new NotSupportedException(
                $"Search input type '{typeof(TInput).Name}' is not supported. "
                    + "Use float[], ReadOnlyMemory<float>, or double[]."
            ),
        };
    }
}
