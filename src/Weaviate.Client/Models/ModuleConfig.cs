namespace Weaviate.Client.Models;

public class ModuleConfigList : Dictionary<string, object>, IEquatable<ModuleConfigList>
{
    public override bool Equals(object? obj)
    {
        return Equals(obj as ModuleConfigList);
    }

    public bool Equals(ModuleConfigList? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Count != other.Count)
            return false;

        foreach (var kvp in this)
        {
            if (!other.TryGetValue(kvp.Key, out var otherValue))
                return false;

            // Deep compare the module config objects via JSON serialization
            var thisJson = System.Text.Json.JsonSerializer.Serialize(kvp.Value);
            var otherJson = System.Text.Json.JsonSerializer.Serialize(otherValue);
            if (thisJson != otherJson)
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var kvp in this)
        {
            hash.Add(kvp.Key);
            // Use JSON serialization for consistent hashing of complex objects
            hash.Add(System.Text.Json.JsonSerializer.Serialize(kvp.Value));
        }
        return hash.ToHashCode();
    }
}
