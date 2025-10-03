namespace Weaviate.Client.Models;

public abstract record GenerativeProvider
{
    protected GenerativeProvider(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public bool ReturnMetadata { get; set; } = false;
    public bool Debug { get; set; } = false;
}

public abstract record GenerativePrompt
{
    public bool Debug { get; set; } = false;
}

public record SinglePrompt : GenerativePrompt
{
    public required string Prompt { get; set; }
    public GenerativeProvider? Provider { get; set; }
}

public record GroupedPrompt : GenerativePrompt
{
    public required string Task { get; set; }
    public List<string> Properties { get; set; } = [];
    public GenerativeProvider? Provider { get; set; }
}
