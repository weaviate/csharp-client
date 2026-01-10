namespace Weaviate.Client.Models;

using V1 = Grpc.Protobuf.V1;

/// <summary>
/// The aggregate group by result
/// </summary>
public partial record AggregateGroupByResult
{
    /// <summary>
    /// The group
    /// </summary>
    public partial record Group
    {
        /// <summary>
        /// The by
        /// </summary>
        public record By(string Property, object Value, Type Type);

        /// <summary>
        /// Gets or inits the value of the grouped by
        /// </summary>
        public required By GroupedBy { get; init; }

        /// <summary>
        /// Gets or inits the value of the properties
        /// </summary>
        public IReadOnlyDictionary<string, Aggregate.Property> Properties { get; init; } =
            new Dictionary<string, Aggregate.Property>();

        /// <summary>
        /// Gets or inits the value of the total count
        /// </summary>
        public long TotalCount { get; init; } = 0;

        #region Typed Accessor Methods

        /// <summary>
        /// Gets the text aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The text aggregation, or null if not found or wrong type.</returns>
        public Aggregate.Text? Text(string propertyName) =>
            Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Text : null;

        /// <summary>
        /// Gets the integer aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The integer aggregation, or null if not found or wrong type.</returns>
        public Aggregate.Integer? Integer(string propertyName) =>
            Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Integer : null;

        /// <summary>
        /// Gets the number aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The number aggregation, or null if not found or wrong type.</returns>
        public Aggregate.Number? Number(string propertyName) =>
            Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Number : null;

        /// <summary>
        /// Gets the boolean aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The boolean aggregation, or null if not found or wrong type.</returns>
        public Aggregate.Boolean? Boolean(string propertyName) =>
            Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Boolean : null;

        /// <summary>
        /// Gets the date aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The date aggregation, or null if not found or wrong type.</returns>
        public Aggregate.Date? Date(string propertyName) =>
            Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Date : null;

        #endregion

        #region TryGet Methods

        /// <summary>
        /// Tries to get the text aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="text">The text aggregation if found and of correct type.</param>
        /// <returns>True if the property exists and is a text aggregation; otherwise, false.</returns>
        public bool TryGetText(string propertyName, out Aggregate.Text text)
        {
            if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Text t)
            {
                text = t;
                return true;
            }

            text = null!;
            return false;
        }

        /// <summary>
        /// Tries to get the integer aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="integer">The integer aggregation if found and of correct type.</param>
        /// <returns>True if the property exists and is an integer aggregation; otherwise, false.</returns>
        public bool TryGetInteger(string propertyName, out Aggregate.Integer integer)
        {
            if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Integer i)
            {
                integer = i;
                return true;
            }

