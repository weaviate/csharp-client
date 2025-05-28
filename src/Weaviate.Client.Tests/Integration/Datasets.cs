using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
    public class DatasetRefCountFilter : TheoryData<string>
    {
        public static Dictionary<string, (Filter, int[])> Cases =>
            new()
            {
                ["NotEqual"] = (Filter.Reference("ref").Count.NotEqual(1), [0, 2]),
                ["LessThan"] = (Filter.Reference("ref").Count.LessThan(2), [0, 1]),
                ["LessThanEqual"] = (Filter.Reference("ref").Count.LessThanEqual(1), [0, 1]),
                ["GreaterThan"] = (Filter.Reference("ref").Count.GreaterThan(0), [1, 2]),
                ["GreaterThanEqual"] = (Filter.Reference("ref").Count.GreaterThanEqual(1), [1, 2]),
            };

        public DatasetRefCountFilter()
            : base(Cases.Keys) { }
    }

    public class DatasetFilteringReferences : TheoryData<string>
    {
        public static Dictionary<string, (Filter, int)> Cases =>
            new()
            {
                ["RefPropertyGreaterThan"] = (
                    Filter.Reference("ref").Property("size").GreaterThan(3),
                    1
                ),
                ["RefPropertyLengthLessThan6"] = (
                    Filter.Reference("ref").Property("name").Length.LessThan(6),
                    0
                ),
                ["RefIDEquals"] = (Filter.Reference("ref").ID.Equal(_reusableUuids[1]), 1),
                ["IndirectSelfRefLengthLessThan6"] = (
                    Filter.Reference("ref2").Reference("ref").Property("name").Length.LessThan(6),
                    2
                ),
            };

        public DatasetFilteringReferences()
            : base(Cases.Keys) { }
    }

    public class DatasetFilterByID : TheoryData<string>
    {
        public static Dictionary<string, Filter> Cases =>
            new()
            {
                ["IdEquals"] = Filter.ID.Equal(_reusableUuids[0]),
                ["IdContainsAny"] = Filter.ID.ContainsAny([_reusableUuids[0]]),
                ["IdNotEqual"] = Filter.ID.NotEqual(_reusableUuids[1]),
                ["IdWithProperty(_id)Equal"] = Filter.Property("_id").Equal(_reusableUuids[0]),
            };

        public DatasetFilterByID()
            : base(Cases.Keys) { }
    }

    public class DatasetTimeFilter : TheoryData<string>
    {
        public static Dictionary<
            string,
            (int filterValue, int[] results, Func<DateTime, Filter> filterFunc)
        > Cases =>
            new()
            {
                ["Equal"] = (2, new int[] { 2 }, dt => Filter.CreationTime.Equal(dt)),
                ["NotEqual"] = (1, new int[] { 0, 2 }, dt => Filter.CreationTime.NotEqual(dt)),
                ["GreaterThan"] = (1, new int[] { 2 }, dt => Filter.CreationTime.GreaterThan(dt)),
                ["GreaterOrEqual"] = (
                    1,
                    new int[] { 1, 2 },
                    dt => Filter.CreationTime.GreaterThanEqual(dt)
                ),
                ["LessThan"] = (1, new int[] { 0 }, dt => Filter.CreationTime.LessThan(dt)),
                ["LessOrEqual"] = (
                    1,
                    new int[] { 0, 1 },
                    dt => Filter.CreationTime.LessThanEqual(dt)
                ),
            };

        public DatasetTimeFilter()
            : base(Cases.Keys) { }
    }
}
