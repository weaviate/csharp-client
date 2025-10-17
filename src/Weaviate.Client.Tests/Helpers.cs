using System.Text.Json;
using System.Text.Json.Nodes;

namespace Weaviate.Client.Tests;

public static class Helpers
{
    // Useful for collection names, backups, aliases, etc.
    public static string GenerateUniqueIdentifier(string name)
    {
        // Sanitize the collection name
        name = SanitizeCollectionName(name);
        // Generate a random part using GUID
        var randomPart = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12);
        return string.Concat(name, randomPart).ToLowerInvariant();
    }

    public static string SanitizeCollectionName(string name)
    {
        name = name.Replace("[", "")
            .Replace("]", "")
            .Replace("-", "")
            .Replace(" ", "")
            .Replace(".", "");
        return char.ToUpper(name[0]) + name.Substring(1);
    }
}

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

public static class JsonComparer
{
    /// <summary>
    /// Sorts all properties in a JSON document recursively by property name
    /// </summary>
    /// <param name="jsonString">JSON string to sort</param>
    /// <returns>Sorted JSON string</returns>
    public static string SortJsonString(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            return jsonString;

        JsonNode? node = JsonNode.Parse(jsonString);
        JsonNode? sortedNode = SortJsonNode(node);
        return sortedNode?.ToJsonString() ?? string.Empty;
    }

    /// <summary>
    /// Sorts all properties in a JsonDocument recursively by property name
    /// </summary>
    /// <param name="document">JsonDocument to sort</param>
    /// <returns>Sorted JSON string</returns>
    public static string SortJsonDocument(JsonDocument document)
    {
        if (document == null)
            return string.Empty;

        JsonNode? node = JsonNode.Parse(document.RootElement.GetRawText());
        JsonNode? sortedNode = SortJsonNode(node);
        return sortedNode?.ToJsonString() ?? string.Empty;
    }

    /// <summary>
    /// Recursively sorts JsonNode properties
    /// </summary>
    private static JsonNode? SortJsonNode(JsonNode? node)
    {
        if (node == null)
            return null;

        switch (node)
        {
            case JsonObject obj:
                var sortedObj = new JsonObject();
                foreach (var kvp in obj.OrderBy(x => x.Key))
                {
                    sortedObj[kvp.Key] = SortJsonNode(kvp.Value);
                }
                return sortedObj;

            case JsonArray arr:
                var sortedArr = new JsonArray();
                foreach (var item in arr)
                {
                    sortedArr.Add(SortJsonNode(item));
                }
                return sortedArr;

            default:
                // For primitive values (string, number, boolean, null), return as-is
                return node.DeepClone();
        }
    }

    /// <summary>
    /// Compares two JSON strings for equality after sorting properties
    /// </summary>
    /// <param name="json1">First JSON string</param>
    /// <param name="json2">Second JSON string</param>
    /// <returns>True if the JSON documents are equivalent</returns>
    public static bool AreJsonEqual(string json1, string json2)
    {
        if (string.IsNullOrEmpty(json1) && string.IsNullOrEmpty(json2))
            return true;

        if (string.IsNullOrEmpty(json1) || string.IsNullOrEmpty(json2))
            return false;

        try
        {
            string sorted1 = SortJsonString(json1);
            string sorted2 = SortJsonString(json2);
            return sorted1 == sorted2;
        }
        catch (JsonException)
        {
            // If either JSON is invalid, they're not equal
            return false;
        }
    }

    /// <summary>
    /// Compares two JsonDocuments for equality after sorting properties
    /// </summary>
    /// <param name="doc1">First JsonDocument</param>
    /// <param name="doc2">Second JsonDocument</param>
    /// <returns>True if the JSON documents are equivalent</returns>
    public static bool AreJsonEqual(JsonDocument doc1, JsonDocument doc2)
    {
        if (doc1 == null && doc2 == null)
            return true;

        if (doc1 == null || doc2 == null)
            return false;

        try
        {
            string sorted1 = SortJsonDocument(doc1);
            string sorted2 = SortJsonDocument(doc2);
            return sorted1 == sorted2;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Mixed comparison: JsonDocument vs JSON string
    /// </summary>
    /// <param name="document">JsonDocument</param>
    /// <param name="jsonString">JSON string</param>
    /// <returns>True if the JSON documents are equivalent</returns>
    public static bool AreJsonEqual(JsonDocument document, string jsonString)
    {
        if (document == null && string.IsNullOrEmpty(jsonString))
            return true;

        if (document == null || string.IsNullOrEmpty(jsonString))
            return false;

        try
        {
            string sorted1 = SortJsonDocument(document);
            string sorted2 = SortJsonString(jsonString);
            return sorted1 == sorted2;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
