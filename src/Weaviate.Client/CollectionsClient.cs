using System.IO;
using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client;

public record CollectionsClient
{
    private readonly WeaviateClient _client;

    internal CollectionsClient(WeaviateClient client)
    {
        _client = client;
    }

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

    private static void ValidateJsonAgainstType<T>(string json, string contextDescription)
        where T : class, new()
    {
        // First validate basic JSON syntax
        ValidateJsonSyntax(json, contextDescription);

        try
        {
            // Deserialize to Dto.Class to validate structure
            var dtoClass = JsonSerializer.Deserialize<Rest.Dto.Class>(
                json,
                Rest.WeaviateRestClient.RestJsonSerializerOptions
            );

            if (dtoClass == null)
            {
                throw new WeaviateClientException(
                    $"Invalid {contextDescription}: JSON deserialized to null."
                );
            }

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

    public async Task<CollectionClient> Create(
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

    public async Task<CollectionClient> Create(
        JsonDocument json,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(json);

        var jsonString = JsonSerializer.Serialize(
            json.RootElement,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        return await Create(jsonString, cancellationToken);
    }

    public async Task<CollectionClient> Create(
        Stream json,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(json);

        using var reader = new StreamReader(json, leaveOpen: true);
        var jsonString = await reader.ReadToEndAsync(cancellationToken);

        return await Create(jsonString, cancellationToken);
    }

    public async Task<CollectionClient> Create(
        Models.CollectionConfig collection,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(collection);

        var jsonString = JsonSerializer.Serialize(
            collection.ToDto(),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        return await Create(jsonString, cancellationToken);
    }

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

        var collectionClient = await Create(json, cancellationToken);

        return await collectionClient.AsTyped<T>(validate, cancellationToken);
    }

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

    public async Task<Typed.TypedCollectionClient<T>> Create<T>(
        Models.CollectionConfig collection,
        bool validateType = false,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(collection);

        var jsonString = JsonSerializer.Serialize(
            collection.ToDto(),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        return await Create<T>(jsonString, validateType, cancellationToken);
    }

    public async Task Delete(string collectionName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(collectionName);

        await _client.EnsureInitializedAsync();
        await _client.RestClient.CollectionDelete(collectionName, cancellationToken);
    }

    public async Task DeleteAll(CancellationToken cancellationToken = default)
    {
        var list = await List(cancellationToken).Select(l => l.Name).ToListAsync(cancellationToken);

        var tasks = list.Select(name => Delete(name, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task<bool> Exists(
        string collectionName,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        return await _client.RestClient.CollectionExists(collectionName, cancellationToken);
    }

    public async Task<CollectionConfig?> Export(
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

    public async IAsyncEnumerable<Models.CollectionConfig> List(
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

    public CollectionClient Use(string name)
    {
        return new CollectionClient(_client, name);
    }

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
    /// <returns>A TypedCollectionClient that provides strongly-typed operations.</returns>
    public async Task<Typed.TypedCollectionClient<T>> Use<T>(string name, bool validateType)
        where T : class, new()
    {
        return await Use(name).AsTyped<T>(validateType: validateType);
    }
}
