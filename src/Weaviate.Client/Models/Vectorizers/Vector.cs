using System.Linq.Expressions;

namespace Weaviate.Client.Models.Vectorizers;


public abstract class Vector2
{
    public sealed record Vectorizer
    {
        private readonly string _name;
        private VectorizerConfig? _vectorizerConfig;
        private VectorIndexConfig? _vectorIndexConfig;
        private string[] _properties = [];

        internal Vectorizer(string name, VectorizerConfig? _vectorizerConfig = null, VectorIndexConfig? _vectorIndexConfig = null, string[]? properties = null)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            this._vectorizerConfig = _vectorizerConfig;
            this._vectorIndexConfig = _vectorIndexConfig;
            _properties = properties ?? Array.Empty<string>();
        }

    }

    public static Vectorizer None(string name = "default", VectorIndexConfig? _vectorIndexConfig = null)
    {
        return new Vectorizer(name, null, _vectorIndexConfig);
    }


    public static Vectorizer Text2vec_Contextionary(string name = "default", VectorIndexConfig? _vectorIndexConfig= null, string[]? properties = null, bool? VectorizeCollectionName = null)
    {
        return new Vectorizer(name, new VectorizerConfig.Text2VecContextionary(VectorizeCollectionName), _vectorIndexConfig, properties);
    }
    public static Vectorizer Text2vec_Weaviate(string name = "default", VectorIndexConfig? _vectorIndexConfig = null, string[]? properties = null, bool? VectorizeCollectionName = null)
    {
        return new Vectorizer(name, new VectorizerConfig.Text2VecWeaviate(VectorizeCollectionName), _vectorIndexConfig, properties);
    }

public abstract class Vector
{

    public static Builder Name(string name)
    {
        return new Builder(name);
    }

    public sealed record Builder
    {
        private readonly string _name;
        private VectorizerConfig? _vectorizerConfig;
        private VectorIndexConfig? _vectorIndexConfig;
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

        public Builder With(VectorIndexConfig indexConfig) =>
            new(this with { _vectorIndexConfig = indexConfig });

        public Builder With(VectorizerConfig vectorizerConfig) =>
            new(this with { _vectorizerConfig = vectorizerConfig });

        public Builder From(params string[] properties) =>
            new(this with { _properties = properties });

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
                .Select(mi => mi.Name.Decapitalize())
                .ToArray();

            return From(names);
        }

        public VectorConfig Build()
        {
            return new(_name)
            {
                Vectorizer = _vectorizerConfig ?? new VectorizerConfig.None(),
                VectorIndexConfig = _vectorIndexConfig ?? VectorIndexConfig.Default,
            };
        }

        public static implicit operator VectorConfig(Builder src)
        {
            return src.Build();
        }

        public static implicit operator VectorConfigList(Builder src)
        {
            return src.Build();
        }
    }

    private static string defaultVectorName = "default";

    public static string DefaultVectorName
    {
        get => defaultVectorName;
        set => defaultVectorName = value;
    }
}
