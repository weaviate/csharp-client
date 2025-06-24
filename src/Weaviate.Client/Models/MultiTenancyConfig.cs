namespace Weaviate.Client.Models;

/// <summary>
/// Configuration related to multi-tenancy within a class
/// </summary>
public class MultiTenancyConfig
{
    private static readonly Lazy<MultiTenancyConfig> _default = new(() => new());

    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    public static MultiTenancyConfig Default => _default.Value;

    /// <summary>
    /// Existing tenants should (not) be turned HOT implicitly when they are accessed and in another activity status (default: false).
    /// </summary>
    public bool AutoTenantActivation { get; set; }

    /// <summary>
    /// Nonexistent tenants should (not) be created implicitly (default: false).
    /// </summary>
    public bool AutoTenantCreation { get; set; }

    /// <summary>
    /// Whether or not multi-tenancy is enabled for this class (default: false).
    /// </summary>
    public bool Enabled { get; set; }
}
