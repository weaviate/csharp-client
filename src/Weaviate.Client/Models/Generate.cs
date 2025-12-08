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
    public GenerativeProvider? Provider { get; internal set; }
}

public record SinglePrompt(string Prompt) : GenerativePrompt
{
    public static implicit operator SinglePrompt(string prompt) => new(prompt);
}

public record GroupedTask(string Task, params string[] Properties) : GenerativePrompt
{
    public static implicit operator GroupedTask(string task) => new(task);
}
