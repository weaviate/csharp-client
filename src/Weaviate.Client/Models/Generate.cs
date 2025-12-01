namespace Weaviate.Client.Models;

public abstract record GenerativeProvider
{
    protected GenerativeProvider(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public bool ReturnMetadata { get; set; } = false;
}

public abstract record GenerativePrompt
{
    public bool Debug { get; set; } = false;
}

public record SinglePrompt(string Prompt) : GenerativePrompt
{
    public GenerativeProvider? Provider { get; set; }
}

public record GroupedPrompt(string Task, params string[] Properties) : GenerativePrompt
{
    public GenerativeProvider? Provider { get; set; }
}
