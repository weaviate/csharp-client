using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;

namespace Weaviate.Client.VectorData.DependencyInjection;

/// <summary>
/// Extension methods for registering Weaviate VectorData services with dependency injection.
/// </summary>
public static class WeaviateVectorDataServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="WeaviateVectorStore"/> as the <see cref="VectorStore"/> implementation.
    /// Requires <see cref="WeaviateClient"/> to already be registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure vector store options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateVectorStore(
        this IServiceCollection services,
        Action<WeaviateVectorStoreOptions>? configure = null
    )
    {
        services.AddSingleton<WeaviateVectorStore>(sp =>
        {
            var client = sp.GetRequiredService<WeaviateClient>();
            WeaviateVectorStoreOptions? options = null;
            if (configure != null)
            {
                options = new WeaviateVectorStoreOptions();
                configure(options);
            }
            return new WeaviateVectorStore(client, options);
        });

        services.AddSingleton<VectorStore>(sp => sp.GetRequiredService<WeaviateVectorStore>());

        return services;
    }

    /// <summary>
    /// Registers a specific <see cref="WeaviateVectorStoreCollection{TKey, TRecord}"/>
    /// as <see cref="VectorStoreCollection{TKey, TRecord}"/>.
    /// Requires <see cref="WeaviateClient"/> to already be registered.
    /// </summary>
    /// <typeparam name="TKey">The key type. Must be <see cref="Guid"/> or <see cref="string"/>.</typeparam>
    /// <typeparam name="TRecord">The record type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="collectionName">The Weaviate collection name.</param>
    /// <param name="configure">Optional action to configure collection options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateVectorStoreCollection<TKey, TRecord>(
        this IServiceCollection services,
        string collectionName,
        Action<WeaviateVectorStoreCollectionOptions>? configure = null
    )
        where TKey : notnull
        where TRecord : class
    {
        services.AddSingleton<WeaviateVectorStoreCollection<TKey, TRecord>>(sp =>
        {
            var client = sp.GetRequiredService<WeaviateClient>();
            WeaviateVectorStoreCollectionOptions? options = null;
            if (configure != null)
            {
                options = new WeaviateVectorStoreCollectionOptions();
                configure(options);
            }
            return new WeaviateVectorStoreCollection<TKey, TRecord>(
                client,
                collectionName,
                options: options
            );
        });

        services.AddSingleton<VectorStoreCollection<TKey, TRecord>>(sp =>
            sp.GetRequiredService<WeaviateVectorStoreCollection<TKey, TRecord>>()
        );

        return services;
    }
}
