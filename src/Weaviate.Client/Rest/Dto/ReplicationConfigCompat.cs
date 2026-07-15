namespace Weaviate.Client.Rest.Dto;

/// <summary>
/// Compatibility extension for the generated <see cref="ReplicationConfig"/> DTO.
/// </summary>
/// <remarks>
/// Weaviate 1.38 removed <c>asyncEnabled</c> from the OpenAPI spec (async replication
/// is now derived server-side as <c>factor &gt; 1</c> unless globally disabled), but the
/// server keeps emitting the field on REST responses through a compatibility shim
/// (<c>adapters/handlers/rest/restcompat</c>), and servers up to 1.37 still honor it as a
/// per-collection setting. Keep the property here so the client round-trips it against
/// every supported server version; the generated model no longer carries it.
/// </remarks>
internal partial record ReplicationConfig
{
    /// <summary>
    /// Enable asynchronous replication (default: <c>false</c>). On Weaviate 1.38+ this is
    /// reported as <c>factor &gt; 1 &amp;&amp; !ASYNC_REPLICATION_DISABLED</c> and is ignored on input.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("asyncEnabled")]
    public bool? AsyncEnabled { get; set; } = default!;
}
