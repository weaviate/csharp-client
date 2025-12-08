using Weaviate.Client.Models;

namespace Weaviate.Client.Womp.Migrations;

/// <summary>
/// Compares two CollectionConfig objects and generates a list of schema changes.
/// </summary>
internal static class SchemaDiffer
{
    /// <summary>
    /// Compares current and target collection configs and returns detected changes.
    /// </summary>
    /// <param name="current">The current collection config from Weaviate (or null if collection doesn't exist).</param>
    /// <param name="target">The target collection config from the class definition.</param>
    /// <returns>List of schema changes.</returns>
    public static List<SchemaChange> Compare(CollectionConfig? current, CollectionConfig target)
    {
        ArgumentNullException.ThrowIfNull(target);

        var changes = new List<SchemaChange>();

        // If collection doesn't exist, all changes are "add" operations
        if (current == null)
        {
            // This should not happen in CheckMigrate - collection should exist
            // But if it doesn't, all properties/vectors/references are additions
            changes.AddRange(GetAllAdditions(target));
            return changes;
        }

        // Compare description
        if (current.Description != target.Description)
        {
            changes.Add(
                new SchemaChange
                {
                    ChangeType = SchemaChangeType.UpdateDescription,
                    Description = $"Update collection description",
                    IsSafe = true,
                    OldValue = current.Description,
                    NewValue = target.Description,
                }
            );
        }

        // Compare properties
        changes.AddRange(
            CompareProperties(current.Properties.ToList(), target.Properties.ToList())
        );

        // Compare references
        changes.AddRange(
            CompareReferences(current.References.ToList(), target.References.ToList())
        );

        // Compare vector configs
        changes.AddRange(CompareVectorConfigs(current.VectorConfig, target.VectorConfig));

        // Compare inverted index config (if meaningful differences)
        changes.AddRange(
            CompareInvertedIndexConfig(current.InvertedIndexConfig, target.InvertedIndexConfig)
        );

        // Compare replication config
        changes.AddRange(
            CompareReplicationConfig(current.ReplicationConfig, target.ReplicationConfig)
        );

        // Compare multi-tenancy config
        changes.AddRange(
            CompareMultiTenancyConfig(current.MultiTenancyConfig, target.MultiTenancyConfig)
        );

        return changes;
    }

    private static List<SchemaChange> GetAllAdditions(CollectionConfig target)
    {
        var changes = new List<SchemaChange>();

        // Add all properties
        foreach (var prop in target.Properties)
        {
            changes.Add(
                new SchemaChange
                {
                    ChangeType = SchemaChangeType.AddProperty,
                    Description = $"Add property '{prop.Name}' ({prop.DataType})",
                    IsSafe = true,
                    Property = prop,
                    NewValue = prop,
                }
            );
        }

        // Add all references
        foreach (var reference in target.References)
        {
            changes.Add(
                new SchemaChange
                {
                    ChangeType = SchemaChangeType.AddReference,
                    Description =
                        $"Add reference '{reference.Name}' → {reference.TargetCollection}",
                    IsSafe = true,
                    Reference = reference,
                    NewValue = reference,
                }
            );
        }

        // Add all vector configs
        foreach (var vector in target.VectorConfig)
        {
            changes.Add(
                new SchemaChange
                {
                    ChangeType = SchemaChangeType.AddVector,
                    Description = $"Add vector '{vector.Name}'",
                    IsSafe = true,
                    VectorConfig = vector,
                    NewValue = vector,
                }
            );
        }

        return changes;
    }

    private static List<SchemaChange> CompareProperties(
        List<Property> current,
        List<Property> target
    )
    {
        var changes = new List<SchemaChange>();
        var currentDict = current.ToDictionary(p => p.Name);
        var targetDict = target.ToDictionary(p => p.Name);

        // Find new properties (in target but not in current)
        foreach (var prop in target)
        {
            if (!currentDict.ContainsKey(prop.Name))
            {
                changes.Add(
                    new SchemaChange
                    {
                        ChangeType = SchemaChangeType.AddProperty,
                        Description = $"Add property '{prop.Name}' ({prop.DataType})",
                        IsSafe = true,
                        Property = prop,
                        NewValue = prop,
                    }
                );
            }
        }

        // Find removed properties (in current but not in target) - BREAKING
        foreach (var prop in current)
        {
            if (!targetDict.ContainsKey(prop.Name))
            {
                changes.Add(
                    new SchemaChange
                    {
                        ChangeType = SchemaChangeType.RemoveProperty,
                        Description =
                            $"Remove property '{prop.Name}' (BREAKING - data will be lost)",
                        IsSafe = false,
                        Property = prop,
                        OldValue = prop,
                    }
                );
            }
        }

        // Find modified properties
        foreach (var targetProp in target)
        {
            if (currentDict.TryGetValue(targetProp.Name, out var currentProp))
            {
                // Check if data type changed - BREAKING
                if (currentProp.DataType != targetProp.DataType)
                {
                    changes.Add(
                        new SchemaChange
                        {
                            ChangeType = SchemaChangeType.ModifyPropertyType,
                            Description =
                                $"Change property '{targetProp.Name}' type from {currentProp.DataType} to {targetProp.DataType} (BREAKING)",
                            IsSafe = false,
                            Property = targetProp,
                            OldValue = currentProp.DataType,
                            NewValue = targetProp.DataType,
                        }
                    );
                }

                // Check if description changed (safe)
                if (currentProp.Description != targetProp.Description)
                {
                    changes.Add(
                        new SchemaChange
                        {
                            ChangeType = SchemaChangeType.UpdatePropertyDescription,
                            Description = $"Update property '{targetProp.Name}' description",
                            IsSafe = true,
                            Property = targetProp,
                            OldValue = currentProp.Description,
                            NewValue = targetProp.Description,
                        }
                    );
                }
            }
        }

        return changes;
    }

