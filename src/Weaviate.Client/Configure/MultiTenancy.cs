using Weaviate.Client.Models;

namespace Weaviate.Client;

public static partial class Configure
{
    public static MultiTenancyConfig MultiTenancy(
        bool enabled,
        bool? autoTenantCreation = null,
        bool? autoTenantActivation = null
    )
    {
        var mtc = new MultiTenancyConfig { Enabled = enabled };

        if (autoTenantCreation.HasValue)
        {
            mtc.AutoTenantCreation = autoTenantCreation.Value;
        }

        if (autoTenantActivation.HasValue)
        {
            mtc.AutoTenantActivation = autoTenantActivation.Value;
        }

        return mtc;
    }
}
