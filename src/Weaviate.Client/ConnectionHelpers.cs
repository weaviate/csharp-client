namespace Weaviate.Client;

public static class Connect
{
    public static WeaviateClient Local(
        Auth.ApiKeyCredentials apiKey, // ApiKeyCredentials is constructed implicitly from a string.
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        HttpMessageHandler? httpMessageHandler = null
    )
    {
        return Local(apiKey, hostname, restPort, grpcPort, useSsl, httpMessageHandler);
    }

    public static WeaviateClient Local(
        ICredentials? credentials = null,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        HttpMessageHandler? httpMessageHandler = null
    ) =>
        new ClientConfiguration(
            RestAddress: hostname,
            GrpcAddress: hostname,
            RestPort: restPort,
            GrpcPort: grpcPort,
            UseSsl: useSsl,
            Credentials: credentials
        ).Client(httpMessageHandler);

    public static WeaviateClient Cloud(
        string restEndpoint,
        string? apiKey = null,
        bool? addEmbeddingHeader = true,
        HttpMessageHandler? httpMessageHandler = null
    ) =>
        new ClientConfiguration(
            RestAddress: restEndpoint,
            GrpcAddress: $"grpc-{restEndpoint}",
            RestPort: 443,
            GrpcPort: 443,
            UseSsl: true,
            Credentials: string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey),
            AddEmbeddingHeader: addEmbeddingHeader ?? true
        ).Client(httpMessageHandler);

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
            restEndpoint: restEndpoint!,
            grpcEndpoint: grpcEndpoint!,
            restPort: restPort,
            grpcPort: grpcPort,
            useSsl: useSsl,
            credentials: string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey)
        );
    }

    public static WeaviateClient Custom(
        string restEndpoint = "localhost",
        string restPath = "v1/",
        string grpcEndpoint = "localhost",
        string grpcPath = "",
        string restPort = "8080",
        string grpcPort = "50051",
        bool useSsl = false,
        ICredentials? credentials = null,
        HttpMessageHandler? httpMessageHandler = null
    )
    {
        return new ClientConfiguration(
            restEndpoint,
            restPath,
            grpcEndpoint,
            grpcPath,
            Convert.ToUInt16(restPort),
            Convert.ToUInt16(grpcPort),
            useSsl,
            credentials
        ).Client(httpMessageHandler);
    }
}
