using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// The test data class
/// </summary>
internal class TestData
{
    /// <summary>
    /// Gets or sets the value of the name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the size
    /// </summary>
    public int Size { get; set; } = 0;
}

/// <summary>
/// The test data value class
/// </summary>
internal class TestDataValue
{
    /// <summary>
    /// Gets or sets the value of the value
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// The filter tests class
/// </summary>
public partial class FilterTests
{
    /// <summary>
    /// The dataset filter array types class
    /// </summary>
    /// <seealso cref="TheoryData{string}"/>
    public class DatasetFilterArrayTypes : TheoryData<string>
    {
        /// <summary>
        /// The is less than equal
        /// </summary>
        public static Dictionary<string, (Filter, int[])> Cases = new()
        {
            ["Test 1"] = (Filter.Property("texts").IsLike("*nana"), new int[] { 1 }),
            ["Test 2"] = (Filter.Property("texts").IsEqual("banana"), new int[] { 1 }),
            ["Test 3"] = (Filter.Property("ints").IsEqual(3), new int[] { 1 }),
            ["Test 4"] = (Filter.Property("ints").IsGreaterThanEqual(3), new int[] { 1, 2 }),
            ["Test 5"] = (Filter.Property("floats").IsEqual(3), new int[] { 1 }),
            ["Test 6"] = (Filter.Property("floats").IsLessThanEqual(3), new int[] { 0, 1 }),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetFilterArrayTypes"/> class
        /// </summary>
        public DatasetFilterArrayTypes()
            : base(Cases.Keys) { }
    }

    // Define test constants
    /// <summary>
    /// The utc now
    /// </summary>
    private static readonly DateTime NOW = DateTime.UtcNow;

    /// <summary>
    /// The add hours
    /// </summary>
    private static readonly DateTime LATER = NOW.AddHours(1);

    /// <summary>
    /// The add days
    /// </summary>
    private static readonly DateTime MUCH_LATER = NOW.AddDays(1);

    /// <summary>
    /// The new guid
    /// </summary>
    private static readonly Guid UUID1 = Guid.NewGuid();

    /// <summary>
    /// The new guid
    /// </summary>
    private static readonly Guid UUID2 = Guid.NewGuid();

    /// <summary>
    /// The new guid
    /// </summary>
    private static readonly Guid UUID3 = Guid.NewGuid();

