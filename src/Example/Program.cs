
using System.Text.Json;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace Example;

class Program
{
    static private ClientConfiguration GetConfiguration()
    {
        var instanceUrl = Environment.GetEnvironmentVariable("WEAVIATE_CLUSTER_URL");
        if (string.IsNullOrEmpty(instanceUrl))
        {
            throw new Exception("Required environment variable WEAVIATE_CLUSTER_URL is missing.");
        }

        var apiKey = Environment.GetEnvironmentVariable("WEAVIATE_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("Required environment variable WEAVIATE_API_KEY is missing.");
        }

        return new ClientConfiguration
        {
            Host = new Uri(instanceUrl),
            ApiKey = apiKey
        };
    }

    static async Task<IEnumerable<T>> GetCatsAsync<T>(string filename)
    {
        try
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found: {filename}");
                return []; // Return an empty list if the file doesn't exist
            }

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                // Deserialize directly from the stream for better performance, especially with large files
                var data = await JsonSerializer.DeserializeAsync<IList<T>>(fs) ?? [];

                return data;
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return []; // Return an empty list on deserialization error
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return []; // Return an empty list on any other error
        }
    }

    static async Task Main()
    {
        //var config = GetConfiguration();

        var weaviate = new WeaviateClient();

        var collection = weaviate.Collections.Use<Cat>("Cat");

        // Should throw CollectionNotFound
        try
        {
            var collectionNotFound = await collection.Get();
        }
        catch
        {
            Console.WriteLine("cat collection not found");
        }

        // Delete any existing "cat" class
        try
        {
            await collection.Delete();
            Console.WriteLine("Deleted existing 'Cat' collection");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting collections: {e.Message}");
        }

        var vectorizerConfigNone = new VectorConfig
        {
            Vectorizer = new Dictionary<string, object>
            {
                { "none", new object { } }
            },
            VectorIndexType = "hnsw",
        };

        var VectorConfigs = new Dictionary<string, VectorConfig>
        {
            { "default", vectorizerConfigNone }
        };

        var catCollection = new Collection()
        {
            Name = "Cat",
            Description = "Lots of Cats of multiple breeds",
            Properties = [Property.Text("Name"), Property.Text("Color"), Property.Text("Breed"), Property.Int("Counter")],
            VectorConfig = VectorConfigs
        };

        collection = await weaviate.Collections.Create<Cat>(catCollection);

        await foreach (var c in weaviate.Collections.List())
        {
            Console.WriteLine($"Collection: {c.Name}");
        }

        // // Read 250 cats from JSON file and unmarshal into Cat class
        var cats = await GetCatsAsync<WeaviateObject<Cat>>("cats.json");

        // Use the C# client to store all cats with a cat class
        Console.WriteLine("Cats to store: " + cats.Count());
        foreach (var cat in cats)
        {
            cat.Vectors = new Dictionary<string, IList<float>>
            {
                { "default", cat.Vector }
            };
            cat.Vector = null;

            var inserted = await collection.Data.Insert(cat);
        }

        // Get all objects and sum up the counter property
        var objects = collection.Query.List(limit: 250);
        var retrieved = await objects.ToListAsync();
        Console.WriteLine("Cats retrieved: " + retrieved.Count());
        var sum = retrieved.Sum(c => c.Data?.Counter ?? 0);

        // Delete object
        var firstObj = retrieved.First();
        if (firstObj.ID is Guid id)
        {
            await collection.Data.Delete(id);
        }

        objects = collection.Query.List(limit: 5);
        retrieved = await objects.ToListAsync();
        Console.WriteLine("Cats retrieved: " + retrieved.Count());

        firstObj = retrieved.First();
        if (firstObj.ID is Guid id2)
        {
            var fetched = await collection.Query.FetchObjectByID(id: id2);
            Console.WriteLine("Cat retrieved via gRPC matches: " + ((fetched?.ID ?? Guid.Empty) == id2));
        }

        {
            var idList = retrieved
                        .Where(c => c.ID.HasValue)
                        .Take(10)
                        .Select(c => c.ID!.Value)
                        .ToHashSet();

            var fetched =
                await collection.Query.FetchObjectsByIDs(idList).ToListAsync();
            Console.WriteLine($"Cats retrieved via gRPC matches:{Environment.NewLine}" + JsonSerializer.Serialize(fetched, new JsonSerializerOptions { WriteIndented = true }).ToString());
        }

        var queryNearVector =
            collection
                .Query
                .NearVector(
                    vector: [20f, 21f, 22f],
                    distance: 0.5f,
                    limit: 5,
                    fields: ["name", "breed", "color", "counter"],
                    metadata: MetadataOptions.Score | MetadataOptions.Distance
                );

        await foreach (var cat in queryNearVector)
        {
            Console.WriteLine(JsonSerializer.Serialize(cat, new JsonSerializerOptions { WriteIndented = true }));
        }


        // Cursor API
        // var objects = collection.Iterator<Cat>();
        // var sum = await objects.SumAsync(c => c.Counter);

        // // Print all cats found
        // await foreach (var cat in objects)
        // {
        //     Console.WriteLine(cat);
        // }

        // Console.WriteLine($"Sum of counter on cats: {sum}");

        // // Do a quick search
        // var queryNearVector = weaviate
        //             .Get()
        //             .WithOperator(new SearchOperatorNearVector(
        //                 vector: [20, 21, 22]
        //             ))
        //             .WithFilter(limit: 5)
        //             .WithClassName("cat")
        //             .WithFields(["name", "breed", "color", "counter"])
        //             .WithMetadata(["score", "distance"]);

        // Console.WriteLine();
        // Console.WriteLine("Querying Neighboring Cats: [20,21,22]");
        // var found = await weaviate.Query<Cat>(queryNearVector);
        // foreach (var cat in found)
        // {
        //     Console.WriteLine(cat);
        // }

        // var sphynxQuery = await collection.Search<Cat>(new SearchOperatorBM25(
        //                 query: "Sphynx"
        //             ), query =>
        //             {
        //                 query.WithMetadata(["score"]);
        //             });

        // Console.WriteLine();
        // Console.WriteLine("Querying Cat Breed: Sphynx");
        // foreach (var cat in sphynxQuery.Select(c => c.Object))
        // {
        //     Console.WriteLine(cat);
        // }
    }
}
