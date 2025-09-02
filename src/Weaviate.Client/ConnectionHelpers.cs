namespace Weaviate.Client;

public static class Connect
{
    public static ClientConfiguration LocalConfig(
        string hostname,
        ushort restPort,
        ushort grpcPort,
        bool useSsl,
        ICredentials? credentials
    ) => new(hostname, hostname, restPort, grpcPort, useSsl, credentials);

    public static WeaviateClient Local(
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        ICredentials? credentials = null,
        HttpMessageHandler? httpMessageHandler = null
    ) =>
        LocalConfig(hostname, restPort, grpcPort, useSsl, credentials: credentials)
            .Client(httpMessageHandler);

    public static ClientConfiguration CloudConfig(
        string restEndpoint,
        string? apiKey = null,
        bool? addEmbeddingHeader = true
    ) =>
        new ClientConfiguration(
            restEndpoint,
            $"grpc-{restEndpoint}",
            443,
            443,
            true,
            string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey),
            addEmbeddingHeader ?? true
        );

    public static WeaviateClient Cloud(
        string restEndpoint,
        string? apiKey = null,
        bool? addEmbeddingHeader = true,
        HttpMessageHandler? httpMessageHandler = null
    ) => CloudConfig(restEndpoint, apiKey, addEmbeddingHeader).Client(httpMessageHandler);

    public static WeaviateClient FromEnvironment(string prefix = "WEAVIATE_")
    {
        var restEndpoint = Environment.GetEnvironmentVariable($"{prefix}REST_ENDPOINT");
        var grpcEndpoint = Environment.GetEnvironmentVariable($"{prefix}GRPC_ENDPOINT");
        var restPort = Environment.GetEnvironmentVariable($"{prefix}REST_PORT") ?? "8080";
        var grpcPort = Environment.GetEnvironmentVariable($"{prefix}GRPC_PORT") ?? "50051";
        var useSsl = Environment.GetEnvironmentVariable($"{prefix}USE_SSL")?.ToLower() == "true";
        var apiKey = Environment.GetEnvironmentVariable($"{prefix}API_KEY");

        if (restEndpoint is null && grpcEndpoint is null)
        {
            throw new InvalidOperationException("No REST or GRPC endpoint provided.");
        }
        else if (restEndpoint is not null && grpcEndpoint is null)
        {
            grpcEndpoint = restEndpoint;
        }
        else if (restEndpoint is null && grpcEndpoint is not null)
        {
            restEndpoint = grpcEndpoint;
        }

        return Custom(
            restEndpoint!,
            grpcEndpoint!,
            restPort,
            grpcPort,
            useSsl,
            string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey)
        );
    }

    public static WeaviateClient Custom(
        string restEndpoint,
        string grpcEndpoint,
        string restPort,
        string grpcPort,
        bool useSsl,
        ICredentials? credentials
    )
    {
        return new(
            new ClientConfiguration(
                restEndpoint,
                grpcEndpoint,
                Convert.ToUInt16(restPort),
                Convert.ToUInt16(grpcPort),
                useSsl,
                credentials
            )
        );
    }
}
