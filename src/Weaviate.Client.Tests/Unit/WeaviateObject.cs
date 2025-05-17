using Weaviate.Client.Models;

namespace Weaviate.Client.Tests;

public partial class UnitTests
{
    [Fact]
    public void DynamicPropertyAccess()
    {
        WeaviateObject obj = new()
        {
            Collection = "DynamicPropertyAccess",
            Properties = new Dictionary<string, object?>
            {
                { "Name", "John" },
                { "Age", 30 },
                {
                    "Address",
                    new Dictionary<string, object?>
                    {
                        { "Street", "123 Main St" },
                        { "City", "Anytown" },
                    }
                },
            },
        };

        var result = obj.Get(x => x.Name);
        Assert.Equal("John", result);

        obj.Do(x =>
        {
            Assert.Equal("John", x.Name);
        });
    }
}
