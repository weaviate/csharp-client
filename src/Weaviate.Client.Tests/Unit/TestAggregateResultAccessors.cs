using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The test aggregate result accessors class
/// </summary>
public class TestAggregateResultAccessors
{
    /// <summary>
    /// Creates the test result
    /// </summary>
    /// <returns>The aggregate result</returns>
    private static AggregateResult CreateTestResult()
    {
        return new AggregateResult
        {
            TotalCount = 100,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["textField"] = new Aggregate.Text
                {
                    Count = 10,
                    TopOccurrences =
                    [
                        new Aggregate.TopOccurrence<string> { Value = "hello", Count = 5 },
                        new Aggregate.TopOccurrence<string> { Value = "world", Count = 3 },
                    ],
                },
                ["intField"] = new Aggregate.Integer
                {
                    Count = 20,
                    Minimum = 1,
                    Maximum = 100,
                    Mean = 50.5,
                    Median = 50,
                    Mode = 42,
                    Sum = 1000,
                },
                ["numberField"] = new Aggregate.Number
                {
                    Count = 15,
                    Minimum = 1.5,
                    Maximum = 99.5,
                    Mean = 50.0,
                    Median = 49.5,
                    Mode = 42.0,
                    Sum = 750.0,
                },
                ["boolField"] = new Aggregate.Boolean
                {
                    Count = 50,
                    TotalTrue = 30,
                    TotalFalse = 20,
                    PercentageTrue = 0.6,
                    PercentageFalse = 0.4,
                },
                ["dateField"] = new Aggregate.Date
                {
                    Count = 25,
                    Minimum = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Maximum = new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Median = new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    Mode = new DateTime(2023, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                },
            },
        };
    }

    #region Typed Accessor Tests

    /// <summary>
    /// Tests that text returns text aggregation when property exists
    /// </summary>
    [Fact]
    public void Text_ReturnsTextAggregation_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var text = result.Text("textField");

