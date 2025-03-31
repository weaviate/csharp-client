namespace Example;

public record Cat
{
    public int Counter { get; set; }
    public string? Color { get; set; }
    public string? Breed { get; set; }
    public string? Name { get; set; }

    public override string ToString()
    {
        return $"Cat {{ Name: {Name}, Color: {Color}, Breed: {Breed} }}";
    }
}

public record CatObject : Cat
{
    public float[] Vector { get; set; } = { };
}