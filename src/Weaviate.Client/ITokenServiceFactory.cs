namespace Weaviate.Client;

/// <summary>
/// The token service factory interface
/// </summary>
public interface ITokenServiceFactory
{
    /// <summary>
    /// Creates the configuration
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <returns>A task containing the token service</returns>
    Task<ITokenService?> CreateAsync(ClientConfiguration configuration);

    /// <summary>
    /// Creates the sync using the specified configuration
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <returns>The token service</returns>
    ITokenService? CreateSync(ClientConfiguration configuration);
}