    /// <summary>
    /// The dataset filter contains class
    /// </summary>
    /// <seealso cref="TheoryData{string}"/>
    public class DatasetFilterContains : TheoryData<string>
    {
        /// <summary>
        /// The uuid
        /// </summary>
        public static Dictionary<string, (Filter, int[])> Cases = new()
        {
            ["ContainsAny ints 1,4"] = (
                Filter.Property("ints").ContainsAny([1, 4]),
                new int[] { 0, 3 }
            ),
            ["ContainsAny ints 1.0,4"] = (
                Filter.Property("ints").ContainsAny([1.0, 4]),
                new int[] { 0, 3 }
            ),
            ["ContainsAny ints 10"] = (Filter.Property("ints").ContainsAny([10]), new int[] { }),
            ["ContainsAny int 1"] = (Filter.Property("int").ContainsAny([1]), new int[] { 0, 1 }),
            ["ContainsAny text test"] = (
                Filter.Property("text").ContainsAny(["test"]),
                new int[] { 0, 1 }
            ),
            ["ContainsAny text real,deal"] = (
                Filter.Property("text").ContainsAny(["real", "deal"]),
                new int[] { 1, 2, 3 }
            ),
            ["ContainsAny texts test"] = (
                Filter.Property("texts").ContainsAny(["test"]),
                new int[] { 0, 1 }
            ),
            ["ContainsAny texts real,deal"] = (
                Filter.Property("texts").ContainsAny(["real", "deal"]),
                new int[] { 1, 2, 3 }
            ),
            ["ContainsAny float 2.0"] = (
                Filter.Property("float").ContainsAny([2.0]),
                new int[] { }
            ),
            ["ContainsAny float 2"] = (Filter.Property("float").ContainsAny([2]), new int[] { }),
            ["ContainsAny float 8"] = (Filter.Property("float").ContainsAny([8]), new int[] { 3 }),
            ["ContainsAny float 8.0"] = (
                Filter.Property("float").ContainsAny([8.0]),
                new int[] { 3 }
            ),
            ["ContainsAny floats 2.0"] = (
                Filter.Property("floats").ContainsAny([2.0]),
                new int[] { 0, 1 }
            ),
            ["ContainsAny floats 0.4,0.7"] = (
                Filter.Property("floats").ContainsAny([0.4, 0.7]),
                new int[] { 0, 1, 3 }
            ),
            ["ContainsAny floats 2"] = (
                Filter.Property("floats").ContainsAny([2]),
                new int[] { 0, 1 }
            ),
            ["ContainsAny bools true,false"] = (
                Filter.Property("bools").ContainsAny([true, false]),
                new int[] { 0, 1, 3 }
            ),
            ["ContainsAny bools false"] = (
                Filter.Property("bools").ContainsAny([false]),
                new int[] { 0, 1 }
            ),
            ["ContainsAny bool true"] = (
                Filter.Property("bool").ContainsAny([true]),
                new int[] { 0, 1, 3 }
            ),
            ["ContainsAll ints 1,4"] = (
                Filter.Property("ints").ContainsAll([1, 4]),
                new int[] { 0 }
            ),
            ["ContainsAll text real,test"] = (
                Filter.Property("text").ContainsAll(["real", "test"]),
                new int[] { 1 }
            ),
            ["ContainsAll texts real,test"] = (
                Filter.Property("texts").ContainsAll(["real", "test"]),
                new int[] { 1 }
            ),
            ["ContainsAll floats 0.7,2"] = (
                Filter.Property("floats").ContainsAll([0.7, 2]),
                new int[] { 1 }
            ),
            ["ContainsAll bools true,false"] = (
                Filter.Property("bools").ContainsAll([true, false]),
                new int[] { 0 }
            ),
            ["ContainsAll bool true,false"] = (
                Filter.Property("bool").ContainsAll([true, false]),
                new int[] { }
            ),
            ["ContainsAll bool true"] = (
                Filter.Property("bool").ContainsAll([true]),
                new int[] { 0, 1, 3 }
            ),
            ["ContainsAny dates now,much_later"] = (
                Filter.Property("dates").ContainsAny([NOW, MUCH_LATER]),
                new int[] { 0, 1, 3 }
            ),
            ["ContainsAny dates now"] = (
                Filter.Property("dates").ContainsAny([NOW]),
                new int[] { 0, 1 }
            ),
            ["Equal date now"] = (Filter.Property("date").IsEqual(NOW), new int[] { 0 }),
            ["GreaterThan date now"] = (
                Filter.Property("date").IsGreaterThan(NOW),
                new int[] { 1, 3 }
            ),
            ["ContainsAll uuids uuid2,uuid1"] = (
                Filter.Property("uuids").ContainsAll([UUID2, UUID1]),
                new int[] { 0, 3 }
            ),
            ["ContainsAny uuids uuid2,uuid1"] = (
                Filter.Property("uuids").ContainsAny([UUID2, UUID1]),
                new int[] { 0, 1, 3 }
            ),
            ["ContainsAny uuid uuid3"] = (
                Filter.Property("uuid").ContainsAny([UUID3]),
                new int[] { }
            ),
            ["ContainsAny uuid uuid1"] = (
                Filter.Property("uuid").ContainsAny([UUID1]),
                new int[] { 0 }
            ),
            ["ContainsAny _id uuid1,uuid3"] = (
                Filter.Property("_id").ContainsAny([UUID1, UUID3]),
                new int[] { 0, 2 }
            ),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetFilterContains"/> class
        /// </summary>
        public DatasetFilterContains()
            : base(Cases.Keys) { }
    }