            integer = null!;
            return false;
        }

        /// <summary>
        /// Tries to get the number aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="number">The number aggregation if found and of correct type.</param>
        /// <returns>True if the property exists and is a number aggregation; otherwise, false.</returns>
        public bool TryGetNumber(string propertyName, out Aggregate.Number number)
        {
            if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Number n)
            {
                number = n;
                return true;
            }

            number = null!;
            return false;
        }

        /// <summary>
        /// Tries to get the boolean aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="boolean">The boolean aggregation if found and of correct type.</param>
        /// <returns>True if the property exists and is a boolean aggregation; otherwise, false.</returns>
        public bool TryGetBoolean(string propertyName, out Aggregate.Boolean boolean)
        {
            if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Boolean b)
            {
                boolean = b;
                return true;
            }

            boolean = null!;
            return false;
        }

        /// <summary>
        /// Tries to get the date aggregation for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="date">The date aggregation if found and of correct type.</param>
        /// <returns>True if the property exists and is a date aggregation; otherwise, false.</returns>
        public bool TryGetDate(string propertyName, out Aggregate.Date date)
        {
            if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Date d)
            {
                date = d;
                return true;
            }

            date = null!;
            return false;
        }

        /// <summary>
        /// Tries to get the aggregation for the specified property as the specified type.
        /// </summary>
        /// <typeparam name="T">The expected aggregation type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="aggregation">The aggregation if found and of correct type.</param>
        /// <returns>True if the property exists and is of the specified type; otherwise, false.</returns>
        public bool TryGet<T>(string propertyName, out T aggregation)
            where T : Aggregate.Property
        {
            if (Properties.TryGetValue(propertyName, out var prop) && prop is T t)
            {
                aggregation = t;
                return true;
            }

            aggregation = null!;
            return false;
        }

        #endregion

        #region Match Methods

        /// <summary>
        /// Executes an action on the property if it exists and matches the expected type.
        /// </summary>
        /// <typeparam name="T">The expected aggregation type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="action">The action to execute if the property matches.</param>
        /// <returns>True if the property was matched and the action executed; otherwise, false.</returns>
        public bool Property<T>(string propertyName, Action<T> action)
            where T : Aggregate.Property
        {
            if (Properties.TryGetValue(propertyName, out var prop) && prop is T t)
            {
                action(t);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes a function on the property if it exists and matches the expected type.
        /// </summary>
        /// <typeparam name="T">The expected aggregation type.</typeparam>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="func">The function to execute if the property matches.</param>
        /// <returns>The result of the function, or default if the property doesn't match.</returns>
        public TResult? Property<T, TResult>(string propertyName, Func<T, TResult> func)
            where T : Aggregate.Property
        {
            if (Properties.TryGetValue(propertyName, out var prop) && prop is T t)
            {
                return func(t);
            }

            return default;
        }

        /// <summary>
        /// Matches the property against all possible aggregation types and executes the corresponding action.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="text">Action to execute if the property is a text aggregation.</param>
        /// <param name="integer">Action to execute if the property is an integer aggregation.</param>
        /// <param name="number">Action to execute if the property is a number aggregation.</param>
        /// <param name="boolean">Action to execute if the property is a boolean aggregation.</param>
        /// <param name="date">Action to execute if the property is a date aggregation.</param>
        /// <returns>True if the property was found and matched; otherwise, false.</returns>
        public bool Match(
            string propertyName,
            Action<Aggregate.Text>? text = null,
            Action<Aggregate.Integer>? integer = null,
            Action<Aggregate.Number>? number = null,
            Action<Aggregate.Boolean>? boolean = null,
            Action<Aggregate.Date>? date = null
        )
        {
            if (!Properties.TryGetValue(propertyName, out var prop))
                return false;

            switch (prop)
            {
                case Aggregate.Text t when text is not null:
                    text(t);
                    return true;
                case Aggregate.Integer i when integer is not null:
                    integer(i);
                    return true;
                case Aggregate.Number n when number is not null:
                    number(n);
                    return true;
                case Aggregate.Boolean b when boolean is not null:
                    boolean(b);
                    return true;
                case Aggregate.Date d when date is not null:
                    date(d);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Matches the property against all possible aggregation types and returns the result of the corresponding function.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="text">Function to execute if the property is a text aggregation.</param>
        /// <param name="integer">Function to execute if the property is an integer aggregation.</param>
        /// <param name="number">Function to execute if the property is a number aggregation.</param>
        /// <param name="boolean">Function to execute if the property is a boolean aggregation.</param>
        /// <param name="date">Function to execute if the property is a date aggregation.</param>
        /// <returns>The result of the matched function, or default if no match.</returns>
        public TResult? Match<TResult>(
            string propertyName,
            Func<Aggregate.Text, TResult>? text = null,
            Func<Aggregate.Integer, TResult>? integer = null,
            Func<Aggregate.Number, TResult>? number = null,
            Func<Aggregate.Boolean, TResult>? boolean = null,
            Func<Aggregate.Date, TResult>? date = null
        )
        {
            if (!Properties.TryGetValue(propertyName, out var prop))
                return default;

            return prop switch
            {
                Aggregate.Text t when text is not null => text(t),
                Aggregate.Integer i when integer is not null => integer(i),
                Aggregate.Number n when number is not null => number(n),
                Aggregate.Boolean b when boolean is not null => boolean(b),
                Aggregate.Date d when date is not null => date(d),
                _ => default,
            };
        }

        #endregion
    }

    /// <summary>
    /// Gets or inits the value of the groups
    /// </summary>
    public List<Group> Groups { get; init; } = new();

    /// <summary>
    /// Creates the grpc reply using the specified result
    /// </summary>
    /// <param name="result">The result</param>
    /// <exception cref="NotImplementedException">Unknown group by type: {gb.ValueCase}</exception>
    /// <returns>The aggregate group by result</returns>
    internal static AggregateGroupByResult FromGrpcReply(V1.AggregateReply result)
    {
        var groupByToGrpc = (V1.AggregateReply.Types.Group.Types.GroupedBy gb) =>
            gb.ValueCase switch
            {
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Boolean =>
                    new Group.By(gb.Path[0], gb.Boolean, typeof(bool)),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Booleans =>
                    new Group.By(gb.Path[0], gb.Booleans.Values.ToArray(), typeof(bool[])),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Int => new Group.By(
                    gb.Path[0],
                    gb.Int,
                    typeof(int)
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Ints => new Group.By(
                    gb.Path[0],
                    gb.Ints.Values.ToArray(),
                    typeof(int[])
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Number => new Group.By(
                    gb.Path[0],
                    gb.Number,
                    typeof(double)
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Numbers =>
                    new Group.By(gb.Path[0], gb.Numbers.Values.ToArray(), typeof(double[])),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Text => new Group.By(
                    gb.Path[0],
                    gb.Text,
                    typeof(string)
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Texts => new Group.By(
                    gb.Path[0],
                    gb.Texts.Values.ToArray(),
                    typeof(string[])
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Geo => new Group.By(
                    gb.Path[0],
                    new GeoCoordinate(gb.Geo.Latitude, gb.Geo.Longitude),
                    typeof(GeoCoordinate)
                ),

                _ => throw new NotImplementedException($"Unknown group by type: {gb.ValueCase}"),
            };

        var groupExtract = new Func<V1.AggregateReply.Types.Group, Group>(g =>
        {
            var groupedBy = groupByToGrpc(g.GroupedBy);
            var properties =
                g.Aggregations?.Aggregations_?.ToDictionary(
                    p => p.Property,
                    AggregateResult.FromGrpcProperty
                ) ?? new Dictionary<string, Aggregate.Property>();

            return new Group
            {
                GroupedBy = groupedBy,
                Properties = properties,
                TotalCount = g.ObjectsCount,
            };
        });

        var groupByGroups = result.GroupedResults?.Groups.Select(groupExtract).ToList() ?? [];

        return new AggregateGroupByResult { Groups = groupByGroups };
    }
}

/// <summary>
/// The aggregate result
/// </summary>
public partial record AggregateResult
{
    /// <summary>
    /// Gets or inits the value of the properties
    /// </summary>
    public IDictionary<string, Aggregate.Property> Properties { get; init; } =
        new Dictionary<string, Aggregate.Property>();

    /// <summary>
    /// Gets or inits the value of the total count
    /// </summary>
    public long TotalCount { get; init; }

    #region Typed Accessor Methods

    /// <summary>
    /// Gets the text aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The text aggregation, or null if not found or wrong type.</returns>
    public Aggregate.Text? Text(string propertyName) =>
        Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Text : null;

    /// <summary>
    /// Gets the integer aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The integer aggregation, or null if not found or wrong type.</returns>
    public Aggregate.Integer? Integer(string propertyName) =>
        Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Integer : null;

    /// <summary>
    /// Gets the number aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The number aggregation, or null if not found or wrong type.</returns>
    public Aggregate.Number? Number(string propertyName) =>
        Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Number : null;

    /// <summary>
    /// Gets the boolean aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The boolean aggregation, or null if not found or wrong type.</returns>
    public Aggregate.Boolean? Boolean(string propertyName) =>
        Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Boolean : null;

    /// <summary>
    /// Gets the date aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The date aggregation, or null if not found or wrong type.</returns>
    public Aggregate.Date? Date(string propertyName) =>
        Properties.TryGetValue(propertyName, out var prop) ? prop as Aggregate.Date : null;

    #endregion

    #region TryGet Methods

    /// <summary>
    /// Tries to get the text aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="text">The text aggregation if found and of correct type.</param>
    /// <returns>True if the property exists and is a text aggregation; otherwise, false.</returns>
    public bool TryGetText(string propertyName, out Aggregate.Text text)
    {
        if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Text t)
        {
            text = t;
            return true;
        }

        text = null!;
        return false;
    }

    /// <summary>
    /// Tries to get the integer aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="integer">The integer aggregation if found and of correct type.</param>
    /// <returns>True if the property exists and is an integer aggregation; otherwise, false.</returns>
    public bool TryGetInteger(string propertyName, out Aggregate.Integer integer)
    {
        if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Integer i)
        {
            integer = i;
            return true;
        }

        integer = null!;
        return false;
    }

    /// <summary>
    /// Tries to get the number aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="number">The number aggregation if found and of correct type.</param>
    /// <returns>True if the property exists and is a number aggregation; otherwise, false.</returns>
    public bool TryGetNumber(string propertyName, out Aggregate.Number number)
    {
        if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Number n)
        {
            number = n;
            return true;
        }

        number = null!;
        return false;
    }

    /// <summary>
    /// Tries to get the boolean aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="boolean">The boolean aggregation if found and of correct type.</param>
    /// <returns>True if the property exists and is a boolean aggregation; otherwise, false.</returns>
    public bool TryGetBoolean(string propertyName, out Aggregate.Boolean boolean)
    {
        if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Boolean b)
        {
            boolean = b;
            return true;
        }

        boolean = null!;
        return false;
    }

    /// <summary>
    /// Tries to get the date aggregation for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="date">The date aggregation if found and of correct type.</param>
    /// <returns>True if the property exists and is a date aggregation; otherwise, false.</returns>
    public bool TryGetDate(string propertyName, out Aggregate.Date date)
    {
        if (Properties.TryGetValue(propertyName, out var prop) && prop is Aggregate.Date d)
        {
            date = d;
            return true;
        }

        date = null!;
        return false;
    }

    /// <summary>
    /// Tries to get the aggregation for the specified property as the specified type.
    /// </summary>
    /// <typeparam name="T">The expected aggregation type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="aggregation">The aggregation if found and of correct type.</param>
    /// <returns>True if the property exists and is of the specified type; otherwise, false.</returns>
    public bool TryGet<T>(string propertyName, out T aggregation)
        where T : Aggregate.Property
    {
        if (Properties.TryGetValue(propertyName, out var prop) && prop is T t)
        {
            aggregation = t;
            return true;
        }

        aggregation = null!;
        return false;
    }

    #endregion

    #region Match Methods

    /// <summary>
    /// Executes an action on the property if it exists and matches the expected type.
    /// </summary>
    /// <typeparam name="T">The expected aggregation type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="action">The action to execute if the property matches.</param>
    /// <returns>True if the property was matched and the action executed; otherwise, false.</returns>
    public bool Property<T>(string propertyName, Action<T> action)
        where T : Aggregate.Property
    {
        if (Properties.TryGetValue(propertyName, out var prop) && prop is T t)
        {
            action(t);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Executes a function on the property if it exists and matches the expected type.
    /// </summary>
    /// <typeparam name="T">The expected aggregation type.</typeparam>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="func">The function to execute if the property matches.</param>
    /// <returns>The result of the function, or default if the property doesn't match.</returns>
    public TResult? Property<T, TResult>(string propertyName, Func<T, TResult> func)
        where T : Aggregate.Property
    {
        if (Properties.TryGetValue(propertyName, out var prop) && prop is T t)
        {
            return func(t);
        }

        return default;
    }

    /// <summary>
    /// Matches the property against all possible aggregation types and executes the corresponding action.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="text">Action to execute if the property is a text aggregation.</param>
    /// <param name="integer">Action to execute if the property is an integer aggregation.</param>
    /// <param name="number">Action to execute if the property is a number aggregation.</param>
    /// <param name="boolean">Action to execute if the property is a boolean aggregation.</param>
    /// <param name="date">Action to execute if the property is a date aggregation.</param>
    /// <returns>True if the property was found and matched; otherwise, false.</returns>
    public bool Match(
        string propertyName,
        Action<Aggregate.Text>? text = null,
        Action<Aggregate.Integer>? integer = null,
        Action<Aggregate.Number>? number = null,
        Action<Aggregate.Boolean>? boolean = null,
        Action<Aggregate.Date>? date = null
    )
    {
        if (!Properties.TryGetValue(propertyName, out var prop))
            return false;

        switch (prop)
        {
            case Aggregate.Text t when text is not null:
                text(t);
                return true;
            case Aggregate.Integer i when integer is not null:
                integer(i);
                return true;
            case Aggregate.Number n when number is not null:
                number(n);
                return true;
            case Aggregate.Boolean b when boolean is not null:
                boolean(b);
                return true;
            case Aggregate.Date d when date is not null:
                date(d);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Matches the property against all possible aggregation types and returns the result of the corresponding function.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="text">Function to execute if the property is a text aggregation.</param>
    /// <param name="integer">Function to execute if the property is an integer aggregation.</param>
    /// <param name="number">Function to execute if the property is a number aggregation.</param>
    /// <param name="boolean">Function to execute if the property is a boolean aggregation.</param>
    /// <param name="date">Function to execute if the property is a date aggregation.</param>
    /// <returns>The result of the matched function, or default if no match.</returns>
    public TResult? Match<TResult>(
        string propertyName,
        Func<Aggregate.Text, TResult>? text = null,
        Func<Aggregate.Integer, TResult>? integer = null,
        Func<Aggregate.Number, TResult>? number = null,
        Func<Aggregate.Boolean, TResult>? boolean = null,
        Func<Aggregate.Date, TResult>? date = null
    )
    {
        if (!Properties.TryGetValue(propertyName, out var prop))
            return default;

        return prop switch
        {
            Aggregate.Text t when text is not null => text(t),
            Aggregate.Integer i when integer is not null => integer(i),
            Aggregate.Number n when number is not null => number(n),
            Aggregate.Boolean b when boolean is not null => boolean(b),
            Aggregate.Date d when date is not null => date(d),
            _ => default,
        };
    }

    #endregion

    /// <summary>
    /// Creates the grpc reply using the specified reply
    /// </summary>
    /// <param name="reply">The reply</param>
    /// <returns>The aggregate result</returns>
    internal static AggregateResult FromGrpcReply(V1.AggregateReply reply)
    {
        return new AggregateResult
        {
            Properties = (
                reply.SingleResult?.Aggregations != null
                    ? reply.SingleResult.Aggregations
                    : new V1.AggregateReply.Types.Aggregations()
            ).Aggregations_.ToDictionary(x => x.Property, AggregateResult.FromGrpcProperty),
            TotalCount = reply.SingleResult?.ObjectsCount ?? 0,
        };
    }

    /// <summary>
    /// Creates the grpc property using the specified x
    /// </summary>
    /// <param name="x">The </param>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="NotImplementedException">Unknown aggregation case: {x.AggregationCase}</exception>
    /// <returns>The aggregate property</returns>
    internal static Aggregate.Property FromGrpcProperty(
        V1.AggregateReply.Types.Aggregations.Types.Aggregation x
    )
    {
        return x.AggregationCase switch
        {
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Text =>
                (Aggregate.Property)
                    new Aggregate.Text
                    {
                        Count = x.Text.Count,
                        TopOccurrences = (
                            x.Text.TopOccurences is null
                                ? []
                                : x.Text.TopOccurences.Items.Select(
                                    o => new Aggregate.TopOccurrence<string>
                                    {
                                        Count = o.Occurs,
                                        Value = o.Value,
                                    }
                                )
                        ).ToList(),
                    },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Int =>
                new Aggregate.Integer
                {
                    Count = x.Int.Count,
                    Maximum = x.Int.Maximum,
                    Mean = x.Int.Mean,
                    Median = x.Int.Median,
                    Minimum = x.Int.Minimum,
                    Mode = x.Int.Mode,
                    Sum = x.Int.Sum,
                },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Number =>
                new Aggregate.Number
                {
                    Count = x.Number.Count,
                    Maximum = x.Number.Maximum,
                    Mean = x.Number.Mean,
                    Median = x.Number.Median,
                    Minimum = x.Number.Minimum,
                    Mode = x.Number.Mode,
                    Sum = x.Number.Sum,
                },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Boolean =>
                new Aggregate.Boolean
                {
                    Count = x.Boolean.Count,
                    PercentageFalse = x.Boolean.PercentageFalse,
                    PercentageTrue = x.Boolean.PercentageTrue,
                    TotalFalse = x.Boolean.TotalFalse,
                    TotalTrue = x.Boolean.TotalTrue,
                },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Date =>
                new Aggregate.Date
                {
                    Count = x.Date.Count,
                    Maximum = x.Date.HasMaximum
                        ? DateTime.Parse(
                            x.Date.Maximum,
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                    Median = x.Date.HasMedian
                        ? DateTime.Parse(
                            x.Date.Median,
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                    Minimum = x.Date.HasMinimum
                        ? DateTime.Parse(
                            x.Date.Minimum,
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                    Mode = x.Date.HasMode
                        ? DateTime.Parse(
                            x.Date.Mode,
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Reference =>
                throw new NotImplementedException(),
            _ => throw new NotImplementedException(
                $"Unknown aggregation case: {x.AggregationCase}"
            ),
        };
    }
}

/// <summary>
/// The metrics class
/// </summary>
public class Metrics
{
    /// <summary>
    /// Gets the value of the property name
    /// </summary>
    private string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Metrics"/> class
    /// </summary>
    /// <param name="name">The name</param>
    protected Metrics(string name)
    {
        PropertyName = name;
    }

    /// <summary>
    /// Fors the property using the specified property name
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <returns>The metrics</returns>
    public static Metrics ForProperty(string propertyName) => new(propertyName);

    /// <summary>
    /// Texts the count
    /// </summary>
    /// <param name="count">The count</param>
    /// <param name="topOccurrencesCount">The top occurrences count</param>
    /// <param name="topOccurrencesValue">The top occurrences value</param>
    /// <param name="minOccurrences">The min occurrences</param>
    /// <returns>The aggregate metric</returns>
    public Aggregate.Metric Text(
        bool count = false,
        bool topOccurrencesCount = false,
        bool topOccurrencesValue = false,
        uint? minOccurrences = null
    )
    {
        if (!(count || topOccurrencesCount || topOccurrencesValue))
        {
            count = topOccurrencesCount = topOccurrencesValue = true;
        }

        return new Aggregate.Metric.Text(PropertyName)
        {
            Count = count,
            TopOccurrencesCount = topOccurrencesCount,
            TopOccurrencesValue = topOccurrencesValue,
            MinOccurrences = minOccurrences,
        };
    }

    /// <summary>
    /// Integers the count
    /// </summary>
    /// <param name="count">The count</param>
    /// <param name="maximum">The maximum</param>
    /// <param name="mean">The mean</param>
    /// <param name="median">The median</param>
    /// <param name="minimum">The minimum</param>
    /// <param name="mode">The mode</param>
    /// <param name="sum">The sum</param>
    /// <returns>The aggregate metric</returns>
    public Aggregate.Metric Integer(
        bool count = false,
        bool maximum = false,
        bool mean = false,
        bool median = false,
        bool minimum = false,
        bool mode = false,
        bool sum = false
    )
    {
        // If all parameters are false, enable all by default
        if (!(count || maximum || mean || median || minimum || mode || sum))
        {
            count = maximum = mean = median = minimum = mode = sum = true;
        }

        return new Aggregate.Metric.Integer(PropertyName)
        {
            Count = count,
            Maximum = maximum,
            Mean = mean,
            Median = median,
            Minimum = minimum,
            Mode = mode,
            Sum = sum,
        };
    }

    /// <summary>
    /// Numbers the count
    /// </summary>
    /// <param name="count">The count</param>
    /// <param name="maximum">The maximum</param>
    /// <param name="mean">The mean</param>
    /// <param name="median">The median</param>
    /// <param name="minimum">The minimum</param>
    /// <param name="mode">The mode</param>
    /// <param name="sum">The sum</param>
    /// <returns>The aggregate metric</returns>
    public Aggregate.Metric Number(
        bool count = false,
        bool maximum = false,
        bool mean = false,
        bool median = false,
        bool minimum = false,
        bool mode = false,
        bool sum = false
    )
    {
        // If all parameters are false, enable all by default
        if (!(count || maximum || mean || median || minimum || mode || sum))
        {
            count = maximum = mean = median = minimum = mode = sum = true;
        }

        return new Aggregate.Metric.Number(PropertyName)
        {
            Count = count,
            Maximum = maximum,
            Mean = mean,
            Median = median,
            Minimum = minimum,
            Mode = mode,
            Sum = sum,
        };
    }

    /// <summary>
    /// Booleans the count
    /// </summary>
    /// <param name="count">The count</param>
    /// <param name="percentageFalse">The percentage false</param>
    /// <param name="percentageTrue">The percentage true</param>
    /// <param name="totalFalse">The total false</param>
    /// <param name="totalTrue">The total true</param>
    /// <returns>The aggregate metric</returns>
    public Aggregate.Metric Boolean(
        bool count = false,
        bool percentageFalse = false,
        bool percentageTrue = false,
        bool totalFalse = false,
        bool totalTrue = false
    )
    {
        if (!(count || percentageFalse || percentageTrue || totalFalse || totalTrue))
        {
            count = percentageFalse = percentageTrue = totalFalse = totalTrue = true;
        }

        return new Aggregate.Metric.Boolean(PropertyName)
        {
            Count = count,
            PercentageFalse = percentageFalse,
            PercentageTrue = percentageTrue,
            TotalFalse = totalFalse,
            TotalTrue = totalTrue,
        };
    }

    /// <summary>
    /// Dates the count
    /// </summary>
    /// <param name="count">The count</param>
    /// <param name="maximum">The maximum</param>
    /// <param name="median">The median</param>
    /// <param name="minimum">The minimum</param>
    /// <param name="mode">The mode</param>
    /// <returns>The aggregate metric</returns>
    public Aggregate.Metric Date(
        bool count = false,
        bool maximum = false,
        bool median = false,
        bool minimum = false,
        bool mode = false
    )
    {
        if (!(count || maximum || median || minimum || mode))
        {
            count = maximum = median = minimum = mode = true;
        }

        return new Aggregate.Metric.Date(PropertyName)
        {
            Count = count,
            Maximum = maximum,
            Median = median,
            Minimum = minimum,
            Mode = mode,
        };
    }
}

/// <summary>
/// The aggregate class
/// </summary>
public static partial class Aggregate
{
    /// <summary>
    /// The metric
    /// </summary>
    public abstract record Metric(string Name)
    {
        /// <summary>
        /// Gets or inits the value of the count
        /// </summary>
        public bool Count { get; init; }

        /// <summary>
        /// The text
        /// </summary>
        public record Text(string Name) : Metric(Name)
        {
            /// <summary>
            /// Gets or inits the value of the top occurrences count
            /// </summary>
            public bool TopOccurrencesCount { get; init; }

            /// <summary>
            /// Gets or inits the value of the top occurrences value
            /// </summary>
            public bool TopOccurrencesValue { get; init; }

            /// <summary>
            /// Gets or inits the value of the min occurrences
            /// </summary>
            public uint? MinOccurrences { get; init; }
        }

        /// <summary>
        /// The integer
        /// </summary>
        public record Integer(string Name) : Metric(Name)
        {
            /// <summary>
            /// Gets or inits the value of the maximum
            /// </summary>
            public bool Maximum { get; init; }

            /// <summary>
            /// Gets or inits the value of the mean
            /// </summary>
            public bool Mean { get; init; }

            /// <summary>
            /// Gets or inits the value of the median
            /// </summary>
            public bool Median { get; init; }

            /// <summary>
            /// Gets or inits the value of the minimum
            /// </summary>
            public bool Minimum { get; init; }

            /// <summary>
            /// Gets or inits the value of the mode
            /// </summary>
            public bool Mode { get; init; }

            /// <summary>
            /// Gets or inits the value of the sum
            /// </summary>
            public bool Sum { get; init; }
        }

        /// <summary>
        /// The number
        /// </summary>
        public record Number(string Name) : Metric(Name)
        {
            /// <summary>
            /// Gets or inits the value of the maximum
            /// </summary>
            public bool Maximum { get; init; }

            /// <summary>
            /// Gets or inits the value of the mean
            /// </summary>
            public bool Mean { get; init; }

            /// <summary>
            /// Gets or inits the value of the median
            /// </summary>
            public bool Median { get; init; }

            /// <summary>
            /// Gets or inits the value of the minimum
            /// </summary>
            public bool Minimum { get; init; }

            /// <summary>
            /// Gets or inits the value of the mode
            /// </summary>
            public bool Mode { get; init; }

            /// <summary>
            /// Gets or inits the value of the sum
            /// </summary>
            public bool Sum { get; init; }
        }

        /// <summary>
        /// The boolean
        /// </summary>
        public record Boolean(string Name) : Metric(Name)
        {
            /// <summary>
            /// Gets or inits the value of the percentage false
            /// </summary>
            public bool PercentageFalse { get; init; }

            /// <summary>
            /// Gets or inits the value of the percentage true
            /// </summary>
            public bool PercentageTrue { get; init; }

            /// <summary>
            /// Gets or inits the value of the total false
            /// </summary>
            public bool TotalFalse { get; init; }

            /// <summary>
            /// Gets or inits the value of the total true
            /// </summary>
            public bool TotalTrue { get; init; }
        }

        /// <summary>
        /// The date
        /// </summary>
        public record Date(string Name) : Metric(Name)
        {
            /// <summary>
            /// Gets or inits the value of the maximum
            /// </summary>
            public bool Maximum { get; init; }

            /// <summary>
            /// Gets or inits the value of the median
            /// </summary>
            public bool Median { get; init; }

            /// <summary>
            /// Gets or inits the value of the minimum
            /// </summary>
            public bool Minimum { get; init; }

            /// <summary>
            /// Gets or inits the value of the mode
            /// </summary>
            public bool Mode { get; init; }
        }
    }
}

/// <summary>
/// The aggregate class
/// </summary>
public static partial class Aggregate
{
    /// <summary>
    /// The group by
    /// </summary>
    public record GroupBy(string Property, uint? Limit = null)
    {
        public static implicit operator GroupBy(string property) => new(property);
    };

    /// <summary>
    /// The property
    /// </summary>
    public abstract record Property
    {
        /// <summary>
        /// Gets or inits the value of the count
        /// </summary>
        public long? Count { get; internal init; }
    }

    /// <summary>
    /// The top occurrence
    /// </summary>
    public record TopOccurrence<T> : Property
    {
        /// <summary>
        /// Gets or inits the value of the value
        /// </summary>
        public T? Value { get; internal init; }
    }

    /// <summary>
    /// The text
    /// </summary>
    public record Text : Property
    {
        /// <summary>
        /// Gets or inits the value of the top occurrences
        /// </summary>
        public List<TopOccurrence<string>> TopOccurrences { get; internal init; } = new();
    };

    /// <summary>
    /// The numeric
    /// </summary>
    public abstract record Numeric<T> : Property
        where T : struct
    {
        /// <summary>
        /// Gets or sets the value of the maximum
        /// </summary>
        public T? Maximum { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the mean
        /// </summary>
        public double? Mean { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the median
        /// </summary>
        public double? Median { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the minimum
        /// </summary>
        public T? Minimum { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the mode
        /// </summary>
        public T? Mode { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the sum
        /// </summary>
        public T? Sum { get; internal set; }
    }

    /// <summary>
    /// The integer
    /// </summary>
    public record Integer : Numeric<long> { };

    /// <summary>
    /// The number
    /// </summary>
    public record Number : Numeric<double> { };

    /// <summary>
    /// The boolean
    /// </summary>
    public record Boolean : Property
    {
        /// <summary>
        /// Gets or sets the value of the percentage false
        /// </summary>
        public double PercentageFalse { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the percentage true
        /// </summary>
        public double PercentageTrue { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the total false
        /// </summary>
        public long TotalFalse { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the total true
        /// </summary>
        public long TotalTrue { get; internal set; }
    };

    /// <summary>
    /// The date
    /// </summary>
    public record Date : Property
    {
        /// <summary>
        /// Gets or sets the value of the maximum
        /// </summary>
        public DateTime? Maximum { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the median
        /// </summary>
        public DateTime? Median { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the minimum
        /// </summary>
        public DateTime? Minimum { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the mode
        /// </summary>
        public DateTime? Mode { get; internal set; }
    };
}
