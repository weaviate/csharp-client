using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace Weaviate.Client.Models;

/// <summary>
/// The inverted index config update
/// </summary>
public record InvertedIndexConfigUpdate(InvertedIndexConfig WrappedConfig)
{
    /// <summary>
    /// Gets the value of the bm 25
    /// </summary>
    public Bm25ConfigUpdate Bm25 => new(WrappedConfig.Bm25 ??= BM25Config.Default);

    /// <summary>
    /// Gets or sets the value of the cleanup interval seconds
    /// </summary>
    public int CleanupIntervalSeconds
    {
        get => WrappedConfig.CleanupIntervalSeconds;
        set => WrappedConfig.CleanupIntervalSeconds = value;
    }

    /// <summary>
    /// Gets the value of the stopwords
    /// </summary>
    public StopwordsConfigUpdate Stopwords =>
        new(WrappedConfig.Stopwords ??= StopwordConfig.Default);
}

/// <summary>
/// The bm 25 config update
/// </summary>
public record Bm25ConfigUpdate(BM25Config WrappedBm25)
{
    /// <summary>
    /// Gets or sets the value of the b
    /// </summary>
    public float? B
    {
        get => Convert.ToSingle(WrappedBm25.B);
        set => WrappedBm25.B = value ?? BM25Config.Default.B;
    }

    /// <summary>
    /// Gets or sets the value of the k 1
    /// </summary>
    public float? K1
    {
        get => Convert.ToSingle(WrappedBm25.K1);
        set => WrappedBm25.K1 = value ?? BM25Config.Default.K1;
    }
}

/// <summary>
/// The stopwords config update
/// </summary>
public record StopwordsConfigUpdate(StopwordConfig WrappedStopwords)
{
    /// <summary>
    /// Gets or sets the value of the additions
    /// </summary>
    public ImmutableList<string> Additions
    {
        get => WrappedStopwords.Additions;
        set => WrappedStopwords.Additions = value ?? [];
    }

    /// <summary>
    /// Gets or sets the value of the preset
    /// </summary>
    public StopwordConfig.Presets Preset
    {
        get => WrappedStopwords.Preset;
        set => WrappedStopwords.Preset = value;
    }

    /// <summary>
    /// Gets or sets the value of the removals
    /// </summary>
    public ImmutableList<string> Removals
    {
        get => WrappedStopwords.Removals;
        set => WrappedStopwords.Removals = value ?? [];
    }
}

/// <summary>
/// The property update
/// </summary>
public partial record PropertyUpdate(Property WrappedProperty)
{
    /// <summary>
    /// Gets the value of the name
    /// </summary>
    public string Name => WrappedProperty.Name;

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string? Description
    {
        get => WrappedProperty.Description;
        set => WrappedProperty.Description = value;
    }
}

/// <summary>
/// The reference update
/// </summary>
public partial record ReferenceUpdate(Reference WrappedReference)
{
    /// <summary>
    /// Gets the value of the name
    /// </summary>
    public string Name => WrappedReference.Name;

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string? Description
    {
        get => WrappedReference.Description;
        set => WrappedReference.Description = value;
    }
}

/// <summary>
/// The collection update
/// </summary>
public partial record CollectionUpdate(CollectionConfig WrappedCollection)
{
    /// <summary>
    /// Gets the value of the name
    /// </summary>
    public string Name => WrappedCollection.Name;

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string Description
    {
        get => WrappedCollection.Description;
        set => WrappedCollection.Description = value;
    }

    // Define properties of the collection.
    /// <summary>
    /// Gets the value of the properties
    /// </summary>
    public ReadOnlyDictionary<string, PropertyUpdate> Properties { get; } =
        WrappedCollection
            .Properties.ToDictionary(p => p.Name, p => new PropertyUpdate(p))
            .AsReadOnly();

    /// <summary>
    /// Gets the value of the references
    /// </summary>
    public ReadOnlyCollection<ReferenceUpdate> References { get; } =
        WrappedCollection.References.Select(r => new ReferenceUpdate(r)).ToList().AsReadOnly();

    // inverted index config
    /// <summary>
    /// Gets the value of the inverted index config
    /// </summary>
    public InvertedIndexConfigUpdate InvertedIndexConfig
    {
        get => new(WrappedCollection.InvertedIndexConfig ??= Models.InvertedIndexConfig.Default);
    }

    /// <summary>
    /// Gets or sets the value of the reranker config
    /// </summary>
    public IRerankerConfig? RerankerConfig
    {
        get => WrappedCollection.RerankerConfig;
        set => WrappedCollection.RerankerConfig = value;
    }

    /// <summary>
    /// Gets or sets the value of the generative config
    /// </summary>
    public IGenerativeConfig? GenerativeConfig
    {
        get => WrappedCollection.GenerativeConfig;
        set => WrappedCollection.GenerativeConfig = value;
    }

    // multi tenancy config
    /// <summary>
    /// Gets the value of the multi tenancy config
    /// </summary>
    public MultiTenancyConfigUpdate MultiTenancyConfig =>
        new(WrappedCollection.MultiTenancyConfig ?? Models.MultiTenancyConfig.Default);

