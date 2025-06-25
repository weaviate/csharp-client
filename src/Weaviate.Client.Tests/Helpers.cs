using System.Text.Json;

namespace Weaviate.Client.Tests;

public class LoggingHandler : DelegatingHandler
{
    private readonly Action<string> _log;

    public LoggingHandler(Action<string> log)
    {
        _log = log;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        _log($"Request: {request.Method} {request.RequestUri}");

        if (request.Content != null)
        {
            var requestContent = await request.Content.ReadAsStringAsync(CancellationToken.None);
            _log($"Request Content: {requestContent}");

            // Buffer the content so it can be read again.
            request.Content = new StringContent(
                requestContent,
                System.Text.Encoding.UTF8,
                "application/json"
            );
        }

        foreach (var header in request.Headers)
        {
            _log($"Request Header: {header.Key}: {string.Join(", ", header.Value)}");
        }

        var response = await base.SendAsync(request, cancellationToken);

        _log($"Response: {response.StatusCode}");

        foreach (var header in response.Headers)
        {
            _log($"Response Header: {header.Key}: {string.Join(", ", header.Value)}");
        }

        if (response.Content != null)
        {
            string responseContent = "";

            var responseContentJson = await response.Content.ReadAsStringAsync(
                CancellationToken.None
            );

            if (responseContentJson.Length > 0)
            {
                var responseContentDoc = JsonDocument.Parse(responseContentJson);
                responseContent = JsonSerializer.Serialize(
                    responseContentDoc,
                    new JsonSerializerOptions { WriteIndented = true }
                );
            }

            _log($"Response Content: {responseContent}");
        }

        return response;
    }
}
