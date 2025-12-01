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

public record SinglePrompt : GenerativePrompt
{
    public SinglePrompt() { }

    public SinglePrompt(string prompt)
    {
        Prompt = prompt;
    }

    public required string Prompt { get; set; }
    public GenerativeProvider? Provider { get; set; }
}

public record GroupedTask : GenerativePrompt
{
    public GroupedTask() { }

    public GroupedTask(string task, params string[] properties)
    {
        Task = task;
        Properties = [.. properties];
    }

    public required string Task { get; set; }
    public OneOrManyOf<string> Properties { get; set; } = [];
    public GenerativeProvider? Provider { get; set; }
}
