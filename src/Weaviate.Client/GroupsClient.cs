using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for listing and inspecting groups (role assignments for groups handled via RolesClient).
/// </summary>
public class GroupsClient
{
    private readonly WeaviateClient _client;

    internal GroupsClient(WeaviateClient client) => _client = client;

    public Task<IEnumerable<string>> List(string groupType) =>
        _client.RestClient.GroupsList(groupType);

    public Task<IEnumerable<RoleInfo>> Roles(
        string groupId,
        string groupType,
        bool? includeFullRoles = null
    ) => _client.Roles.RolesForGroup(groupId, groupType, includeFullRoles);
}