        Assert.NotNull(text);
        Assert.Equal(10, text.Count);
        Assert.Equal(2, text.TopOccurrences.Count);
        Assert.Equal("hello", text.TopOccurrences[0].Value);
    }

    /// <summary>
    /// Tests that text returns null when property does not exist
    /// </summary>
    [Fact]
    public void Text_ReturnsNull_WhenPropertyDoesNotExist()
    {
        var result = CreateTestResult();

        var text = result.Text("nonExistent");

        Assert.Null(text);
    }

    /// <summary>
    /// Tests that text returns null when property is wrong type
    /// </summary>
    [Fact]
    public void Text_ReturnsNull_WhenPropertyIsWrongType()
    {
        var result = CreateTestResult();

        var text = result.Text("intField");

        Assert.Null(text);
    }

    /// <summary>
    /// Tests that integer returns integer aggregation when property exists
    /// </summary>
    [Fact]
    public void Integer_ReturnsIntegerAggregation_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var integer = result.Integer("intField");

        Assert.NotNull(integer);
        Assert.Equal(20, integer.Count);
        Assert.Equal(1, integer.Minimum);
        Assert.Equal(100, integer.Maximum);
        Assert.Equal(50.5, integer.Mean);
    }

    /// <summary>
    /// Tests that number returns number aggregation when property exists
    /// </summary>
    [Fact]
    public void Number_ReturnsNumberAggregation_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var number = result.Number("numberField");

        Assert.NotNull(number);
        Assert.Equal(15, number.Count);
        Assert.Equal(1.5, number.Minimum);
        Assert.Equal(99.5, number.Maximum);
    }

    /// <summary>
    /// Tests that boolean returns boolean aggregation when property exists
    /// </summary>
    [Fact]
    public void Boolean_ReturnsBooleanAggregation_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var boolean = result.Boolean("boolField");

        Assert.NotNull(boolean);
        Assert.Equal(50, boolean.Count);
        Assert.Equal(30, boolean.TotalTrue);
        Assert.Equal(20, boolean.TotalFalse);
        Assert.Equal(0.6, boolean.PercentageTrue);
    }

    /// <summary>
    /// Tests that date returns date aggregation when property exists
    /// </summary>
    [Fact]
    public void Date_ReturnsDateAggregation_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var date = result.Date("dateField");

        Assert.NotNull(date);
        Assert.Equal(25, date.Count);
        Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), date.Minimum);
        Assert.Equal(new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc), date.Maximum);
    }

    #endregion

    #region TryGet Tests

    /// <summary>
    /// Tests that try get text returns true when property exists
    /// </summary>
    [Fact]
    public void TryGetText_ReturnsTrue_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var success = result.TryGetText("textField", out var text);

        Assert.True(success);
        Assert.NotNull(text);
        Assert.Equal(10, text.Count);
    }

    /// <summary>
    /// Tests that try get text returns false when property does not exist
    /// </summary>
    [Fact]
    public void TryGetText_ReturnsFalse_WhenPropertyDoesNotExist()
    {
        var result = CreateTestResult();

        var success = result.TryGetText("nonExistent", out _);

        Assert.False(success);
    }

    /// <summary>
    /// Tests that try get text returns false when property is wrong type
    /// </summary>
    [Fact]
    public void TryGetText_ReturnsFalse_WhenPropertyIsWrongType()
    {
        var result = CreateTestResult();

        var success = result.TryGetText("intField", out _);

        Assert.False(success);
    }

    /// <summary>
    /// Tests that try get integer returns true when property exists
    /// </summary>
    [Fact]
    public void TryGetInteger_ReturnsTrue_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var success = result.TryGetInteger("intField", out var integer);

        Assert.True(success);
        Assert.NotNull(integer);
        Assert.Equal(20, integer.Count);
    }

    /// <summary>
    /// Tests that try get number returns true when property exists
    /// </summary>
    [Fact]
    public void TryGetNumber_ReturnsTrue_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var success = result.TryGetNumber("numberField", out var number);

        Assert.True(success);
        Assert.NotNull(number);
        Assert.Equal(15, number.Count);
    }

    /// <summary>
    /// Tests that try get boolean returns true when property exists
    /// </summary>
    [Fact]
    public void TryGetBoolean_ReturnsTrue_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var success = result.TryGetBoolean("boolField", out var boolean);

        Assert.True(success);
        Assert.NotNull(boolean);
        Assert.Equal(50, boolean.Count);
    }

    /// <summary>
    /// Tests that try get date returns true when property exists
    /// </summary>
    [Fact]
    public void TryGetDate_ReturnsTrue_WhenPropertyExists()
    {
        var result = CreateTestResult();

        var success = result.TryGetDate("dateField", out var date);

        Assert.True(success);
        Assert.NotNull(date);
        Assert.Equal(25, date.Count);
    }

    /// <summary>
    /// Tests that try get generic returns true when property exists and matches type
    /// </summary>
    [Fact]
    public void TryGet_Generic_ReturnsTrue_WhenPropertyExistsAndMatchesType()
    {
        var result = CreateTestResult();

        var success = result.TryGet<Aggregate.Text>("textField", out var text);

        Assert.True(success);
        Assert.NotNull(text);
        Assert.Equal(10, text.Count);
    }

    /// <summary>
    /// Tests that try get generic returns false when property is wrong type
    /// </summary>
    [Fact]
    public void TryGet_Generic_ReturnsFalse_WhenPropertyIsWrongType()
    {
        var result = CreateTestResult();

        var success = result.TryGet<Aggregate.Text>("intField", out _);

        Assert.False(success);
    }

    #endregion

    #region Property Lambda Tests

    /// <summary>
    /// Tests that property action executes action when property matches
    /// </summary>
    [Fact]
    public void Property_Action_ExecutesAction_WhenPropertyMatches()
    {
        var result = CreateTestResult();
        long? capturedCount = null;

        var matched = result.Property<Aggregate.Text>("textField", t => capturedCount = t.Count);

        Assert.True(matched);
        Assert.Equal(10, capturedCount);
    }

    /// <summary>
    /// Tests that property action returns false when property does not match
    /// </summary>
    [Fact]
    public void Property_Action_ReturnsFalse_WhenPropertyDoesNotMatch()
    {
        var result = CreateTestResult();
        var executed = false;

        var matched = result.Property<Aggregate.Text>("intField", _ => executed = true);

        Assert.False(matched);
        Assert.False(executed);
    }

    /// <summary>
    /// Tests that property func returns result when property matches
    /// </summary>
    [Fact]
    public void Property_Func_ReturnsResult_WhenPropertyMatches()
    {
        var result = CreateTestResult();

        var count = result.Property<Aggregate.Text, long?>("textField", t => t.Count);

        Assert.Equal(10, count);
    }

    /// <summary>
    /// Tests that property func returns default when property does not match
    /// </summary>
    [Fact]
    public void Property_Func_ReturnsDefault_WhenPropertyDoesNotMatch()
    {
        var result = CreateTestResult();

        var count = result.Property<Aggregate.Text, long?>("intField", t => t.Count);

        Assert.Null(count);
    }

    /// <summary>
    /// Tests that property func with value type returns value when matches
    /// </summary>
    [Fact]
    public void Property_Func_WithValueType_ReturnsValue_WhenMatches()
    {
        var result = CreateTestResult();

        var minimum = result.Property<Aggregate.Integer, long?>("intField", i => i.Minimum);

        Assert.Equal(1, minimum);
    }

    #endregion

    #region Match Tests

    /// <summary>
    /// Tests that match action executes correct action for text property
    /// </summary>
    [Fact]
    public void Match_Action_ExecutesCorrectAction_ForTextProperty()
    {
        var result = CreateTestResult();
        string? capturedType = null;

        var matched = result.Match(
            "textField",
            text: _ =>
            {
                capturedType = "text";
            },
            integer: _ =>
            {
                capturedType = "integer";
            },
            number: _ =>
            {
                capturedType = "number";
            },
            boolean: _ =>
            {
                capturedType = "boolean";
            },
            date: _ =>
            {
                capturedType = "date";
            }
        );

        Assert.True(matched);
        Assert.Equal("text", capturedType);
    }

    /// <summary>
    /// Tests that match action executes correct action for integer property
    /// </summary>
    [Fact]
    public void Match_Action_ExecutesCorrectAction_ForIntegerProperty()
    {
        var result = CreateTestResult();
        string? capturedType = null;

        var matched = result.Match(
            "intField",
            text: _ =>
            {
                capturedType = "text";
            },
            integer: _ =>
            {
                capturedType = "integer";
            }
        );

        Assert.True(matched);
        Assert.Equal("integer", capturedType);
    }

    /// <summary>
    /// Tests that match action executes correct action for number property
    /// </summary>
    [Fact]
    public void Match_Action_ExecutesCorrectAction_ForNumberProperty()
    {
        var result = CreateTestResult();
        string? capturedType = null;

        var matched = result.Match(
            "numberField",
            number: _ =>
            {
                capturedType = "number";
            }
        );

        Assert.True(matched);
        Assert.Equal("number", capturedType);
    }

    /// <summary>
    /// Tests that match action executes correct action for boolean property
    /// </summary>
    [Fact]
    public void Match_Action_ExecutesCorrectAction_ForBooleanProperty()
    {
        var result = CreateTestResult();
        string? capturedType = null;

        var matched = result.Match(
            "boolField",
            boolean: _ =>
            {
                capturedType = "boolean";
            }
        );

        Assert.True(matched);
        Assert.Equal("boolean", capturedType);
    }

    /// <summary>
    /// Tests that match action executes correct action for date property
    /// </summary>
    [Fact]
    public void Match_Action_ExecutesCorrectAction_ForDateProperty()
    {
        var result = CreateTestResult();
        string? capturedType = null;

        var matched = result.Match(
            "dateField",
            date: _ =>
            {
                capturedType = "date";
            }
        );

        Assert.True(matched);
        Assert.Equal("date", capturedType);
    }

    /// <summary>
    /// Tests that match action returns false when property does not exist
    /// </summary>
    [Fact]
    public void Match_Action_ReturnsFalse_WhenPropertyDoesNotExist()
    {
        var result = CreateTestResult();
        var executed = false;

        var matched = result.Match(
            "nonExistent",
            text: _ =>
            {
                executed = true;
            },
            integer: _ =>
            {
                executed = true;
            }
        );

        Assert.False(matched);
        Assert.False(executed);
    }

    /// <summary>
    /// Tests that match action returns false when no matching handler
    /// </summary>
    [Fact]
    public void Match_Action_ReturnsFalse_WhenNoMatchingHandler()
    {
        var result = CreateTestResult();
        var executed = false;

        // textField exists but we only provide integer handler
        var matched = result.Match(
            "textField",
            integer: _ =>
            {
                executed = true;
            }
        );

        Assert.False(matched);
        Assert.False(executed);
    }

    /// <summary>
    /// Tests that match func returns correct value for text property
    /// </summary>
    [Fact]
    public void Match_Func_ReturnsCorrectValue_ForTextProperty()
    {
        var result = CreateTestResult();

        var description = result.Match(
            "textField",
            text: t => $"Text with {t.Count} items",
            integer: i => $"Integer range [{i.Minimum}, {i.Maximum}]"
        );

        Assert.Equal("Text with 10 items", description);
    }

    /// <summary>
    /// Tests that match func returns correct value for integer property
    /// </summary>
    [Fact]
    public void Match_Func_ReturnsCorrectValue_ForIntegerProperty()
    {
        var result = CreateTestResult();

        var description = result.Match(
            "intField",
            text: t => $"Text with {t.Count} items",
            integer: i => $"Integer range [{i.Minimum}, {i.Maximum}]"
        );

        Assert.Equal("Integer range [1, 100]", description);
    }

    /// <summary>
    /// Tests that match func returns default when property does not exist
    /// </summary>
    [Fact]
    public void Match_Func_ReturnsDefault_WhenPropertyDoesNotExist()
    {
        var result = CreateTestResult();

        var description = result.Match(
            "nonExistent",
            text: t => $"Text: {t.Count}",
            integer: i => $"Integer: {i.Count}"
        );

        Assert.Null(description);
    }

    /// <summary>
    /// Tests that match func returns default when no matching handler
    /// </summary>
    [Fact]
    public void Match_Func_ReturnsDefault_WhenNoMatchingHandler()
    {
        var result = CreateTestResult();

        var description = result.Match("textField", integer: i => $"Integer: {i.Count}");

        Assert.Null(description);
    }

    #endregion

    #region GroupByResult Tests

    /// <summary>
    /// Creates the test group by result
    /// </summary>
    /// <returns>The aggregate group by result</returns>
    private static AggregateGroupByResult CreateTestGroupByResult()
    {
        return new AggregateGroupByResult
        {
            Groups =
            [
                new AggregateGroupByResult.Group
                {
                    GroupedBy = new AggregateGroupByResult.Group.By(
                        "category",
                        "A",
                        typeof(string)
                    ),
                    TotalCount = 50,
                    Properties = new Dictionary<string, Aggregate.Property>
                    {
                        ["textField"] = new Aggregate.Text
                        {
                            Count = 5,
                            TopOccurrences =
                            [
                                new Aggregate.TopOccurrence<string> { Value = "test", Count = 3 },
                            ],
                        },
                        ["intField"] = new Aggregate.Integer
                        {
                            Count = 10,
                            Minimum = 1,
                            Maximum = 50,
                        },
                    },
                },
                new AggregateGroupByResult.Group
                {
                    GroupedBy = new AggregateGroupByResult.Group.By(
                        "category",
                        "B",
                        typeof(string)
                    ),
                    TotalCount = 30,
                    Properties = new Dictionary<string, Aggregate.Property>
                    {
                        ["textField"] = new Aggregate.Text
                        {
                            Count = 3,
                            TopOccurrences =
                            [
                                new Aggregate.TopOccurrence<string> { Value = "other", Count = 2 },
                            ],
                        },
                    },
                },
            ],
        };
    }

    /// <summary>
    /// Tests that group text returns text aggregation when property exists
    /// </summary>
    [Fact]
    public void Group_Text_ReturnsTextAggregation_WhenPropertyExists()
    {
        var result = CreateTestGroupByResult();
        var group = result.Groups[0];

        var text = group.Text("textField");

        Assert.NotNull(text);
        Assert.Equal(5, text.Count);
    }

    /// <summary>
    /// Tests that group try get text returns true when property exists
    /// </summary>
    [Fact]
    public void Group_TryGetText_ReturnsTrue_WhenPropertyExists()
    {
        var result = CreateTestGroupByResult();
        var group = result.Groups[0];

        var success = group.TryGetText("textField", out var text);

        Assert.True(success);
        Assert.NotNull(text);
        Assert.Equal(5, text.Count);
    }

    /// <summary>
    /// Tests that group property action executes action when property matches
    /// </summary>
    [Fact]
    public void Group_Property_Action_ExecutesAction_WhenPropertyMatches()
    {
        var result = CreateTestGroupByResult();
        var group = result.Groups[0];
        long? capturedCount = null;

        var matched = group.Property<Aggregate.Text>("textField", t => capturedCount = t.Count);

        Assert.True(matched);
        Assert.Equal(5, capturedCount);
    }

    /// <summary>
    /// Tests that group match action executes correct action
    /// </summary>
    [Fact]
    public void Group_Match_Action_ExecutesCorrectAction()
    {
        var result = CreateTestGroupByResult();
        var group = result.Groups[0];
        string? capturedType = null;

        var matched = group.Match(
            "textField",
            text: _ =>
            {
                capturedType = "text";
            },
            integer: _ =>
            {
                capturedType = "integer";
            }
        );

        Assert.True(matched);
        Assert.Equal("text", capturedType);
    }

    /// <summary>
    /// Tests that group match func returns correct value
    /// </summary>
    [Fact]
    public void Group_Match_Func_ReturnsCorrectValue()
    {
        var result = CreateTestGroupByResult();
        var group = result.Groups[0];

        var description = group.Match(
            "intField",
            text: t => $"Text: {t.Count}",
            integer: i => $"Integer range [{i.Minimum}, {i.Maximum}]"
        );

        Assert.Equal("Integer range [1, 50]", description);
    }

    #endregion

    #region Grpc Presence Mapping

    /// <summary>
    /// Every scalar in the aggregate reply is <c>optional</c> in the proto. When the server
    /// leaves one unset the client must report null, not the field type's zero — a returned 0,
    /// 0.0 or false is indistinguishable from a real aggregate. Driven straight off the proto
    /// message so an unset field is guaranteed, which a live server will not always produce:
    /// Weaviate 1.39.0 always fills the four boolean members in, so only this test pins them.
    /// </summary>
    [Fact]
    public void FromGrpcProperty_UnsetScalars_MapToNull()
    {
        var integer = AggregateResult.FromGrpcProperty(
            new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation
            {
                Property = "intField",
                Int =
                    new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation.Types.Integer(),
            }
        );
        var typedInteger = Assert.IsType<Aggregate.Integer>(integer);
        Assert.Null(typedInteger.Maximum);
        Assert.Null(typedInteger.Mean);
        Assert.Null(typedInteger.Median);
        Assert.Null(typedInteger.Minimum);
        Assert.Null(typedInteger.Mode);
        Assert.Null(typedInteger.Sum);
        // Count is deliberately still read unconditionally, as python does: a count of 0 is a
        // meaningful answer for an empty aggregate, not an absent one.
        Assert.Equal(0, typedInteger.Count);

        var number = AggregateResult.FromGrpcProperty(
            new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation
            {
                Property = "floatField",
                Number =
                    new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation.Types.Number(),
            }
        );
        var typedNumber = Assert.IsType<Aggregate.Number>(number);
        Assert.Null(typedNumber.Maximum);
        Assert.Null(typedNumber.Mean);
        Assert.Null(typedNumber.Median);
        Assert.Null(typedNumber.Minimum);
        Assert.Null(typedNumber.Mode);
        Assert.Null(typedNumber.Sum);

        var boolean = AggregateResult.FromGrpcProperty(
            new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation
            {
                Property = "boolField",
                Boolean =
                    new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation.Types.Boolean(),
            }
        );
        var typedBoolean = Assert.IsType<Aggregate.Boolean>(boolean);
        Assert.Null(typedBoolean.PercentageFalse);
        Assert.Null(typedBoolean.PercentageTrue);
        Assert.Null(typedBoolean.TotalFalse);
        Assert.Null(typedBoolean.TotalTrue);
    }

    /// <summary>
    /// The complement of the case above: a scalar the server did set is carried through, so the
    /// presence checks do not swallow real values. A deliberate zero must survive as zero.
    /// </summary>
    [Fact]
    public void FromGrpcProperty_SetScalars_MapToValues()
    {
        var integer = AggregateResult.FromGrpcProperty(
            new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation
            {
                Property = "intField",
                Int =
                    new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation.Types.Integer
                    {
                        Count = 3,
                        Maximum = 0,
                        Mean = 0,
                        Median = 2,
                        Minimum = -5,
                        Mode = 1,
                        Sum = 0,
                    },
            }
        );
        var typedInteger = Assert.IsType<Aggregate.Integer>(integer);
        Assert.Equal(3, typedInteger.Count);
        Assert.Equal(0, typedInteger.Maximum);
        Assert.Equal(0, typedInteger.Mean);
        Assert.Equal(2, typedInteger.Median);
        Assert.Equal(-5, typedInteger.Minimum);
        Assert.Equal(1, typedInteger.Mode);
        Assert.Equal(0, typedInteger.Sum);

        var number = AggregateResult.FromGrpcProperty(
            new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation
            {
                Property = "floatField",
                Number =
                    new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation.Types.Number
                    {
                        Count = 2,
                        Maximum = 0,
                        Mean = 0,
                        Median = 1.5,
                        Minimum = -1.5,
                        Mode = 0,
                        Sum = 0,
                    },
            }
        );
        var typedNumber = Assert.IsType<Aggregate.Number>(number);
        Assert.Equal(0, typedNumber.Maximum);
        Assert.Equal(0, typedNumber.Mean);
        Assert.Equal(1.5, typedNumber.Median);
        Assert.Equal(-1.5, typedNumber.Minimum);
        Assert.Equal(0, typedNumber.Mode);
        Assert.Equal(0, typedNumber.Sum);

        var boolean = AggregateResult.FromGrpcProperty(
            new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation
            {
                Property = "boolField",
                Boolean =
                    new Grpc.Protobuf.V1.AggregateReply.Types.Aggregations.Types.Aggregation.Types.Boolean
                    {
                        Count = 4,
                        PercentageFalse = 0,
                        PercentageTrue = 1,
                        TotalFalse = 0,
                        TotalTrue = 4,
                    },
            }
        );
        var typedBoolean = Assert.IsType<Aggregate.Boolean>(boolean);
        Assert.Equal(0, typedBoolean.PercentageFalse);
        Assert.Equal(1, typedBoolean.PercentageTrue);
        Assert.Equal(0, typedBoolean.TotalFalse);
        Assert.Equal(4, typedBoolean.TotalTrue);
    }

    #endregion
}