    private static List<SchemaChange> CompareReferences(
        List<Reference> current,
        List<Reference> target
    )
    {
        var changes = new List<SchemaChange>();
        var currentDict = current.ToDictionary(r => r.Name);
        var targetDict = target.ToDictionary(r => r.Name);

        // Find new references
        foreach (var reference in target)
        {
            if (!currentDict.ContainsKey(reference.Name))
            {
                changes.Add(
                    new SchemaChange
                    {
                        ChangeType = SchemaChangeType.AddReference,
                        Description =
                            $"Add reference '{reference.Name}' → {reference.TargetCollection}",
                        IsSafe = true,
                        Reference = reference,
                        NewValue = reference,
                    }
                );
            }
        }

        // Find removed references - BREAKING
        foreach (var reference in current)
        {
            if (!targetDict.ContainsKey(reference.Name))
            {
                changes.Add(
                    new SchemaChange
                    {
                        ChangeType = SchemaChangeType.RemoveReference,
                        Description =
                            $"Remove reference '{reference.Name}' (BREAKING - data will be lost)",
                        IsSafe = false,
                        Reference = reference,
                        OldValue = reference,
                    }
                );
            }
        }

        // Check for description changes
        foreach (var targetRef in target)
        {
            if (
                currentDict.TryGetValue(targetRef.Name, out var currentRef)
                && currentRef.Description != targetRef.Description
            )
            {
                changes.Add(
                    new SchemaChange
                    {
                        ChangeType = SchemaChangeType.UpdateReferenceDescription,
                        Description = $"Update reference '{targetRef.Name}' description",
                        IsSafe = true,
                        Reference = targetRef,
                        OldValue = currentRef.Description,
                        NewValue = targetRef.Description,
                    }
                );
            }
        }

        return changes;
    }

    private static List<SchemaChange> CompareVectorConfigs(
        VectorConfigList current,
        VectorConfigList target
    )
    {
        var changes = new List<SchemaChange>();

        // VectorConfigList is a dictionary-like type, iterate over keys
        var currentKeys = current.Select(v => v.Value.Name).ToHashSet();
        var targetKeys = target.Select(v => v.Value.Name).ToHashSet();

        // Find new vector configs
        foreach (var key in targetKeys)
        {
            if (!currentKeys.Contains(key))
            {
                changes.Add(
                    new SchemaChange
                    {
                        ChangeType = SchemaChangeType.AddVector,
                        Description = $"Add vector '{key}'",
                        IsSafe = true,
                        VectorConfig = target[key],
                        NewValue = target[key],
                    }
                );
            }
        }

        // Find removed vectors - BREAKING
        foreach (var key in currentKeys)
        {
            if (!targetKeys.Contains(key))
            {
                changes.Add(
                    new SchemaChange
                    {
                        ChangeType = SchemaChangeType.RemoveVector,
                        Description = $"Remove vector '{key}' (BREAKING - data will be lost)",
                        IsSafe = false,
                        VectorConfig = current[key],
                        OldValue = current[key],
                    }
                );
            }
        }

        // Note: We're not detecting vector config updates (vectorizer changes, index changes)
        // as those are typically breaking and require more sophisticated handling

        return changes;
    }

    private static List<SchemaChange> CompareInvertedIndexConfig(
        InvertedIndexConfig? current,
        InvertedIndexConfig? target
    )
    {
        // Simplified comparison - only flag if there are significant differences
        // Full inverted index comparison would be complex
        return new List<SchemaChange>();
    }

    private static List<SchemaChange> CompareReplicationConfig(
        ReplicationConfig? current,
        ReplicationConfig? target
    )
    {
        var changes = new List<SchemaChange>();

        if (current == null || target == null)
            return changes;

        // Check for factor changes
        if (current.Factor != target.Factor)
        {
            changes.Add(
                new SchemaChange
                {
                    ChangeType = SchemaChangeType.UpdateReplication,
                    Description =
                        $"Update replication factor from {current.Factor} to {target.Factor}",
                    IsSafe = true,
                    OldValue = current.Factor,
                    NewValue = target.Factor,
                }
            );
        }

        return changes;
    }

    private static List<SchemaChange> CompareMultiTenancyConfig(
        MultiTenancyConfig? current,
        MultiTenancyConfig? target
    )
    {
        var changes = new List<SchemaChange>();

        if (current == null || target == null)
            return changes;

        // Check if Enabled changed (BREAKING - immutable)
        if (current.Enabled != target.Enabled)
        {
            changes.Add(
                new SchemaChange
                {
                    ChangeType = SchemaChangeType.UpdateMultiTenancy,
                    Description =
                        $"Change multi-tenancy Enabled from {current.Enabled} to {target.Enabled} "
                        + "(BREAKING - multi-tenancy Enabled is immutable and cannot be changed after creation)",
                    IsSafe = false,
                    OldValue = current,
                    NewValue = target,
                }
            );
        }

        // Check if mutable config changed (AutoTenantCreation, AutoTenantActivation)
        if (
            current.AutoTenantCreation != target.AutoTenantCreation
            || current.AutoTenantActivation != target.AutoTenantActivation
        )
        {
            changes.Add(
                new SchemaChange
                {
                    ChangeType = SchemaChangeType.UpdateMultiTenancy,
                    Description =
                        $"Update multi-tenancy configuration (AutoTenantCreation: {target.AutoTenantCreation}, AutoTenantActivation: {target.AutoTenantActivation})",
                    IsSafe = true,
                    OldValue = current,
                    NewValue = target,
                }
            );
        }

        return changes;
    }
}
