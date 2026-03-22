using Weaviate.Client.Internal;

namespace Weaviate.Client.Tests.Unit;

public class TestBatchStreamTypes
{
    [Fact]
    public void BatchStreamError_WithUuid_IsReferenceIsFalse()
    {
        var error = new BatchStreamError { UUID = Guid.NewGuid(), Error = "oops" };
        Assert.False(error.IsReference);
        Assert.Null(error.Beacon);
    }

    [Fact]
    public void BatchStreamError_WithBeacon_IsReferenceIsTrue()
    {
        var error = new BatchStreamError
        {
            Beacon = "weaviate://localhost/A/uuid/prop",
            Error = "oops",
        };
        Assert.True(error.IsReference);
        Assert.Null(error.UUID);
    }

    [Fact]
    public void BatchStreamSuccess_WithUuid_IsReferenceIsFalse()
    {
        var success = new BatchStreamSuccess { UUID = Guid.NewGuid() };
        Assert.False(success.IsReference);
        Assert.Null(success.Beacon);
    }

    [Fact]
    public void BatchStreamSuccess_WithBeacon_IsReferenceIsTrue()
    {
        var success = new BatchStreamSuccess { Beacon = "weaviate://localhost/A/uuid/prop" };
        Assert.True(success.IsReference);
        Assert.Null(success.UUID);
    }
}
