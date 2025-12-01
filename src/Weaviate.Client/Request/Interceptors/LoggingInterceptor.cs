using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Weaviate.Client.Request.Interceptors;

/// <summary>
/// Interceptor that logs request and response information.
/// </summary>
public class LoggingInterceptor : RequestInterceptorBase
{
    /// <summary>
    /// Maximum depth for JSON serialization to avoid excessive logging.
    /// </summary>
    private const int MaxSerializationDepth = 3;

    /// <summary>
    /// Maximum length of response string to log before truncating.
    /// </summary>
    private const int MaxResponseLength = 200;

    private readonly ILogger? _logger;
    private readonly Action<string>? _logAction;
    private readonly LogLevel _logLevel;
    private readonly bool _logRequestDetails;
    private readonly bool _logResponseDetails;

    /// <summary>
    /// Creates a logging interceptor using an ILogger.
    /// </summary>
    public LoggingInterceptor(
        ILogger logger,
        LogLevel logLevel = LogLevel.Information,
        bool logRequestDetails = true,
        bool logResponseDetails = false)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logLevel = logLevel;
        _logRequestDetails = logRequestDetails;
        _logResponseDetails = logResponseDetails;
    }

    /// <summary>
    /// Creates a logging interceptor using a custom log action.
    /// </summary>
    public LoggingInterceptor(
        Action<string> logAction,
        bool logRequestDetails = true,
        bool logResponseDetails = false)
    {
        _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
        _logRequestDetails = logRequestDetails;
        _logResponseDetails = logResponseDetails;
    }

    public override Task<RequestContext> OnBeforeSendAsync(RequestContext context)
    {
        var message = $"Sending {context.Request.OperationName} request";

        if (_logRequestDetails)
        {
            message += $"\n  Type: {context.Request.Type}";
            message += $"\n  Protocol: {context.Request.PreferredProtocol}";
            if (context.Collection != null)
                message += $"\n  Collection: {context.Collection}";
            if (context.Tenant != null)
                message += $"\n  Tenant: {context.Tenant}";
            if (context.Timeout.HasValue)
                message += $"\n  Timeout: {context.Timeout.Value.TotalSeconds}s";
        }

        Log(message);
        return base.OnBeforeSendAsync(context);
    }

    public override Task<TResponse> OnAfterReceiveAsync<TResponse>(RequestContext context, TResponse response)
    {
        var message = $"Received response for {context.Request.OperationName}";

        if (_logResponseDetails && response != null)
        {
            try
            {
                var responseType = response.GetType();
                message += $"\n  Response Type: {responseType.Name}";

                // Try to serialize for debugging (be careful with large responses)
                if (responseType.IsClass && responseType != typeof(string))
                {
                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        MaxDepth = MaxSerializationDepth
                    });

                    if (json.Length > MaxResponseLength)
                        message += $"\n  Response: {json.Substring(0, MaxResponseLength)}...";
                    else
                        message += $"\n  Response: {json}";
                }
                else
                {
                    message += $"\n  Response: {response}";
                }
            }
            catch
            {
                message += $"\n  Response: (could not serialize)";
            }
        }

        Log(message);
        return base.OnAfterReceiveAsync(context, response);
    }

    public override Task OnErrorAsync(RequestContext context, Exception exception)
    {
        var message = $"Error executing {context.Request.OperationName}: {exception.Message}";
        Log(message, isError: true);
        return base.OnErrorAsync(context, exception);
    }

    private void Log(string message, bool isError = false)
    {
        if (_logger != null)
        {
            var level = isError ? LogLevel.Error : _logLevel;
            _logger.Log(level, message);
        }
        else if (_logAction != null)
        {
            _logAction(message);
        }
    }
}
