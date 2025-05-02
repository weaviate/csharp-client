using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public static class Filter
{
    internal static Filters WithID(Guid id) => new Filters
    {
        Operator = Filters.Types.Operator.Equal,
        ValueText = id.ToString(),
        Target = new FilterTarget()
        {
            Property = "_id"
        }
    };

    internal static Filters WithIDs(ISet<Guid> ids) => Or(ids.Select(WithID));

    internal static Filters Or(IEnumerable<Filters> filters) => new Filters
    {
        Operator = Filters.Types.Operator.Or,
        Filters_ = { filters }
    };

    internal static Filters And(IEnumerable<Filters> filters) => new Filters
    {
        Operator = Filters.Types.Operator.And,
        Filters_ = { filters }
    };
}