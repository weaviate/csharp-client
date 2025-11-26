namespace Weaviate.Client;

public interface ITokenServiceFactory
{
    Task<ITokenService?> CreateAsync(ClientConfiguration configuration);
    ITokenService? CreateSync(ClientConfiguration configuration);
}