    // replication config
    /// <summary>
    /// The default
    /// </summary>
    public ReplicationConfigUpdate ReplicationConfig = new(
        WrappedCollection.ReplicationConfig ?? Models.ReplicationConfig.Default
    );

    // Manage how the index should be sharded and distributed in the cluster
    /// <summary>
    /// Gets the value of the sharding config
    /// </summary>
    public ShardingConfig? ShardingConfig
    {
        get => WrappedCollection.ShardingConfig;
    }

    // Configure named vectors.
    /// <summary>
    /// Gets the value of the vector config
    /// </summary>
    public ReadOnlyDictionary<string, VectorConfigUpdate> VectorConfig { get; } =
        WrappedCollection
            .VectorConfig.Select(vc => new VectorConfigUpdate(WrappedCollection, vc.Value))
            .ToDictionary(vc => vc.Name, vc => vc)
            .AsReadOnly();

    /// <summary>
    /// Returns the string
    /// </summary>
    /// <returns>The string</returns>
    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(
            WrappedCollection.ToDto(),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
    }

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>The int</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Description);
        hash.Add(Properties);
        hash.Add(References);
        hash.Add(InvertedIndexConfig);
        hash.Add(MultiTenancyConfig);
        hash.Add(ReplicationConfig);
        hash.Add(ShardingConfig);
        hash.Add(VectorConfig);
        return hash.ToHashCode();
    }
}

/// <summary>
/// The vector config update class
/// </summary>
public class VectorConfigUpdate(
    CollectionConfig WrappedCollection,
    VectorConfig WrappedVectorConfig
)
{
    /// <summary>
    /// Gets or sets the value of the name
    /// </summary>
    public string Name
    {
        get => WrappedVectorConfig.Name;
        set
        {
            var vc = WrappedCollection.VectorConfig[WrappedVectorConfig.Name];
            WrappedCollection.VectorConfig.Remove(vc.Name);
            WrappedVectorConfig.Name = value;
            WrappedCollection.VectorConfig.Add(WrappedVectorConfig);
        }
    }

    /// <summary>
    /// Gets the value of the vector index config
    /// </summary>
    public VectorIndexConfigUpdate VectorIndexConfig
    {
        get => new(WrappedVectorConfig.VectorIndexConfig);
    }
}

/// <summary>
/// The vector index config update class
/// </summary>
public class VectorIndexConfigUpdate(VectorIndexConfig? WrappedVectorIndexConfig)
{
    /// <summary>
    /// Updates the hnsw using the specified c
    /// </summary>
    /// <param name="c">The </param>
    /// <exception cref="InvalidOperationException">VectorIndexConfig is not of type HNSW.</exception>
    public void UpdateHNSW(Action<VectorIndexConfigUpdateHNSW> c)
    {
        VectorIndex.HNSW hnsw =
            WrappedVectorIndexConfig as VectorIndex.HNSW
            ?? throw new InvalidOperationException("VectorIndexConfig is not of type HNSW.");

        c(new(hnsw));
    }

    /// <summary>
    /// Updates the flat using the specified c
    /// </summary>
    /// <param name="c">The </param>
    /// <exception cref="InvalidOperationException">VectorIndexConfig is not of type Flat.</exception>
    public void UpdateFlat(Action<VectorIndexConfigUpdateFlat> c)
    {
        VectorIndex.Flat flat =
            WrappedVectorIndexConfig as VectorIndex.Flat
            ?? throw new InvalidOperationException("VectorIndexConfig is not of type Flat.");

        c(new(flat));
    }

    /// <summary>
    /// Updates the dynamic using the specified c
    /// </summary>
    /// <param name="c">The </param>
    /// <exception cref="InvalidOperationException">VectorIndexConfig is not of type Dynamic.</exception>
    public void UpdateDynamic(Action<VectorIndexConfigUpdateDynamic> c)
    {
        VectorIndex.Dynamic dynamic =
            WrappedVectorIndexConfig as VectorIndex.Dynamic
            ?? throw new InvalidOperationException("VectorIndexConfig is not of type Dynamic.");

        c(new(dynamic));
    }
}

/// <summary>
/// The vector index config update hnsw class
/// </summary>
public class VectorIndexConfigUpdateHNSW(VectorIndex.HNSW WrappedHNSW)
{
    /// <summary>
    /// Gets or sets the value of the dynamic ef factor
    /// </summary>
    public int? DynamicEfFactor
    {
        get => WrappedHNSW.DynamicEfFactor;
        set => WrappedHNSW.DynamicEfFactor = value;
    }

    /// <summary>
    /// Gets or sets the value of the dynamic ef max
    /// </summary>
    public int? DynamicEfMax
    {
        get => WrappedHNSW.DynamicEfMax;
        set => WrappedHNSW.DynamicEfMax = value;
    }

    /// <summary>
    /// Gets or sets the value of the dynamic ef min
    /// </summary>
    public int? DynamicEfMin
    {
        get => WrappedHNSW.DynamicEfMin;
        set => WrappedHNSW.DynamicEfMin = value;
    }