    /// <summary>
    /// The dataset ref count filter class
    /// </summary>
    /// <seealso cref="TheoryData{string}"/>
    public class DatasetRefCountFilter : TheoryData<string>
    {
        /// <summary>
        /// Gets the value of the cases
        /// </summary>
        public static Dictionary<string, (Filter, int[])> Cases =>
            new()
            {
                ["NotEqual"] = (Filter.Reference("ref").Count.IsNotEqual(1), [0, 2]),
                ["LessThan"] = (Filter.Reference("ref").Count.IsLessThan(2), [0, 1]),
                ["LessThanEqual"] = (Filter.Reference("ref").Count.IsLessThanEqual(1), [0, 1]),
                ["GreaterThan"] = (Filter.Reference("ref").Count.IsGreaterThan(0), [1, 2]),
                ["GreaterThanEqual"] = (
                    Filter.Reference("ref").Count.IsGreaterThanEqual(1),
                    [1, 2]
                ),
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetRefCountFilter"/> class
        /// </summary>
        public DatasetRefCountFilter()
            : base(Cases.Keys) { }
    }

    /// <summary>
    /// The dataset filtering references class
    /// </summary>
    /// <seealso cref="TheoryData{string}"/>
    public class DatasetFilteringReferences : TheoryData<string>
    {
        /// <summary>
        /// Gets the value of the cases
        /// </summary>
        public static Dictionary<string, (Filter, int)> Cases =>
            new()
            {
                ["RefPropertyGreaterThan"] = (
                    Filter.Reference("ref").Property("size").IsGreaterThan(3),
                    1
                ),
                ["RefPropertyLengthLessThan6"] = (
                    Filter.Reference("ref").Property("name").HasLength().IsLessThan(6),
                    0
                ),
                ["RefIDEquals"] = (Filter.Reference("ref").UUID.IsEqual(_reusableUuids[1]), 1),
                ["IndirectSelfRefLengthLessThan6"] = (
                    Filter
                        .Reference("ref2")
                        .Reference("ref")
                        .Property("name")
                        .HasLength()
                        .IsLessThan(6),
                    2
                ),
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetFilteringReferences"/> class
        /// </summary>
        public DatasetFilteringReferences()
            : base(Cases.Keys) { }
    }

    /// <summary>
    /// The dataset filter by id class
    /// </summary>
    /// <seealso cref="TheoryData{string}"/>
    public class DatasetFilterByID : TheoryData<string>
    {
        /// <summary>
        /// Gets the value of the cases
        /// </summary>
        public static Dictionary<string, Filter> Cases =>
            new()
            {
                ["IdEquals"] = Filter.UUID.IsEqual(_reusableUuids[0]),
                ["IdContainsAny"] = Filter.UUID.ContainsAny([_reusableUuids[0]]),
                ["IdNotEqual"] = Filter.UUID.IsNotEqual(_reusableUuids[1]),
                ["IdWithProperty(_id)Equal"] = Filter.Property("_id").IsEqual(_reusableUuids[0]),
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetFilterByID"/> class
        /// </summary>
        public DatasetFilterByID()
            : base(Cases.Keys) { }
    }

