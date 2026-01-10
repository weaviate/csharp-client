namespace Example;

/// <summary>
/// The cat
/// </summary>
public record Cat
{
    /// <summary>
    /// Gets or sets the value of the counter
    /// </summary>
    public int Counter { get; set; }

    /// <summary>
    /// Gets or sets the value of the color
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the value of the breed
    /// </summary>
    public string? Breed { get; set; }

    /// <summary>
    /// Gets or sets the value of the name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Returns the string
    /// </summary>
    /// <returns>The string</returns>
    public override string ToString()
    {
        return $"Cat ({Counter}) {{ Name: {Name}, Color: {Color}, Breed: {Breed} }}";
    }
}
