using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest.Dto
{
    /// <summary>
    /// Describes which API is effected, usually one of: REST, GraphQL
    /// </summary>
    public class Deprecation
    {
        /// <summary>
        /// The id that uniquely identifies this particular deprecations (mostly used internally)
        /// </summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ID { get; set; }

        /// <summary>
        /// The locations within the specified API affected by this deprecation
        /// </summary>
        [JsonPropertyName("locations")]
        public List<string> Locations { get; set; } = new List<string>();

        /// <summary>
        /// User-required object to not be affected by the (planned) removal
        /// </summary>
        [JsonPropertyName("mitigation")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Mitigation { get; set; }

        /// <summary>
        /// What this deprecation is about
        /// </summary>
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        /// <summary>
        /// A best-effort guess of which upcoming version will remove the feature entirely
        /// </summary>
        [JsonPropertyName("plannedRemovalVersion")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PlannedRemovalVersion { get; set; }

        /// <summary>
        /// If the feature has already been removed, it was removed in this version
        /// </summary>
        [JsonPropertyName("removedIn")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RemovedIn { get; set; }

        /// <summary>
        /// If the feature has already been removed, it was removed at this timestamp
        /// Format: date-time
        /// </summary>
        [JsonPropertyName("removedTime")]
        public DateTime? RemovedTime { get; set; }

        /// <summary>
        /// The deprecation was introduced in this version
        /// Format: date-time
        /// </summary>
        [JsonPropertyName("sinceTime")]
        public DateTime SinceTime { get; set; }

        /// <summary>
        /// The deprecation was introduced in this version
        /// </summary>
        [JsonPropertyName("sinceVersion")]
        public string? SinceVersion { get; set; }

        /// <summary>
        /// Whether the problematic API functionality is deprecated (planned to be removed) or already removed
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}