    /// <summary>
    /// Gets or sets the value of the ef
    /// </summary>
    public int? Ef
    {
        get => WrappedHNSW.Ef;
        set => WrappedHNSW.Ef = value;
    }

    /// <summary>
    /// Gets or sets the value of the filter strategy
    /// </summary>
    public VectorIndexConfig.VectorIndexFilterStrategy? FilterStrategy
    {
        get => WrappedHNSW.FilterStrategy;
        set => WrappedHNSW.FilterStrategy = value;
    }

    /// <summary>
    /// Gets or sets the value of the flat search cutoff
    /// </summary>
    public int? FlatSearchCutoff
    {
        get => WrappedHNSW.FlatSearchCutoff;
        set => WrappedHNSW.FlatSearchCutoff = value;
    }

    /// <summary>
    /// Gets or sets the value of the quantizer
    /// </summary>
    public VectorIndexConfig.QuantizerConfigBase? Quantizer
    {
        get => WrappedHNSW.Quantizer;
        set => WrappedHNSW.Quantizer = value;
    }

    /// <summary>
    /// Gets or sets the value of the vector cache max objects
    /// </summary>
    public long? VectorCacheMaxObjects
    {
        get => WrappedHNSW.VectorCacheMaxObjects;
        set => WrappedHNSW.VectorCacheMaxObjects = value;
    }
}

/// <summary>
/// The vector index config update dynamic class
/// </summary>
public class VectorIndexConfigUpdateDynamic(VectorIndex.Dynamic WrappedDynamic)
{
    /// <summary>
    /// Gets or sets the value of the distance
    /// </summary>
    public VectorIndexConfig.VectorDistance? Distance
    {
        get => WrappedDynamic.Distance;
        set => WrappedDynamic.Distance = value;
    }

    /// <summary>
    /// Gets or sets the value of the threshold
    /// </summary>
    public int? Threshold
    {
        get => WrappedDynamic.Threshold;
        set => WrappedDynamic.Threshold = value;
    }

    /// <summary>
    /// Gets the value of the hnsw
    /// </summary>
    public VectorIndexConfigUpdateHNSW? Hnsw
    {
        get => new(WrappedDynamic.Hnsw ?? new VectorIndex.HNSW());
    }

    /// <summary>
    /// Gets the value of the flat
    /// </summary>
    public VectorIndexConfigUpdateFlat? Flat
    {
        get => new(WrappedDynamic.Flat ?? new VectorIndex.Flat());
    }
}

/// <summary>
/// The vector index config update flat class
/// </summary>
public class VectorIndexConfigUpdateFlat(VectorIndex.Flat WrappedFlat)
{
    /// <summary>
    /// Gets or sets the value of the quantizer
    /// </summary>
    public VectorIndexConfig.QuantizerConfigFlat? Quantizer
    {
        get => (VectorIndexConfig.QuantizerConfigFlat?)WrappedFlat.Quantizer;
        set => WrappedFlat.Quantizer = value;
    }

    /// <summary>
    /// Gets or sets the value of the vector cache max objects
    /// </summary>
    public long? VectorCacheMaxObjects
    {
        get => WrappedFlat.VectorCacheMaxObjects;
        set => WrappedFlat.VectorCacheMaxObjects = value;
    }
}

/// <summary>
/// The replication config update class
/// </summary>
public class ReplicationConfigUpdate(ReplicationConfig WrappedReplicationConfig)
{
    /// <summary>
    /// Gets or sets the value of the async enabled
    /// </summary>
    public bool AsyncEnabled
    {
        get => WrappedReplicationConfig.AsyncEnabled;
        set => WrappedReplicationConfig.AsyncEnabled = value;
    }

    /// <summary>
    /// Gets or sets the value of the deletion strategy
    /// </summary>
    public DeletionStrategy? DeletionStrategy
    {
        get => WrappedReplicationConfig.DeletionStrategy;
        set => WrappedReplicationConfig.DeletionStrategy = value;
    }

    /// <summary>
    /// Gets or sets the value of the factor
    /// </summary>
    public int Factor
    {
        get => WrappedReplicationConfig.Factor;
        set => WrappedReplicationConfig.Factor = value;
    }
}

/// <summary>
/// The multi tenancy config update class
/// </summary>
public class MultiTenancyConfigUpdate(MultiTenancyConfig WrappedMultiTenancyConfig)
{
    /// <summary>
    /// Gets or sets the value of the auto tenant creation
    /// </summary>
    public bool AutoTenantCreation
    {
        get => WrappedMultiTenancyConfig.AutoTenantCreation;
        set => WrappedMultiTenancyConfig.AutoTenantCreation = value;
    }

    /// <summary>
    /// Gets or sets the value of the auto tenant activation
    /// </summary>
    public bool AutoTenantActivation
    {
        get => WrappedMultiTenancyConfig.AutoTenantActivation;
        set => WrappedMultiTenancyConfig.AutoTenantActivation = value;
    }
}
