using Weaviate.Client.CollectionMapper.Migrations;
using Weaviate.Client.CollectionMapper.Schema;
using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Extensions;

/// <summary>
/// Provides extension methods for WeaviateClient to manage ORM-based collection migrations.
/// </summary>
public static class CollectionMigrationExtensions
{
    /// <summary>
    /// Checks for schema changes between a C# class definition and an existing Weaviate collection.
    /// </summary>
    /// <typeparam name="T">The C# class representing the Weaviate collection.</typeparam>
    /// <param name="collections">The CollectionsClient instance from WeaviateClient.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A MigrationPlan object detailing the detected changes.</returns>
    public static async Task<MigrationPlan> CheckMigrate<T>(
        this CollectionsClient collections,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var targetConfig = CollectionSchemaBuilder.FromClass<T>();
        var collectionName = targetConfig.Name;

        CollectionConfig? currentConfig = null;
        try
        {
            currentConfig = await collections.Export(collectionName, cancellationToken);
        }
        catch (Exception ex)
        {
            // If collection not found, currentConfig remains null.
            // Other exceptions are re-thrown.
            if (!ex.Message.Contains("not found"))
            {
                throw;
            }
        }

        var changes = SchemaDiffer.Compare(currentConfig, targetConfig);

        return new MigrationPlan
        {
            CollectionName = collectionName,
            Changes = changes,
            CurrentConfig = currentConfig,
            TargetConfig = targetConfig,
        };
    }

    /// <summary>
    /// Applies schema changes to an existing Weaviate collection based on a C# class definition.
    /// </summary>
    /// <typeparam name="T">The C# class representing the Weaviate collection.</typeparam>
    /// <param name="collections">The CollectionsClient instance from WeaviateClient.</param>
    /// <param name="checkFirst">If true, performs a check and throws an exception if breaking changes are found without explicit confirmation.</param>
    /// <param name="allowBreakingChanges">If true, allows applying migrations even if they contain breaking changes. USE WITH CAUTION.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A MigrationPlan object detailing the applied changes.</returns>
    public static async Task<MigrationPlan> Migrate<T>(
        this CollectionsClient collections,
        bool checkFirst = true,
        bool allowBreakingChanges = false,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var migrationPlan = await collections.CheckMigrate<T>(cancellationToken);

        if (!migrationPlan.HasChanges)
        {
            return migrationPlan; // Nothing to do
        }

        if (checkFirst && !migrationPlan.IsSafe && !allowBreakingChanges)
        {
            throw new InvalidOperationException(
                $"Migration for collection '{migrationPlan.CollectionName}' contains breaking changes. "
                    + "Set 'allowBreakingChanges' to true to proceed (USE WITH CAUTION) or fix your schema."
            );
        }

        if (migrationPlan.CurrentConfig == null)
        {
            // Collection does not exist, create it
            await collections.Create(migrationPlan.TargetConfig, cancellationToken);
            return migrationPlan;
        }

        // Get a single client for the collection to apply all changes
        var collectionClient = collections.Use(migrationPlan.CollectionName);

        // Apply changes
        foreach (var change in migrationPlan.Changes)
        {
            switch (change.ChangeType)
            {
                case SchemaChangeType.AddProperty:
                    if (change.Property != null)
                    {
                        await collectionClient.Config.AddProperty(
                            change.Property,
                            cancellationToken: cancellationToken
                        );
                    }
                    break;
                case SchemaChangeType.AddReference:
                    if (change.Reference != null)
                    {
                        await collectionClient.Config.AddReference(
                            change.Reference,
                            cancellationToken
                        );
                    }
                    break;
                case SchemaChangeType.AddVector:
                    if (change.VectorConfig != null)
                    {
                        await collectionClient.Config.AddVector(
                            change.VectorConfig,
                            cancellationToken
                        );
                    }
                    break;

                case SchemaChangeType.UpdateDescription:
                case SchemaChangeType.UpdatePropertyDescription:
                case SchemaChangeType.UpdateReferenceDescription:
                    // SchemaDiffer marks these as safe (IsSafe=true), but there's no direct API for updating descriptions.
                    // For now, these are treated as info-only changes and are not actively applied by the ORM.
                    break;
                case SchemaChangeType.UpdateInvertedIndex:
                case SchemaChangeType.UpdateVectorIndex:
                case SchemaChangeType.UpdateReplication:
                case SchemaChangeType.UpdateMultiTenancy:
                    // SchemaDiffer marks these as safe (IsSafe=true) for additive or non-breaking updates,
                    // but there's no direct simple API for applying these changes through the current client/ORM.
                    // These typically require specific update calls that are not yet integrated or more complex handling.
                    // For now, these are treated as info-only changes and are not actively applied by the ORM.
                    break;
                case SchemaChangeType.RemoveProperty:
                case SchemaChangeType.RemoveReference:
                case SchemaChangeType.RemoveVector:
                case SchemaChangeType.ModifyPropertyType:
                    // These are breaking changes (IsSafe=false) and are not applied by the ORM directly.
                    // The 'allowBreakingChanges' flag allows the operation to proceed, but the user is
                    // responsible for handling the implications (e.g., data loss, manual recreation).
                    // The ORM does not attempt to apply these.
                    break;
                case SchemaChangeType.Other:
                    // Generic catch-all for other changes, treated as info-only or requiring manual intervention.
                    break;
                default:
                    throw new NotImplementedException(
                        $"Migration change type '{change.ChangeType}' is not yet implemented or recognized for application."
                    );
            }
        }

        return migrationPlan;
    }
}
