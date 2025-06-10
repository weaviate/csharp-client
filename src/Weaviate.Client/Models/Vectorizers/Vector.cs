using System.Linq.Expressions;

namespace Weaviate.Client.Models.Vectorizers;

public abstract class Vector
{
    public static Builder Name(string name)
    {
        return new Builder(name);
    }

    public sealed record Builder
    {
        private readonly string _name;
        private VectorizerConfig? _vectorizerConfig = null;
        private VectorIndexConfig? _vectorIndexConfig = null;
        private string[] _properties = [];

        internal Builder(string namedVector)
        {
            _name = namedVector;
        }

        // Copy constructor with additional behavior
        Builder(Builder other)
        {
            _name = other._name;
            _vectorizerConfig = other._vectorizerConfig;
            _vectorIndexConfig = other._vectorIndexConfig;
            _properties = other._properties;

            if (_vectorizerConfig is not null && _properties.Length > 0)
            {
                _vectorizerConfig.Properties = _properties;
            }
        }

        public Builder From(params string[] properties) =>
            new Builder(this) with
            {
                _properties = properties,
            };

        private static MemberExpression? GetMemberExpression<TData>(
            Expression<Func<TData, object>> expression
        )
        {
            // Handle direct member access: t => t.Property
            if (expression.Body is MemberExpression memberExpr)
            {
                return memberExpr;
            }

            // Handle value types that get boxed: t => t.IntProperty becomes t => (object)t.IntProperty
            if (
                expression.Body is UnaryExpression unaryExpr
                && unaryExpr.NodeType == ExpressionType.Convert
                && unaryExpr.Operand is MemberExpression convertedMember
            )
            {
                return convertedMember;
            }

            return null;
        }

        public Builder From<TData>(params Expression<Func<TData, object>>[] properties)
        {
            if (!properties.All(s => GetMemberExpression(s) != null))
            {
                throw new ArgumentException(
                    "All expressions must be a member access",
                    nameof(properties)
                );
            }

            var names = properties
                .Select(GetMemberExpression)
                .Where(me => me != null)
                .Select(me => me!.Member)
                .Select(mi => mi.Name)
                .ToArray();

            return From(names);
        }

        public Builder With(VectorIndexConfig indexConfig) =>
            new Builder(this) with
            {
                _vectorIndexConfig = indexConfig,
            };

        public Builder With(VectorizerConfig vectorizerConfig) =>
            new Builder(this) with
            {
                _vectorizerConfig = vectorizerConfig,
            };

        public VectorConfig Build()
        {
            return New(_name, _vectorizerConfig, _vectorIndexConfig, _properties);
        }

        public static implicit operator VectorConfig(Builder src)
        {
            return src.Build();
        }

        public static implicit operator VectorConfigList(Builder src)
        {
            return src;
        }
    }

    private static string defaultVectorName = "default";

    public static string DefaultVectorName
    {
        get => defaultVectorName;
        set => defaultVectorName = value;
    }

    private static VectorConfig New(
        string namedVector,
        VectorizerConfig? config = null,
        VectorIndexConfig? vectorIndexConfig = null,
        params string[] sourceProperties
    )
    {
        config ??= new VectorizerConfig.None();

        config.Properties = [.. sourceProperties];

        vectorIndexConfig ??= new VectorIndexConfig.HNSW();

        return new(namedVector) { Vectorizer = config, VectorIndexConfig = vectorIndexConfig };
    }

    public static VectorConfig None(
        string name,
        VectorIndexConfig? vectorIndexConfig = null,
        params string[] sourceProperties
    ) => New(name, vectorIndexConfig: vectorIndexConfig, sourceProperties: sourceProperties);

    public static VectorConfig Text2VecContextionary(
        string name,
        VectorizerConfig.Text2VecContextionary? vectorConfig = null,
        VectorIndexConfig? vectorIndexConfig = null,
        params string[] sourceProperties
    )
    {
        return New(
            namedVector: name,
            config: vectorConfig ?? new VectorizerConfig.Text2VecContextionary(),
            vectorIndexConfig: vectorIndexConfig,
            sourceProperties
        );
    }

    public static VectorConfig Text2VecWeaviate(
        string name,
        VectorizerConfig.Text2VecWeaviate? vectorConfig = null,
        VectorIndexConfig? vectorIndexConfig = null,
        params string[] sourceProperties
    )
    {
        return New(
            namedVector: name,
            config: vectorConfig ?? new VectorizerConfig.Text2VecWeaviate(),
            vectorIndexConfig: vectorIndexConfig,
            sourceProperties
        );
    }
}
