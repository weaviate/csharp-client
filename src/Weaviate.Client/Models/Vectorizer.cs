namespace Weaviate.Client.Models;

public record VectorizerList(params Vectorizer[] Vectorizers)
{
    public static implicit operator Dictionary<string, object>(VectorizerList list)
    {
        return list.Vectorizers.ToDictionary(v => v.Name, v => v.Configuration);
    }

    public static implicit operator VectorizerList(Vectorizer[] list)
    {
        return new VectorizerList(list);
    }
}

public record Vectorizer
{
    public string Name { get; }
    public object Configuration { get; }

    private Vectorizer(string name, object configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    public static implicit operator Dictionary<string, object>(Vectorizer vectorizer)
    {
        return new Dictionary<string, object> { [vectorizer.Name] = vectorizer.Configuration };
    }

    public static Vectorizer Text2VecContextionary(bool vectorizeClassName = false)
    {
        return new Vectorizer("text2vec-contextionary", new { VectorizeClassName = false });
    }
}