    /// <summary>
    /// The dataset time filter class
    /// </summary>
    /// <seealso cref="TheoryData{string}"/>
    public class DatasetTimeFilter : TheoryData<string>
    {
        /// <summary>
        /// Gets the value of the cases
        /// </summary>
        public static Dictionary<
            string,
            (int filterValue, int[] results, Func<DateTime, Filter> filterFunc)
        > Cases =>
            new()
            {
                ["Equal"] = (2, new int[] { 2 }, dt => Filter.CreationTime.IsEqual(dt)),
                ["NotEqual"] = (1, new int[] { 0, 2 }, dt => Filter.CreationTime.IsNotEqual(dt)),
                ["GreaterThan"] = (1, new int[] { 2 }, dt => Filter.CreationTime.IsGreaterThan(dt)),
                ["GreaterOrEqual"] = (
                    1,
                    new int[] { 1, 2 },
                    dt => Filter.CreationTime.IsGreaterThanEqual(dt)
                ),
                ["LessThan"] = (1, new int[] { 0 }, dt => Filter.CreationTime.IsLessThan(dt)),
                ["LessOrEqual"] = (
                    1,
                    new int[] { 0, 1 },
                    dt => Filter.CreationTime.IsLessThanEqual(dt)
                ),
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetTimeFilter"/> class
        /// </summary>
        public DatasetTimeFilter()
            : base(Cases.Keys) { }
    }
}

/// <summary>
/// The batch tests class
/// </summary>
public partial class BatchTests
{
    /// <summary>
    /// The dataset batch insert many class
    /// </summary>
    /// <seealso cref="TheoryData{string}"/>
    public class DatasetBatchInsertMany : TheoryData<string>
    {
        /// <summary>
        /// Gets the value of the cases
        /// </summary>
        public static Dictionary<
            string,
            (
                int expectedObjects,
                int expectedErrors,
                int expectedReferences,
                int expectedReferencedObjects,
                IEnumerable<BatchInsertRequest[]> data
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
                        [
                            BatchInsertRequest.Create(
                                new { Name = "some name" },
                                vectors: new int[] { 1, 2, 3 }
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "some other name" },
                                uuid: _reusableUuids[0]
                            ),
                        ],
                    ]
                ),
                ["all data types"] = (
                    1,
                    0,
                    0,
                    0,
                    [
                        [
                            BatchInsertRequest.Create(
                                new
                                {
                                    Name = "some name",
                                    Size = 3,
                                    Price = 10.5,
                                    IsAvailable = true,
                                    AvailableSince = new DateTime(2023, 1, 1),
                                }
                            ),
                        ],
                    ]
                ),
                ["wrong type for property"] = (
                    0,
                    1,
                    0,
                    0,
                    [
                        [BatchInsertRequest.Create(new { Name = 1 })],
                    ]
                ),
                ["batch with self-reference"] = (
                    5,
                    0,
                    1,
                    1,
                    [
                        [
                            BatchInsertRequest.Create(
                                new { Name = "Name 1" },
                                uuid: _reusableUuids[0]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 2" },
                                uuid: _reusableUuids[1]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 3" },
                                uuid: _reusableUuids[2]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 4" },
                                uuid: _reusableUuids[3]
                            ),
                        ],
                        [
                            BatchInsertRequest.Create(
                                new { Name = "Name 5" },
                                references: [new("ref", _reusableUuids[1])]
                            ),
                        ],
                    ]
                ),
                ["batch with multiple self-references"] = (
                    5,
                    0,
                    1,
                    2,
                    [
                        [
                            BatchInsertRequest.Create(
                                new { Name = "Name 1" },
                                uuid: _reusableUuids[0]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 2" },
                                uuid: _reusableUuids[1]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 3" },
                                uuid: _reusableUuids[2]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 4" },
                                uuid: _reusableUuids[3]
                            ),
                        ],
                        [
                            BatchInsertRequest.Create(
                                new { Name = "Name 5" },
                                references: [new("ref", _reusableUuids[1], _reusableUuids[2])]
                            ),
                        ],
                    ]
                ),
                ["batch with multiple self-reference properties"] = (
                    7,
                    0,
                    3,
                    4,
                    [
                        [
                            BatchInsertRequest.Create(
                                new { Name = "Name 1" },
                                uuid: _reusableUuids[0]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 2" },
                                uuid: _reusableUuids[1]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 3" },
                                uuid: _reusableUuids[2]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 4" },
                                uuid: _reusableUuids[3]
                            ),
                        ],
                        [
                            BatchInsertRequest.Create(
                                new { Name = "Name 5" },
                                references: [new("ref", _reusableUuids[1])]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 6" },
                                references: [new("ref2", _reusableUuids[2])]
                            ),
                            BatchInsertRequest.Create(
                                new { Name = "Name 7" },
                                references:
                                [
                                    new("ref", _reusableUuids[1]),
                                    new("ref2", _reusableUuids[2]),
                                ]
                            ),
                        ],
                    ]
                ),
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetBatchInsertMany"/> class
        /// </summary>
        public DatasetBatchInsertMany()
            : base(Cases.Keys) { }
    }
}
