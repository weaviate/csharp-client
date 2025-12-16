using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient
{
    public TenantsClient Tenants => new(this);
}

public class TenantsClient
{
    private readonly CollectionClient _collectionClient;

    internal TenantsClient(CollectionClient collectionClient)
    {
        _collectionClient = collectionClient;
    }

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
                ? (Models.TenantActivityStatus)
                    Enum.Parse(
                        typeof(Models.TenantActivityStatus),
                        r.ActivityStatus.Value.ToString(),
                        true
                    )
                : Models.TenantActivityStatus.Unspecified,
        });
    }

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

    public async Task<Tenant?> Get(string tenantName, CancellationToken cancellationToken = default)
    {
        var tenants = await List(new[] { tenantName }, cancellationToken);
        return tenants.FirstOrDefault();
    }

    public async Task<IEnumerable<Tenant>> List(CancellationToken cancellationToken = default) =>
        await List(Array.Empty<string>(), cancellationToken);

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

    public async Task<bool> Exists(string tenantName, CancellationToken cancellationToken = default)
    {
        var response = await _collectionClient.Client.GrpcClient.TenantsGet(
            _collectionClient.Name,
            [tenantName],
            cancellationToken
        );

        return response.Any();
    }

    public async Task<bool> Activate(
        Tenant[] tenants,
        CancellationToken cancellationToken = default
    )
    {
        await Activate(tenants.Select(tenant => tenant.Name).ToArray(), cancellationToken);
        return true;
    }

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

    public async Task<bool> Deactivate(
        Tenant[] tenants,
        CancellationToken cancellationToken = default
    )
    {
        await Deactivate(tenants.Select(tenant => tenant.Name).ToArray(), cancellationToken);
        return true;
    }

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
