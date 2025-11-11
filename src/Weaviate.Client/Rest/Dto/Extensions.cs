namespace Weaviate.Client.Rest.Dto;

internal partial class NestedProperty
{
    public Models.Property ToModel()
    {
        return new Models.Property
        {
            Name = Name ?? string.Empty,
            DataType = DataType?.ToList() ?? new List<string>(),
            Description = Description,
            IndexFilterable = IndexFilterable,
            IndexSearchable = IndexSearchable,
            IndexRangeFilters = IndexRangeFilters,
            PropertyTokenization = (Models.PropertyTokenization?)Tokenization,
            NestedProperties = NestedProperties?.Select(np => np.ToModel()).ToArray(),
        };
    }
}

internal partial class Property {
    public  Models.Property ToModel()
    {
        return new Models.Property
        {
            Name = Name ?? string.Empty,
            DataType = DataType?.ToList() ?? new List<string>(),
            Description = Description,
            IndexFilterable = IndexFilterable,
#pragma warning disable CS0612 // Type or member is obsolete
            IndexInverted = IndexInverted,
#pragma warning restore CS0612 // Type or member is obsolete
            IndexRangeFilters = IndexRangeFilters,
            IndexSearchable = IndexSearchable,
            PropertyTokenization = (Models.PropertyTokenization?)Tokenization,
            NestedProperties = NestedProperties?.Select(np => np.ToModel()).ToArray(),
        };
    }
}

internal partial class GeoCoordinates
{
    public Models.GeoCoordinate ToModel()
    {
        return new Models.GeoCoordinate(Latitude!.Value, Longitude!.Value);
    }
}

internal partial class PhoneNumber
{
    public Models.PhoneNumber ToModel()
    {
        return new Models.PhoneNumber(Input!)
        {
            DefaultCountry = DefaultCountry,
            CountryCode = CountryCode is null ? null :Convert.ToUInt64(CountryCode),
            National = National is null ? null : Convert.ToUInt64(National),
            InternationalFormatted = InternationalFormatted,
            NationalFormatted = NationalFormatted,
            Valid = Valid,
        };
    }
}

// RBAC Permission mapping
internal partial class Permission
{
    public Weaviate.Client.Models.PermissionInfo ToModel()
    {
        var resources = new Weaviate.Client.Models.PermissionResource(
            Backups is not null ? new Weaviate.Client.Models.BackupsResource(Backups.Collection) : null,
            Data is not null ? new Weaviate.Client.Models.DataResource(Data.Collection, Data.Tenant, Data.Object) : null,
            Nodes is not null ? new Weaviate.Client.Models.NodesResource(Nodes.Collection, Nodes.Verbosity?.ToString()) : null,
            Users is not null ? new Weaviate.Client.Models.UsersResource(Users.Users1) : null,
            Groups is not null ? new Weaviate.Client.Models.GroupsResource(Groups.Group, Groups.GroupType?.ToString()) : null,
            Tenants is not null ? new Weaviate.Client.Models.TenantsResource(Tenants.Collection, Tenants.Tenant) : null,
            Roles is not null ? new Weaviate.Client.Models.RolesResource(Roles.Role, Roles.Scope?.ToString()) : null,
            Collections is not null ? new Weaviate.Client.Models.CollectionsResource(Collections.Collection) : null,
            Replicate is not null ? new Weaviate.Client.Models.ReplicateResource(Replicate.Collection, Replicate.Shard) : null,
            Aliases is not null ? new Weaviate.Client.Models.AliasesResource(Aliases.Collection, Aliases.Alias) : null
        );
        return new Weaviate.Client.Models.PermissionInfo(Action.ToEnumMemberString() ?? string.Empty, resources);
    }
}

// RBAC Role mapping
internal partial class Role
{
    public Weaviate.Client.Models.RoleInfo ToModel()
    {
        return new Weaviate.Client.Models.RoleInfo(
            Name ?? string.Empty,
            Models.Permissions.Parse((Permissions ?? []).Select(p => p.ToModel()))
        );
    }
}

