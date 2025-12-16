using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Migrations;

/// <summary>
/// Represents a planned migration with schema changes to be applied to a collection.
/// </summary>
public class MigrationPlan
{
    /// <summary>
    /// The collection name being migrated.
    /// </summary>
    public required string CollectionName { get; init; }

    /// <summary>
    /// List of schema changes detected.
    /// </summary>
    public required List<SchemaChange> Changes { get; init; }

    /// <summary>
    /// Whether there are any changes to apply.
    /// </summary>
    public bool HasChanges => Changes.Count > 0;

    /// <summary>
    /// Whether all changes are safe (additive only, no breaking changes).
    /// Breaking changes include type modifications, property deletions, etc.
    /// </summary>
    public bool IsSafe => Changes.All(c => c.IsSafe);

    /// <summary>
    /// The current collection config from Weaviate.
    /// </summary>
    public CollectionConfig? CurrentConfig { get; init; }

    /// <summary>
    /// The target collection config from the class definition.
    /// </summary>
    public required CollectionConfig TargetConfig { get; init; }

    /// <summary>
    /// Gets a summary of the migration plan.
    /// </summary>
    public string GetSummary()
    {
        if (!HasChanges)
            return $"No schema changes detected for collection '{CollectionName}'.";

        var summary =
            $"Migration plan for '{CollectionName}' ({Changes.Count} change{(Changes.Count == 1 ? "" : "s")}):";

        foreach (var change in Changes)
        {
            summary +=
                $"\n  {(change.IsSafe ? "✓" : "⚠")} {change.ChangeType}: {change.Description}";
        }

        if (!IsSafe)
        {
            summary +=
                "\n\n⚠ WARNING: This migration contains breaking changes that may cause data loss.";
        }

        return summary;
    }
}

/// <summary>
/// Represents a single schema change operation.
/// </summary>
public class SchemaChange
{
    /// <summary>
    /// The type of change.
    /// </summary>
    public required SchemaChangeType ChangeType { get; init; }

    /// <summary>
    /// Human-readable description of the change.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Whether this change is safe (additive only).
    /// </summary>
    public required bool IsSafe { get; init; }

    /// <summary>
    /// The property being added, updated, or removed (if applicable).
    /// </summary>
    public Property? Property { get; init; }

    /// <summary>
    /// The reference being added, updated, or removed (if applicable).
    /// </summary>
    public Reference? Reference { get; init; }

    /// <summary>
    /// The vector config being added, updated, or removed (if applicable).
    /// </summary>
    public VectorConfig? VectorConfig { get; init; }

    /// <summary>
    /// The previous value (for updates/deletions).
    /// </summary>
    public object? OldValue { get; init; }

    /// <summary>
    /// The new value (for updates/additions).
    /// </summary>
    public object? NewValue { get; init; }
}

/// <summary>
/// Types of schema changes that can occur during migration.
/// </summary>
public enum SchemaChangeType
{
    /// <summary>
    /// A new property was added.
    /// </summary>
    AddProperty,

    /// <summary>
    /// A new reference was added.
    /// </summary>
    AddReference,

    /// <summary>
    /// A new vector configuration was added.
    /// </summary>
    AddVector,

    /// <summary>
    /// Collection description was updated.
    /// </summary>
    UpdateDescription,

    /// <summary>
    /// Property description was updated.
    /// </summary>
    UpdatePropertyDescription,

    /// <summary>
    /// Reference description was updated.
    /// </summary>
    UpdateReferenceDescription,

    /// <summary>
    /// Inverted index configuration was updated.
    /// </summary>
    UpdateInvertedIndex,

    /// <summary>
    /// Vector index configuration was updated.
    /// </summary>
    UpdateVectorIndex,

    /// <summary>
    /// Replication configuration was updated.
    /// </summary>
    UpdateReplication,

    /// <summary>
    /// Multi-tenancy configuration was updated.
    /// </summary>
    UpdateMultiTenancy,

    /// <summary>
    /// A property was removed (BREAKING).
    /// </summary>
    RemoveProperty,

    /// <summary>
    /// A reference was removed (BREAKING).
    /// </summary>
    RemoveReference,

    /// <summary>
    /// A vector configuration was removed (BREAKING).
    /// </summary>
    RemoveVector,

    /// <summary>
    /// A property's data type was changed (BREAKING).
    /// </summary>
    ModifyPropertyType,

    /// <summary>
    /// Other configuration changes.
    /// </summary>
    Other,
}
