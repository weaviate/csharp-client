using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// A client for managing collections in Weaviate.
/// </summary>
public record CollectionsClient
{
    /// <summary>
    /// The client
    /// </summary>
    private readonly WeaviateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionsClient"/> class
    /// </summary>
    /// <param name="client">The client</param>
    internal CollectionsClient(WeaviateClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Validates the json syntax using the specified json
    /// </summary>
    /// <param name="json">The json</param>
    /// <param name="contextDescription">The context description</param>
    /// <exception cref="WeaviateClientException">Invalid {contextDescription}: JSON cannot be null or empty.</exception>
    /// <exception cref="WeaviateClientException">Invalid {contextDescription}: JSON must be a root object, not {document.RootElement.ValueKind}.</exception>
    /// <exception cref="WeaviateClientException">Invalid {contextDescription}: JSON syntax error. {ex.Message} </exception>
    private static void ValidateJsonSyntax(string json, string contextDescription)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new WeaviateClientException(
                $"Invalid {contextDescription}: JSON cannot be null or empty."
            );
        }

        try
        {
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new WeaviateClientException(
                    $"Invalid {contextDescription}: JSON must be a root object, not {document.RootElement.ValueKind}."
                );
            }
        }
        catch (JsonException ex)
        {
            throw new WeaviateClientException(
                $"Invalid {contextDescription}: JSON syntax error. {ex.Message}",
                ex
            );
        }
        catch (WeaviateClientException)
        {
            throw;
        }
    }

    /// <summary>
    /// Validates the json against type using the specified json
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="json">The json</param>
    /// <param name="contextDescription">The context description</param>
    /// <exception cref="WeaviateClientException">Invalid {contextDescription}: Failed to deserialize JSON. {ex.Message} </exception>
    /// <exception cref="WeaviateClientException">Invalid {contextDescription}: JSON deserialized to null.</exception>
    /// <exception cref="WeaviateClientException">Invalid {contextDescription}: Type {typeof(T).Name} is not compatible with the provided collection schema. {errorMessages}</exception>
    private static void ValidateJsonAgainstType<T>(string json, string contextDescription)
        where T : class, new()
    {
        // First validate basic JSON syntax
        ValidateJsonSyntax(json, contextDescription);

        try
        {
            // Deserialize to Dto.Class to validate structure
            var dtoClass =
                JsonSerializer.Deserialize<Rest.Dto.Class>(
                    json,
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                )
                ?? throw new WeaviateClientException(
                    $"Invalid {contextDescription}: JSON deserialized to null."
                );

            // Convert to CollectionConfig model
            var collectionConfig = dtoClass.ToModel();

            // Use TypeValidator to validate against type T
            var validator = Validation.TypeValidator.Default;
            var validationResult = validator.ValidateType<T>(collectionConfig);

            if (!validationResult.IsValid)
            {
                var errorMessages = string.Join(
                    "; ",
                    validationResult.Errors.Select(e => e.Message)
                );

                throw new WeaviateClientException(
                    $"Invalid {contextDescription}: Type {typeof(T).Name} is not compatible with the provided collection schema. {errorMessages}"
                );
            }
        }
        catch (JsonException ex)
        {
            throw new WeaviateClientException(
                $"Invalid {contextDescription}: Failed to deserialize JSON. {ex.Message}",
                ex
            );
        }
        catch (WeaviateClientException)
        {
            throw;
        }
    }

    /// <summary>
    /// Create a new collection from a json string.
    /// </summary>
    /// <param name="json">The json string defining the collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="CollectionClient"/> instance for the new collection.</returns>
    public async Task<CollectionClient> CreateFromJson(
        string json,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        ValidateJsonSyntax(json, "collection configuration");

        await _client.EnsureInitializedAsync();

        var response = await _client.RestClient.CollectionCreateRaw(json, cancellationToken);

        return new CollectionClient(_client, response.ToModel());
    }

    /// <summary>
    /// Create a new collection from a <see cref="JsonDocument"/>.
    /// </summary>
    /// <param name="json">The <see cref="JsonDocument"/> defining the collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="CollectionClient"/> instance for the new collection.</returns>
    public async Task<CollectionClient> CreateFromJson(
        JsonDocument json,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(json);

        var jsonString = JsonSerializer.Serialize(
            json.RootElement,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        return await CreateFromJson(jsonString, cancellationToken);
    }

    /// <summary>
    /// Create a new collection from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="json">The <see cref="Stream"/> containing the json that defines the collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="CollectionClient"/> instance for the new collection.</returns>
    public async Task<CollectionClient> CreateFromJson(
        Stream json,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(json);

        using var reader = new StreamReader(json, leaveOpen: true);
        var jsonString = await reader.ReadToEndAsync(cancellationToken);

        return await CreateFromJson(jsonString, cancellationToken);
    }

    /// <summary>
    /// Create a new collection.
    /// </summary>
    /// <param name="collection">The collection to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="CollectionClient"/> instance for the new collection.</returns>
    public async Task<CollectionClient> Create(
        CollectionCreateParams collection,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(collection);

        await EnsureTextAnalyzerFeaturesSupported(collection);

        var config = CollectionConfig.FromCollectionCreate(collection);

        var jsonString = JsonSerializer.Serialize(
            config.ToDto(),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        return await CreateFromJson(jsonString, cancellationToken);
    }

    private static readonly Version TextAnalyzerMinimumVersion = new(1, 37, 0);

    private async Task EnsureTextAnalyzerFeaturesSupported(CollectionCreateParams collection)
    {
        string? feature = DetectTextAnalyzerFeature(collection);
        if (feature is null)
            return;

        await _client.EnsureInitializedAsync();

        var serverVersion = _client.WeaviateVersion;
        if (serverVersion is null)
            return;

        if (serverVersion < TextAnalyzerMinimumVersion)
        {
            throw new WeaviateVersionMismatchException(
                feature,
                TextAnalyzerMinimumVersion,
                serverVersion
            );
        }
    }

    private static string? DetectTextAnalyzerFeature(CollectionCreateParams collection)
    {
        if (collection.InvertedIndexConfig?.StopwordPresets is { Count: > 0 })
            return "InvertedIndexConfig.StopwordPresets";

        foreach (var property in collection.Properties)
        {
            if (PropertyUsesTextAnalyzer(property))
                return "Property.TextAnalyzer";
        }

        return null;
    }

    private static bool PropertyUsesTextAnalyzer(Property property)
    {
        if (property.TextAnalyzer is not null)
            return true;
        if (property.NestedProperties is { } nested)
        {
            foreach (var np in nested)
            {
                if (PropertyUsesTextAnalyzer(np))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Create a new typed collection from a json string.
    /// </summary>
    /// <typeparam name="T">The type of the objects in this collection.</typeparam>
    /// <param name="json">The json string defining the collection.</param>
    /// <param name="validate">Whether to validate the collection against the type `T` after creation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Typed.TypedCollectionClient{T}"/> instance for the new collection.</returns>
    public async Task<Typed.TypedCollectionClient<T>> Create<T>(
        string json,
        bool validate = false,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (validate)
        {
            ValidateJsonAgainstType<T>(json, "collection configuration");
        }

        var collectionClient = await CreateFromJson(json, cancellationToken);

        return await collectionClient.AsTyped<T>(validate, cancellationToken);
    }

    /// <summary>
    /// Create a new typed collection from a <see cref="JsonDocument"/>.
    /// </summary>
    /// <typeparam name="T">The type of the objects in this collection.</typeparam>
    /// <param name="json">The <see cref="JsonDocument"/> defining the collection.</param>
    /// <param name="validate">Whether to validate the collection against the type `T` after creation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Typed.TypedCollectionClient{T}"/> instance for the new collection.</returns>
    public async Task<Typed.TypedCollectionClient<T>> Create<T>(
        JsonDocument json,
        bool validate = false,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(json);

        var jsonString = JsonSerializer.Serialize(
            json.RootElement,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        return await Create<T>(jsonString, validate, cancellationToken);
    }

    /// <summary>
    /// Create a new typed collection from a <see cref="Stream"/>.
    /// </summary>
    /// <typeparam name="T">The type of the objects in this collection.</typeparam>
    /// <param name="json">The <see cref="Stream"/> containing the json that defines the collection.</param>
    /// <param name="validate">Whether to validate the collection against the type `T` after creation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Typed.TypedCollectionClient{T}"/> instance for the new collection.</returns>
    public async Task<Typed.TypedCollectionClient<T>> Create<T>(
        Stream json,
        bool validate = false,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(json);

        using var reader = new StreamReader(json, leaveOpen: true);
        var jsonString = await reader.ReadToEndAsync(cancellationToken);

        return await Create<T>(jsonString, validate, cancellationToken);
    }

    /// <summary>
    /// Create a new typed collection.
    /// </summary>
    /// <typeparam name="T">The type of the objects in this collection.</typeparam>
    /// <param name="collection">The collection to create.</param>
    /// <param name="validateType">Whether to validate the collection against the type `T` after creation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Typed.TypedCollectionClient{T}"/> instance for the new collection.</returns>
    public async Task<Typed.TypedCollectionClient<T>> Create<T>(
        CollectionCreateParams collection,
        bool validateType = false,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(collection);

        var config = CollectionConfig.FromCollectionCreate(collection);

        var jsonString = JsonSerializer.Serialize(
            config.ToDto(),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        return await Create<T>(jsonString, validateType, cancellationToken);
    }

    /// <summary>
    /// Delete a collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task Delete(string collectionName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(collectionName);

        await _client.EnsureInitializedAsync();
        await _client.RestClient.CollectionDelete(collectionName, cancellationToken);
    }

    /// <summary>
    /// Delete all collections.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task DeleteAll(CancellationToken cancellationToken = default)
    {
        var list = await List(cancellationToken).Select(l => l.Name).ToListAsync(cancellationToken);

        var tasks = list.Select(name => Delete(name, cancellationToken));

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Check if a collection exists.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the collection exists, false otherwise.</returns>
    public async Task<bool> Exists(
        string collectionName,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        return await _client.RestClient.CollectionExists(collectionName, cancellationToken);
    }

    /// <summary>
    /// Export the configuration of a collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection configuration.</returns>
    public async Task<CollectionConfigExport?> Export(
        string collectionName,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var response = await _client.RestClient.CollectionGet(collectionName, cancellationToken);

        if (response is null)
        {
            return null;
        }

        return response.ToModel();
    }

    /// <summary>
    /// List all collections.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of all collections.</returns>
    public async IAsyncEnumerable<CollectionConfig> List(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var response = await _client.RestClient.CollectionList(cancellationToken);

        foreach (var c in response?.Classes ?? Enumerable.Empty<Rest.Dto.Class>())
        {
            yield return c.ToModel();
        }
    }

    /// <summary>
    /// Get a client for a specific collection.
    /// </summary>
    /// <param name="name">The name of the collection.</param>
    /// <returns>A <see cref="CollectionClient"/> instance for the collection.</returns>
    public CollectionClient Use(string name)
    {
        return new CollectionClient(_client, name);
    }

    /// <summary>
    /// Get a client for a specific collection.
    /// </summary>
    /// <param name="name">The name of the collection.</param>
    /// <returns>A <see cref="CollectionClient"/> instance for the collection.</returns>
    public CollectionClient this[string name] => Use(name);

    /// <summary>
    /// Creates a strongly-typed collection client for accessing a specific collection.
    /// The collection client provides type-safe data and query operations.
    /// </summary>
    /// <typeparam name="T">The C# type representing objects in this collection.</typeparam>
    /// <param name="name">The name of the collection.</param>
    /// <returns>A TypedCollectionClient that provides strongly-typed operations.</returns>
    public Typed.TypedCollectionClient<T> Use<T>(string name)
        where T : class, new()
    {
        return Use(name).AsTyped<T>();
    }

    /// <summary>
    /// Creates a strongly-typed collection client for accessing a specific collection.
    /// The collection client provides type-safe data and query operations.
    /// </summary>
    /// <typeparam name="T">The C# type representing objects in this collection.</typeparam>
    /// <param name="name">The name of the collection.</param>
    /// <param name="validateType">Whether to validate the collection against the type `T` after creation.</param>
    /// <returns>A TypedCollectionClient that provides strongly-typed operations.</returns>
    public async Task<Typed.TypedCollectionClient<T>> Use<T>(string name, bool validateType)
        where T : class, new()
    {
        return await Use(name).AsTyped<T>(validateType: validateType);
    }
}
