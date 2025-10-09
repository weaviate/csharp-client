namespace Example;

public record Cat
{
    public int Counter { get; set; }
    public string? Color { get; set; }
    public string? Breed { get; set; }
    public string? Name { get; set; }

    public override string ToString()
    {
        return $"Cat ({Counter}) {{ Name: {Name}, Color: {Color}, Breed: {Breed} }}";
    }
}
