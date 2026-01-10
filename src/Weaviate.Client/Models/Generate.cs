namespace Weaviate.Client.Models;

/// <summary>
/// The generative provider
/// </summary>
public abstract record GenerativeProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeProvider"/> class
    /// </summary>
    /// <param name="name">The name</param>
    protected GenerativeProvider(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the value of the name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the value of the return metadata
    /// </summary>
    public bool ReturnMetadata { get; set; } = false;
}

/// <summary>
/// The generative prompt
/// </summary>
public abstract record GenerativePrompt
{
    /// <summary>
    /// Gets or sets the value of the debug
    /// </summary>
    public bool Debug { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the provider
    /// </summary>
    public GenerativeProvider? Provider { get; internal set; }
}

/// <summary>
/// The single prompt
/// </summary>
public record SinglePrompt(string Prompt) : GenerativePrompt
{
    /// <summary>
    /// Implicitly converts a string to a SinglePrompt
    /// </summary>
    /// <param name="prompt">The prompt text</param>
    public static implicit operator SinglePrompt(string prompt) => new(prompt);
}

/// <summary>
/// The grouped task
/// </summary>
public record GroupedTask(string Task, params string[] Properties) : GenerativePrompt
{
    /// <summary>
    /// Implicitly converts a string to a GroupedTask
    /// </summary>
    /// <param name="task">The task text</param>
    public static implicit operator GroupedTask(string task) => new(task);
}
