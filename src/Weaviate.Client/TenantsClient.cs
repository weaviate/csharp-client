using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The collection client class
/// </summary>
public partial class CollectionClient
{
    /// <summary>
    /// A client for managing tenants in the Weaviate cluster.
    /// </summary>
    public TenantsClient Tenants => new(this);
}

/// <summary>
/// The tenants client class
/// </summary>
public class TenantsClient
{
    /// <summary>
    /// The collection client
    /// </summary>
    private readonly CollectionClient _collectionClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantsClient"/> class
    /// </summary>
    /// <param name="collectionClient">The collection client</param>
    internal TenantsClient(CollectionClient collectionClient)
    {
        _collectionClient = collectionClient;
    }

    /// <summary>
    /// Creates the tenants
    /// </summary>
    /// <param name="tenants">The tenants</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentException">At least one tenant must be provided. </exception>
    /// <returns>A task containing an enumerable of tenant</returns>
    public async Task<IEnumerable<Tenant>> Create(
        AutoArray<Tenant> tenants,
        CancellationToken cancellationToken = default
    )
    {
        if (tenants == null || !tenants.Any())
        {
            throw new ArgumentException("At least one tenant must be provided.", nameof(tenants));
        }
        // Map Models.Tenant to Rest.Dto.Tenant
        var restTenants = tenants
            .Select(t => new Rest.Dto.Tenant
            {
                Name = t.Name,
                ActivityStatus = Enum.Parse<Rest.Dto.TenantActivityStatus>(
                    t.Status.ToString(),
                    true
                ),
            })
            .ToArray();
        var result = await _collectionClient.Client.RestClient.TenantsAdd(
            _collectionClient.Name,
            restTenants,
            cancellationToken
        );
        return result.Select(t => new Tenant
        {
            Name = t.Name!,
            Status = t.ActivityStatus.HasValue
                ? Enum.Parse<TenantActivityStatus>(t.ActivityStatus.Value.ToString(), true)
                : Models.TenantActivityStatus.Unspecified,
        });
    }

    /// <summary>
    /// Updates the tenants
    /// </summary>
    /// <param name="tenants">The tenants</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentException">At least one tenant must be provided. </exception>
    /// <returns>A task containing an enumerable of tenant</returns>
    public async Task<IEnumerable<Tenant>> Update(
        AutoArray<Tenant> tenants,
        CancellationToken cancellationToken = default
    )
    {
        if (tenants == null || !tenants.Any())
        {
            throw new ArgumentException("At least one tenant must be provided.", nameof(tenants));
        }

        // Map Models.Tenant to Rest.Dto.Tenant
        var restTenants = tenants
            .Select(tenant => new Rest.Dto.Tenant
            {
                Name = tenant.Name,
                ActivityStatus = (Rest.Dto.TenantActivityStatus?)
                    Enum.Parse(
                        typeof(Rest.Dto.TenantActivityStatus),
                        tenant.Status.ToString(),
                        true
                    ),
            })
            .ToArray();

        var result = await _collectionClient.Client.RestClient.TenantUpdate(
            _collectionClient.Name,
            restTenants,
            cancellationToken
        );
        return result.Select(r => new Tenant
        {
            Name = r.Name!,
            Status = r.ActivityStatus.HasValue
                ? (TenantActivityStatus)
                    Enum.Parse(
                        typeof(TenantActivityStatus),
                        r.ActivityStatus.Value.ToString(),
                        true
                    )
                : Models.TenantActivityStatus.Unspecified,
        });
    }

    /// <summary>
    /// Deletes the tenant names
    /// </summary>
    /// <param name="tenantNames">The tenant names</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentException">At least one tenant name must be provided. </exception>
    public async Task Delete(
        AutoArray<string> tenantNames,
        CancellationToken cancellationToken = default
    )
    {
        if (tenantNames == null || !tenantNames.Any())
        {
            throw new ArgumentException(
                "At least one tenant name must be provided.",
                nameof(tenantNames)
            );
        }
        await _collectionClient.Client.RestClient.TenantsDelete(
            _collectionClient.Name,
            tenantNames,
            cancellationToken
        );
    }

    /// <summary>
    /// Gets the tenant name
    /// </summary>
    /// <param name="tenantName">The tenant name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the tenant</returns>
    public async Task<Tenant?> Get(string tenantName, CancellationToken cancellationToken = default)
    {
        var tenants = await List(new[] { tenantName }, cancellationToken);
        return tenants.FirstOrDefault();
    }

    /// <summary>
    /// Lists the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of tenant</returns>
    public async Task<IEnumerable<Tenant>> List(CancellationToken cancellationToken = default) =>
        await List(Array.Empty<string>(), cancellationToken);

    /// <summary>
    /// Lists the tenant names
    /// </summary>
    /// <param name="tenantNames">The tenant names</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of tenant</returns>
    public async Task<IEnumerable<Tenant>> List(
        string[] tenantNames,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _collectionClient.Client.GrpcClient.TenantsGet(
            _collectionClient.Name,
            tenantNames.ToArray(),
            cancellationToken
        );

        return response.Select(t => Tenant.FromGrpc(t));
    }

    /// <summary>
    /// Existses the tenant name
    /// </summary>
    /// <param name="tenantName">The tenant name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> Exists(string tenantName, CancellationToken cancellationToken = default)
    {
        var response = await _collectionClient.Client.GrpcClient.TenantsGet(
            _collectionClient.Name,
            [tenantName],
            cancellationToken
        );

        return response.Any();
    }

    /// <summary>
    /// Activates the tenants
    /// </summary>
    /// <param name="tenants">The tenants</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> Activate(
        Tenant[] tenants,
        CancellationToken cancellationToken = default
    )
    {
        await Activate(tenants.Select(tenant => tenant.Name).ToArray(), cancellationToken);
        return true;
    }

    /// <summary>
    /// Activates the tenant names
    /// </summary>
    /// <param name="tenantNames">The tenant names</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> Activate(
        string[] tenantNames,
        CancellationToken cancellationToken = default
    )
    {
        await Task.WhenAll(
            tenantNames.Select(name =>
                Update(
                    new[]
                    {
                        new Tenant { Name = name, Status = Models.TenantActivityStatus.Active },
                    },
                    cancellationToken
                )
            )
        );
        return true;
    }

    /// <summary>
    /// Deactivates the tenants
    /// </summary>
    /// <param name="tenants">The tenants</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> Deactivate(
        Tenant[] tenants,
        CancellationToken cancellationToken = default
    )
    {
        await Deactivate(tenants.Select(tenant => tenant.Name).ToArray(), cancellationToken);
        return true;
    }

    /// <summary>
    /// Deactivates the tenant names
    /// </summary>
    /// <param name="tenantNames">The tenant names</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> Deactivate(
        string[] tenantNames,
        CancellationToken cancellationToken = default
    )
    {
        await Task.WhenAll(
            tenantNames.Select(name =>
                Update(
                    new[]
                    {
                        new Tenant { Name = name, Status = Models.TenantActivityStatus.Inactive },
                    },
                    cancellationToken
                )
            )
        );
        return true;
    }

    /// <summary>
    /// Offloads the tenant names
    /// </summary>
    /// <param name="tenantNames">The tenant names</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> Offload(
        string[] tenantNames,
        CancellationToken cancellationToken = default
    )
    {
        await Task.WhenAll(
            tenantNames.Select(name =>
                Update(
                    new[]
                    {
                        new Tenant { Name = name, Status = Models.TenantActivityStatus.Offloaded },
                    },
                    cancellationToken
                )
            )
        );
        return true;
    }
}
