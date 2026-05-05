namespace Weaviate.Client;

/// <summary>
/// Factory that constructs an <see cref="ITokenService"/> from a <see cref="ClientConfiguration"/>.
/// Implement this interface to supply a custom authentication token provider.
/// </summary>
public interface ITokenServiceFactory
{
    /// <summary>
    /// Asynchronously creates an <see cref="ITokenService"/> for the given configuration,
    /// or <c>null</c> if the configuration does not require authentication.
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <returns>A task containing the token service</returns>
    Task<ITokenService?> CreateAsync(ClientConfiguration configuration);

    /// <summary>
    /// Synchronously creates an <see cref="ITokenService"/> for the given configuration,
    /// or <c>null</c> if the configuration does not require authentication.
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <returns>The token service</returns>
    ITokenService? CreateSync(ClientConfiguration configuration);
}
