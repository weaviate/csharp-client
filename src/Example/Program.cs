
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

        await foreach (var c in weaviate.Collections.List())
        {
            Console.WriteLine($"Collection: {c.Name}");
        }

        // Should throw CollectionNotFound
        try
        {
            var collectionNotFound = await weaviate.Collections.Get("cat");
        }
        catch
        {
            Console.WriteLine("cat collection not found");
        }

        // Delete any existing "cat" class
        try
        {
            await weaviate.Collections.Delete("cat");
            Console.WriteLine("Deleted existing 'Cat' collection");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting collections: {e.Message}");
        }

        await weaviate.Collections.Create(c =>
        {
            c.Description = "Lots of Cats of multiple breeds";
            c.Name = "cat";
            c.Properties = [Property.Text("Name"), Property.Text("Color"), Property.Text("Breed"), Property.Int("Counter")];
        });

        // TODO Maybe pass the expected working type to Collections.Get
        var collection = await weaviate.Collections.Get("cat");

        // // Read 250 cats from JSON file and unmarshal into Cat class
        var cats = await GetCatsAsync<WeaviateObject<Cat>>("cats.json");

        // Use the C# client to store all cats with a cat class
        Console.WriteLine("Cats to store: " + cats.Count());
        foreach (var cat in cats)
        {
            var inserted = await collection.Insert(cat);
        }

        // Get all objects and sum up the counter property
        var objects = collection.ListObjects<Cat>(limit: 250);
        var retrieved = await objects.ToListAsync();
        Console.WriteLine("Cats retrieved: " + retrieved.Count());
        var sum = retrieved.Sum(c => c.Data?.Counter ?? 0);

        // Delete object
        var firstObj = retrieved.First();
        if (firstObj.ID is Guid id)
        {
            await collection.DeleteObject(id);
        }

        objects = collection.ListObjects<Cat>(limit: 5);
        retrieved = await objects.ToListAsync();
        Console.WriteLine("Cats retrieved: " + retrieved.Count());

        firstObj = retrieved.First();
        if (firstObj.ID is Guid id2)
        {
            var fetched = await collection.FetchObjectByID<Cat>(id: id2);
            Console.WriteLine("Cat retrieved via gRPC matches: " + ((fetched?.ID ?? Guid.Empty) == id2));
        }

        {
            var idList = retrieved
                        .Where(c => c.ID.HasValue)
                        .Take(10)
                        .Select(c => c.ID!.Value)
                        .ToHashSet();

            var fetched =
                await collection.FetchObjectsByIDs<Cat>(idList).ToListAsync();
            Console.WriteLine("Cats retrieved via gRPC matches: " + fetched.ToString());
        }

        // var queryNearVector = collection
        //             .Search<NearVector>()
        //             .WithLimit(5)
        //             .WithFilter(limit: 5)
        //             .WithClassName("cat")
        //             .WithFields(["name", "breed", "color", "counter"])
        //             .WithMetadata(["score", "distance"]);

        // var queryNearVector2 = collection
        //             .Query<NearVector>()
        //             .With(
        //                 limit: 5,
        //                 fields: ["name", "breed", "color", "counter"],
        //                 metadata: ["score", "distance"]
        //             );


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
