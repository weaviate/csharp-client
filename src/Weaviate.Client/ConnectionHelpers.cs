namespace Weaviate.Client;

public static class Connect
{
    public static WeaviateClientBuilder Local(
        Auth.ApiKeyCredentials credentials, // ApiKeyCredentials is constructed implicitly from a string.
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null
    )
    {
        return Local(
            credentials,
            hostname,
            restPort,
            grpcPort,
            useSsl,
            headers,
            httpMessageHandler
        );
    }

    public static WeaviateClientBuilder Local(
        ICredentials? credentials = null,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null
    ) =>
        new WeaviateClientBuilder()
            .WithRestEndpoint(hostname)
            .WithGrpcEndpoint(hostname)
            .WithRestPort(restPort)
            .WithGrpcPort(grpcPort)
            .UseSsl(useSsl)
            .WithCredentials(credentials ?? null)
            .WithHttpMessageHandler(httpMessageHandler)
            .WithHeaders(headers);

    public static WeaviateClientBuilder Cloud(
        string restEndpoint,
        string? apiKey = null,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null
    ) =>
        new WeaviateClientBuilder()
            .WithRestEndpoint(restEndpoint)
            .WithGrpcEndpoint($"grpc-{restEndpoint}")
            .WithRestPort(443)
            .WithGrpcPort(443)
            .UseSsl(true)
            .WithCredentials(string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey))
            .WithHeaders(headers)
            .WithHttpMessageHandler(httpMessageHandler);

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

    public static WeaviateClientBuilder Custom(
        string restEndpoint = "localhost",
        string restPath = "v1/",
        string grpcEndpoint = "localhost",
        string grpcPath = "",
        string restPort = "8080",
        string grpcPort = "50051",
        bool useSsl = false,
        Dictionary<string, string>? headers = null,
        ICredentials? credentials = null,
        HttpMessageHandler? httpMessageHandler = null
    )
    {
        return new WeaviateClientBuilder()
            .WithRestEndpoint(restEndpoint)
            .WithRestPath(restPath)
            .WithGrpcEndpoint(grpcEndpoint)
            .WithGrpcPath(grpcPath)
            .WithRestPort(Convert.ToUInt16(restPort))
            .WithGrpcPort(Convert.ToUInt16(grpcPort))
            .UseSsl(useSsl)
            .WithHeaders(headers)
            .WithCredentials(credentials)
            .WithHttpMessageHandler(httpMessageHandler);
    }
}
