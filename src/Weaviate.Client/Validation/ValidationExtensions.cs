using Weaviate.Client.Cache;
using Weaviate.Client.Validation;

namespace Weaviate.Client;

/// <summary>
/// Extension methods for type validation operations.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates a C# type against a collection schema and throws if validation fails.
    /// </summary>
    /// <typeparam name="T">The C# type to validate.</typeparam>
    /// <param name="schema">The collection schema to validate against.</param>
    /// <param name="collectionName">The collection name (for error messages).</param>
    /// <param name="typeValidator">Optional type validator instance. If null, uses TypeValidator.Default.</param>
    /// <exception cref="InvalidOperationException">Thrown if validation fails with errors.</exception>
    public static void ValidateTypeOrThrow<T>(
        this Models.CollectionConfig? schema,
        TypeValidator? typeValidator = null
    )
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(schema);

        var validator = typeValidator ?? TypeValidator.Default;
        var validationResult = validator.ValidateType<T>(schema);

        if (!validationResult.IsValid)
        {
            var errorMessages = string.Join(", ", validationResult.Errors.Select(e => e.Message));
            throw new InvalidOperationException(
                $"Type {typeof(T).Name} is not compatible with collection '{schema.Name}': {errorMessages}"
            );
        }
    }

    /// <summary>
    /// Validates a C# type against a collection schema.
    /// </summary>
    /// <typeparam name="T">The C# type to validate.</typeparam>
    /// <param name="schema">The collection schema to validate against.</param>
    /// <param name="typeValidator">Optional type validator instance. If null, uses TypeValidator.Default.</param>
    /// <returns>A validation result containing any errors and warnings.</returns>
    public static ValidationResult ValidateType<T>(
        this Models.CollectionConfig? schema,
        TypeValidator? typeValidator = null
    )
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(schema);

        var validator = typeValidator ?? TypeValidator.Default;
        return validator.ValidateType<T>(schema);
    }

    /// <summary>
    /// Validates a C# type against a collection schema.
    /// </summary>
    /// <typeparam name="T">The C# type to validate.</typeparam>
    /// <param name="typeValidator">Optional type validator instance. If null, uses TypeValidator.Default.</param>
    /// <param name="schemaCache">Optional schema cache instance. If null, uses SchemaCache.Default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A validation result containing any errors and warnings.</returns>
    public static async Task<ValidationResult> ValidateType<T>(
        this Typed.TypedCollectionClient<T> client,
        TypeValidator? typeValidator = null,
        SchemaCache? schemaCache = null,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        var schema = await client.Config.GetCachedConfig(schemaCache, cancellationToken);

        return schema.ValidateType<T>(typeValidator);
    }

    /// <summary>
    /// Validates a C# type against a collection schema and throws if validation fails.
    /// </summary>
    /// <typeparam name="T">The C# type to validate.</typeparam>
    /// <param name="schema">The collection schema to validate against.</param>
    /// <param name="collectionName">The collection name (for error messages).</param>
    /// <param name="typeValidator">Optional type validator instance. If null, uses TypeValidator.Default.</param>
    /// <exception cref="InvalidOperationException">Thrown if validation fails with errors.</exception>
    public static async Task<Typed.TypedCollectionClient<T>> ValidateTypeOrThrow<T>(
        this Typed.TypedCollectionClient<T> client,
        TypeValidator? typeValidator = null,
        SchemaCache? schemaCache = null,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        var schema = await client.Config.GetCachedConfig(schemaCache, cancellationToken);

        schema.ValidateTypeOrThrow<T>(typeValidator);

        return client;
    }
}
