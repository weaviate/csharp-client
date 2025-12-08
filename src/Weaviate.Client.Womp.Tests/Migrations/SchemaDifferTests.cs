using Weaviate.Client.Models;
using Weaviate.Client.Womp.Migrations;
using Xunit;

namespace Weaviate.Client.Womp.Tests.Migrations;

public class SchemaDifferTests
{
    [Fact]
    public void Compare_IdenticalConfigs_ReturnsNoChanges()
    {
        // Arrange
        var collectionConfig = new CollectionConfig
        {
            Name = "TestCollection",
            Description = "A test collection",
            Properties = new List<Property>
            {
                new Property { Name = "prop1", DataType = DataType.Text },
                new Property { Name = "prop2", DataType = DataType.Int },
            }.ToArray(),
            References = new List<Reference>
            {
                new Reference("ref1", "TargetCollection1"),
            }.ToArray(),
            VectorConfig = new VectorConfigList { },
            InvertedIndexConfig = new InvertedIndexConfig { IndexTimestamps = true },
            ReplicationConfig = new ReplicationConfig { Factor = 2 },
            MultiTenancyConfig = new MultiTenancyConfig { Enabled = true },
        };

        // Act
        var changes = SchemaDiffer.Compare(collectionConfig, collectionConfig);

        // Assert
        Assert.Empty(changes);
    }

    [Fact]
    public void Compare_AddedProperty_DetectsAddition()
    {
        // Arrange
        var current = new CollectionConfig
        {
            Name = "Test",
            Properties = new List<Property>
            {
                new Property { Name = "prop1", DataType = DataType.Text },
            }.ToArray(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = new List<Property>
            {
                new Property { Name = "prop1", DataType = DataType.Text },
                new Property { Name = "prop2", DataType = DataType.Int },
            }.ToArray(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.AddProperty, changes[0].ChangeType);
        Assert.True(changes[0].IsSafe);
        Assert.Equal("prop2", changes[0].Property?.Name);
    }

    [Fact]
    public void Compare_RemovedProperty_DetectsRemoval()
    {
        // Arrange
        var current = new CollectionConfig
        {
            Name = "Test",
            Properties = new List<Property>
            {
                new Property { Name = "prop1", DataType = DataType.Text },
                new Property { Name = "prop2", DataType = DataType.Int },
            }.ToArray(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = new List<Property>
            {
                new Property { Name = "prop1", DataType = DataType.Text },
            }.ToArray(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.RemoveProperty, changes[0].ChangeType);
        Assert.False(changes[0].IsSafe);
        Assert.Contains("BREAKING", changes[0].Description);
    }

    [Fact]
    public void Compare_ModifiedPropertyType_DetectsModification()
    {
        // Arrange
        var current = new CollectionConfig
        {
            Name = "Test",
            Properties = new List<Property>
            {
                new Property { Name = "prop1", DataType = DataType.Text },
            }.ToArray(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = new List<Property>
            {
                new Property { Name = "prop1", DataType = DataType.Int },
            }.ToArray(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.ModifyPropertyType, changes[0].ChangeType);
        Assert.False(changes[0].IsSafe);
        Assert.Contains("BREAKING", changes[0].Description);
    }

    [Fact]
    public void Compare_UpdatedDescription_DetectsSafeChange()
    {
        // Arrange
        var current = new CollectionConfig
        {
            Name = "Test",
            Description = "Old description",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        var target = new CollectionConfig
        {
            Name = "Test",
            Description = "New description",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.UpdateDescription, changes[0].ChangeType);
        Assert.True(changes[0].IsSafe);
    }

    [Fact]
    public void Compare_AddedReference_DetectsAddition()
    {
        // Arrange
        var current = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = new List<Reference> { new Reference("author", "Author") }.ToArray(),
            VectorConfig = new VectorConfigList { },
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.AddReference, changes[0].ChangeType);
        Assert.True(changes[0].IsSafe);
        Assert.Equal("author", changes[0].Reference?.Name);
    }

    [Fact]
    public void Compare_RemovedReference_DetectsBreakingChange()
    {
        // Arrange
        var current = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = new List<Reference> { new Reference("author", "Author") }.ToArray(),
            VectorConfig = new VectorConfigList { },
        };

        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.RemoveReference, changes[0].ChangeType);
        Assert.False(changes[0].IsSafe);
    }

    [Fact]
    public void Compare_ChangedReplicationFactor_DetectsSafeChange()
    {
        // Arrange
        var current = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
            ReplicationConfig = new ReplicationConfig { Factor = 1 },
        };

        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
            ReplicationConfig = new ReplicationConfig { Factor = 3 },
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.UpdateReplication, changes[0].ChangeType);
        Assert.True(changes[0].IsSafe);
    }

    [Fact]
    public void Compare_NullCurrentConfig_ReturnsAllAdditions()
    {
        // Arrange
        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = new List<Property>
            {
                new Property { Name = "prop1", DataType = DataType.Text },
            }.ToArray(),
            References = new List<Reference> { new Reference("ref1", "Target") }.ToArray(),
            VectorConfig = new VectorConfigList { },
        };

        // Act
        var changes = SchemaDiffer.Compare(null, target);

        // Assert
        Assert.Equal(2, changes.Count);
        Assert.All(changes, c => Assert.True(c.IsSafe));
        Assert.Contains(changes, c => c.ChangeType == SchemaChangeType.AddProperty);
        Assert.Contains(changes, c => c.ChangeType == SchemaChangeType.AddReference);
    }

    [Fact]
    public void Compare_AddedVectorConfig_DetectsAddition()
    {
        // Arrange
        var current = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        var targetVectorConfig = new VectorConfig(
            name: "title_vector",
            vectorIndexConfig: new VectorIndex.HNSW()
        );

        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList(targetVectorConfig),
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.AddVector, changes[0].ChangeType);
        Assert.True(changes[0].IsSafe);
        Assert.Equal("title_vector", changes[0].VectorConfig?.Name);
    }

    [Fact]
    public void Compare_RemovedVectorConfig_DetectsBreakingChange()
    {
        // Arrange
        var currentVectorConfig = new VectorConfig(
            name: "title_vector",
            vectorIndexConfig: new VectorIndex.HNSW()
        );

        var current = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList(currentVectorConfig),
        };

        var target = new CollectionConfig
        {
            Name = "Test",
            Properties = Array.Empty<Property>(),
            References = Array.Empty<Reference>(),
            VectorConfig = new VectorConfigList { },
        };

        // Act
        var changes = SchemaDiffer.Compare(current, target);

        // Assert
        Assert.Single(changes);
        Assert.Equal(SchemaChangeType.RemoveVector, changes[0].ChangeType);
        Assert.False(changes[0].IsSafe);
        Assert.Contains("BREAKING", changes[0].Description);
    }
}
