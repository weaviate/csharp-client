using Weaviate.Client.Models;
using Weaviate.Client.Orm.Migrations;
using Xunit;

namespace Weaviate.Client.Orm.Tests.Migrations;

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
}
