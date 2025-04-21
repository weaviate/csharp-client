using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public static class Filter
{
    public static Filters WithID(Guid id) => new Filters
    {
        Operator = Filters.Types.Operator.Equal,
        ValueText = id.ToString(),
        Target = new FilterTarget()
        {
            Property = "_id"
        }
    };

    public static Filters WithIDs(ISet<Guid> ids) => Or(ids.Select(WithID));

    public static Filters Or(IEnumerable<Filters> filters) => new Filters
    {
        Operator = Filters.Types.Operator.Or,
        Filters_ = { filters }
    };

    public static Filters And(IEnumerable<Filters> filters) => new Filters
    {
        Operator = Filters.Types.Operator.Or,
        Filters_ = { filters }
    };
}