using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Weaviate.Client.Request.Transport;

namespace Weaviate.Client.Request.Testing;

/// <summary>
/// Fluent builder for creating mock HTTP responses.
/// </summary>
public class ResponseBuilder
{
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private object? _body;
    private readonly Dictionary<string, string> _headers = new();

    /// <summary>
    /// Sets the HTTP status code.
    /// </summary>
    public ResponseBuilder WithStatusCode(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    /// <summary>
    /// Sets the response body (will be serialized to JSON).
    /// </summary>
    public ResponseBuilder WithBody(object body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// Sets the response body as JSON string.
    /// </summary>
    public ResponseBuilder WithJson(string json)
    {
        _body = json;
        return this;
    }

    /// <summary>
    /// Adds a response header.
    /// </summary>
    public ResponseBuilder WithHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    /// <summary>
    /// Sets a successful (200 OK) response.
    /// </summary>
    public ResponseBuilder Success()
    {
        _statusCode = HttpStatusCode.OK;
        return this;
    }

    /// <summary>
    /// Sets a not found (404) response.
    /// </summary>
    public ResponseBuilder NotFound()
    {
        _statusCode = HttpStatusCode.NotFound;
        return this;
    }

    /// <summary>
    /// Sets an error (500) response.
    /// </summary>
    public ResponseBuilder ServerError()
    {
        _statusCode = HttpStatusCode.InternalServerError;
        return this;
    }

    /// <summary>
    /// Sets a conflict (409) response.
    /// </summary>
    public ResponseBuilder Conflict()
    {
        _statusCode = HttpStatusCode.Conflict;
        return this;
    }

    /// <summary>
    /// Builds the HTTP response message.
    /// </summary>
    public HttpResponseMessage Build()
    {
        var response = MockRestTransport.CreateJsonResponse(_statusCode, _body);

        foreach (var header in _headers)
        {
            response.Headers.Add(header.Key, header.Value);
        }

        return response;
    }
}

/// <summary>
/// Fluent builder for configuring mock responses based on request patterns.
/// </summary>
public class MockResponseScenario
{
    private readonly MockRestTransport _transport;
    private Func<HttpRequestDetails, bool>? _matcher;

    public MockResponseScenario(MockRestTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Matches requests for a specific operation.
    /// </summary>
    public MockResponseScenario ForOperation(string operationName)
    {
        _matcher = req => req.LogicalRequest?.OperationName == operationName;
        return this;
    }

    /// <summary>
    /// Matches requests with a specific HTTP method and URI pattern.
    /// </summary>
    public MockResponseScenario ForRequest(HttpMethod method, string uriPattern)
    {
        _matcher = req => req.Method == method && req.Uri.Contains(uriPattern);
        return this;
    }

    /// <summary>
    /// Matches requests using a custom predicate.
    /// </summary>
    public MockResponseScenario Matching(Func<HttpRequestDetails, bool> predicate)
    {
        _matcher = predicate;
        return this;
    }

    /// <summary>
    /// Configures the response to return.
    /// </summary>
    public void RespondWith(Func<ResponseBuilder, ResponseBuilder> configure)
    {
        if (_matcher == null)
            throw new InvalidOperationException("Must specify request matcher before calling RespondWith");

        var builder = new ResponseBuilder();
        builder = configure(builder);
        var response = builder.Build();

        _transport.AddResponseRule(_matcher, _ => response);
    }

    /// <summary>
    /// Configures the response to return a specific status code.
    /// </summary>
    public void RespondWith(HttpStatusCode statusCode, object? body = null)
    {
        if (_matcher == null)
            throw new InvalidOperationException("Must specify request matcher before calling RespondWith");

        _transport.AddResponseRule(_matcher, _ => MockRestTransport.CreateJsonResponse(statusCode, body));
    }

    /// <summary>
    /// Configures a success response with optional body.
    /// </summary>
    public void RespondWithSuccess(object? body = null)
    {
        RespondWith(HttpStatusCode.OK, body);
    }

    /// <summary>
    /// Configures a not found response.
    /// </summary>
    public void RespondWithNotFound()
    {
        RespondWith(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Configures a server error response.
    /// </summary>
    public void RespondWithError(string? errorMessage = null)
    {
        var body = errorMessage != null ? new { error = errorMessage } : null;
        RespondWith(HttpStatusCode.InternalServerError, body);
    }
}

/// <summary>
/// Extension methods for setting up mock response scenarios.
/// </summary>
public static class MockResponseExtensions
{
    /// <summary>
    /// Starts building a mock response scenario.
    /// </summary>
    public static MockResponseScenario When(this MockRestTransport transport)
    {
        return new MockResponseScenario(transport);
    }
}
