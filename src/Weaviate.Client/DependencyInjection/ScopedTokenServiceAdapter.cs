using Microsoft.Extensions.DependencyInjection;

namespace Weaviate.Client.DependencyInjection;

/// <summary>
/// Bridges a singleton WeaviateClient to a scoped ITokenService registered in DI.
/// Each call to GetAccessTokenAsync / RefreshTokenAsync opens a fresh DI scope so
/// the underlying provider is resolved according to its registered lifetime (typically
/// Scoped, meaning once per HTTP request when used in ASP.NET Core).
/// </summary>
internal sealed class ScopedTokenServiceAdapter : ITokenService
{
    private readonly IServiceScopeFactory _scopeFactory;

    internal ScopedTokenServiceAdapter(IServiceScopeFactory scopeFactory) =>
        _scopeFactory = scopeFactory;

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<ITokenService>();
        return await svc.GetAccessTokenAsync(cancellationToken);
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<ITokenService>();
        return await svc.RefreshTokenAsync(cancellationToken);
    }

    // Always optimistic — the real authentication check happens inside GetAccessTokenAsync.
    public bool IsAuthenticated() => true;
}
