namespace Weaviate.Client;

public static class Connect
{
    public static WeaviateClient Local(
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

    public static WeaviateClient Local(
        ICredentials? credentials = null,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null
    ) =>
        WeaviateClientBuilder.Local(
            credentials,
            hostname,
            restPort,
            grpcPort,
            useSsl,
            headers,
            httpMessageHandler
        );

    public static WeaviateClient Cloud(
        string restEndpoint,
        string? apiKey = null,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null
    ) => WeaviateClientBuilder.Cloud(restEndpoint, apiKey, headers, httpMessageHandler);

    public static WeaviateClient FromEnvironment(string prefix = "WEAVIATE_")
    {
        var restEndpoint = Environment.GetEnvironmentVariable($"{prefix}REST_ENDPOINT");
        var grpcEndpoint = Environment.GetEnvironmentVariable($"{prefix}GRPC_ENDPOINT");
        var restPort = Environment.GetEnvironmentVariable($"{prefix}REST_PORT") ?? "8080";
        var grpcPort = Environment.GetEnvironmentVariable($"{prefix}GRPC_PORT") ?? "50051";
        var useSsl = Environment.GetEnvironmentVariable($"{prefix}USE_SSL")?.ToLower() == "true";
        var apiKey = Environment.GetEnvironmentVariable($"{prefix}API_KEY");
        var openaiKey = Environment.GetEnvironmentVariable($"{prefix}OPENAI_API_KEY");

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

        var builder = WeaviateClientBuilder.Custom(
            restEndpoint: restEndpoint!,
            grpcEndpoint: grpcEndpoint!,
            restPort: restPort,
            grpcPort: grpcPort,
            useSsl: useSsl,
            credentials: string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey)
        );

        if (openaiKey is not null && !string.IsNullOrEmpty(openaiKey))
        {
            builder.WithOpenAI(openaiKey);
        }

        return builder.Build();
    }
}
