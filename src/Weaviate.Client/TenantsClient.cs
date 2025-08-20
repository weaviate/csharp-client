using Weaviate.Client.Models;

namespace Weaviate.Client;

public class TenantsClient
{
    private CollectionClient _collectionClient;

    internal TenantsClient(CollectionClient collectionClient)
    {
        _collectionClient = collectionClient;
    }

    public async Task<IEnumerable<Tenant>> Add(params string[] tenants)
    {
        var tenantModels = tenants.Select(name => new Tenant { Name = name }).ToArray();

        return await Add(tenantModels);
    }

    public async Task<IEnumerable<Tenant>> Add(params Tenant[] tenants)
    {
        // Map Models.Tenant to Rest.Dto.Tenant
        var restTenants = tenants
            .Select(t => new Rest.Dto.Tenant
            {
                Name = t.Name,
                ActivityStatus = (Rest.Dto.TenantActivityStatus?)
                    Enum.Parse(typeof(Rest.Dto.TenantActivityStatus), t.Status.ToString(), true),
            })
            .ToArray();
        var result = await _collectionClient.Client.RestClient.TenantsAdd(
            _collectionClient.Name,
            restTenants
        );
        return result.Select(t => new Tenant
        {
            Name = t.Name!,
            Status = t.ActivityStatus.HasValue
                ? (Models.TenantActivityStatus)
                    Enum.Parse(
                        typeof(Models.TenantActivityStatus),
                        t.ActivityStatus.Value.ToString(),
                        true
                    )
                : Models.TenantActivityStatus.Unspecified,
        });
    }

    public async Task<IEnumerable<Tenant>> Update(params Tenant[] tenants)
    {
        if (tenants == null || tenants.Length == 0)
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
            restTenants
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

    public async Task Delete(IEnumerable<string> tenantNames)
    {
        await _collectionClient.Client.RestClient.TenantsDelete(
            _collectionClient.Name,
            tenantNames
        );
    }

    public async Task<Tenant?> Get(string tenantName)
    {
        var tenants = await List(tenantName);
        return tenants.FirstOrDefault();
    }

    public async Task<IEnumerable<Tenant>> List(params string[] tenantNames)
    {
        var response = await _collectionClient.Client.GrpcClient.TenantsGet(
            _collectionClient.Name,
            tenantNames
        );

        return response.Select(t => Tenant.FromGrpc(t));
    }

    public async Task<bool> Exists(string tenantName)
    {
        var response = await _collectionClient.Client.GrpcClient.TenantsGet(
            _collectionClient.Name,
            [tenantName]
        );

        return response.Any();
    }

    public async Task<bool> Activate(params Tenant[] tenants)
    {
        await Activate(tenants.Select(tenant => tenant.Name).ToArray());

        return true;
    }

    public async Task<bool> Activate(params string[] tenantNames)
    {
        await Task.WhenAll(
            tenantNames.Select(name =>
                Update(new Tenant { Name = name, Status = Models.TenantActivityStatus.Active })
            )
        );
        return true;
    }

    public async Task<bool> Deactivate(params Tenant[] tenants)
    {
        await Deactivate(tenants.Select(tenant => tenant.Name).ToArray());

        return true;
    }

    public async Task<bool> Deactivate(params string[] tenantNames)
    {
        await Task.WhenAll(
            tenantNames.Select(name =>
                Update(new Tenant { Name = name, Status = Models.TenantActivityStatus.Inactive })
            )
        );
        return true;
    }

    public async Task<bool> Offload(params string[] tenantNames)
    {
        await Task.WhenAll(
            tenantNames.Select(name =>
                Update(new Tenant { Name = name, Status = Models.TenantActivityStatus.Offloaded })
            )
        );
        return true;
    }
}
