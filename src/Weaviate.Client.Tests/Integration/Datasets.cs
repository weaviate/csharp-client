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

    public class DatasetBatchInsertMany : TheoryData<string>
    {
        public static Dictionary<
            string,
            (
                int expectedObjects,
                int expectedErrors,
                int expectedReferences,
                int expectedReferencedObjects,
                Action<DataClient<dynamic>.InsertDelegate>[] batcher
            )
        > Cases =>
            new()
            {
                ["2 simple objects, no errors"] = (
                    2,
                    0,
                    0,
                    0,
                    [
                        add =>
                        {
                            add(
                                new { Name = "some name" },
                                vectors: new() { { "default", 1, 2, 3 } }
                            );
                            add(new { Name = "some other name" }, id: _reusableUuids[0]);
                        },
                    ]
                ),
                ["all data types"] = (
                    1,
                    0,
                    0,
                    0,
                    [
                        add =>
                        {
                            add(
                                new
                                {
                                    Name = "some name",
                                    Size = 3,
                                    Price = 10.5,
                                    IsAvailable = true,
                                    AvailableSince = new DateTime(2023, 1, 1),
                                }
                            );
                        },
                    ]
                ),
                ["wrong type for property"] = (
                    0,
                    1,
                    0,
                    0,
                    [
                        add =>
                        {
                            add(new { Name = 1 });
                        },
                    ]
                ),
                ["batch with self-reference"] = (
                    5,
                    0,
                    1,
                    1,
                    [
                        add =>
                        {
                            add(new { Name = "Name 1" }, id: _reusableUuids[0]);
                            add(new { Name = "Name 2" }, id: _reusableUuids[1]);
                            add(new { Name = "Name 3" }, id: _reusableUuids[2]);
                            add(new { Name = "Name 4" }, id: _reusableUuids[3]);
                        },
                        add =>
                        {
                            add(
                                new { Name = "Name 5" },
                                references: [new ObjectReference("ref", _reusableUuids[1])]
                            );
                        },
                    ]
                ),
                ["batch with multiple self-references"] = (
                    5,
                    0,
                    1,
                    2,
                    [
                        add =>
                        {
                            add(new { Name = "Name 1" }, id: _reusableUuids[0]);
                            add(new { Name = "Name 2" }, id: _reusableUuids[1]);
                            add(new { Name = "Name 3" }, id: _reusableUuids[2]);
                            add(new { Name = "Name 4" }, id: _reusableUuids[3]);
                        },
                        add =>
                        {
                            add(
                                new { Name = "Name 5" },
                                references:
                                [
                                    new ObjectReference(
                                        "ref",
                                        _reusableUuids[1],
                                        _reusableUuids[2]
                                    ),
                                ]
                            );
                        },
                    ]
                ),
                ["batch with multiple self-reference properties"] = (
                    7,
                    0,
                    3,
                    4,
                    [
                        add =>
                        {
                            add(new { Name = "Name 1" }, id: _reusableUuids[0]);
                            add(new { Name = "Name 2" }, id: _reusableUuids[1]);
                            add(new { Name = "Name 3" }, id: _reusableUuids[2]);
                            add(new { Name = "Name 4" }, id: _reusableUuids[3]);
                        },
                        add =>
                        {
                            add(
                                new { Name = "Name 5" },
                                references: [new("ref", _reusableUuids[1])]
                            );
                            add(
                                new { Name = "Name 6" },
                                references: [new("ref2", _reusableUuids[2])]
                            );
                            add(
                                new { Name = "Name 7" },
                                references:
                                [
                                    new("ref", _reusableUuids[1]),
                                    new("ref2", _reusableUuids[2]),
                                ]
                            );
                        },
                    ]
                ),
            };

        public DatasetBatchInsertMany()
            : base(Cases.Keys) { }
    }
}
