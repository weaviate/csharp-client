﻿using System.Text.Json;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace Example;

class Program
{
    private static readonly bool _useBatchInsert = true;

    private record CatDataWithVectors(float[] Vector, Cat Data);

    static async Task<List<CatDataWithVectors>> GetCatsAsync(string filename)
    {
        try
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found: {filename}");
                return []; // Return an empty list if the file doesn't exist
            }

            using FileStream fs = new FileStream(
                filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );

            // Deserialize directly from the stream for better performance, especially with large files
            var data = await JsonSerializer.DeserializeAsync<List<CatDataWithVectors>>(fs) ?? [];

            return data;
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
        // Read 250 cats from JSON file and unmarshal into Cat class
        var cats = await GetCatsAsync("cats.json");

        // Use the C# client to store all cats with a cat class
        Console.WriteLine("Cats to store: " + cats.Count);

        WeaviateClient weaviate = Connect.Local();

        var collection = weaviate.Collections.Use<Cat>("Cat");

        // Should throw CollectionNotFound
        try
        {
            var collectionNotFound = await collection.Config.Get();
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

        var catCollection = new CollectionConfig()
        {
            Name = "Cat",
            Description = "Lots of Cats of multiple breeds",
            Properties = [.. Property.FromClass<Cat>()],
            VectorConfig = new VectorConfig("default", new Vectorizer.Text2VecWeaviate()),
        };

        collection = await weaviate.Collections.Create<Cat>(catCollection);

        await foreach (var c in weaviate.Collections.List())
        {
            Console.WriteLine($"Collection: {c.Name}");
        }

        if (_useBatchInsert)
        {
            // Batch Insertion Demo
            var requests = cats.Select(c => (c.Data, new Vectors { { "default", c.Vector } }));

            var batchInsertions = await collection.Data.InsertMany(requests);
        }
        else
        {
            // Normal Insertion Demo
            foreach (var cat in cats)
            {
                var vectors = new Vectors() { { "default", cat.Vector } };

                var inserted = await collection.Data.Insert(cat.Data, vectors: vectors);
            }
        }

        // Get all objects and sum up the counter property
        var result = await collection.Query.FetchObjects(limit: 250);
        var retrieved = result.Objects.ToList();
        Console.WriteLine("Cats retrieved: " + retrieved.Count());
        var sum = retrieved.Sum(c => c.As<Cat>()?.Counter ?? 0);

        // Delete object
        var firstObj = retrieved.First();
        if (firstObj.ID is Guid id)
        {
            await collection.Data.DeleteByID(id);
        }

        result = await collection.Query.FetchObjects(limit: 5);
        retrieved = result.Objects.ToList();
        Console.WriteLine("Cats retrieved: " + retrieved.Count());

        firstObj = retrieved.First();
        if (firstObj.ID is Guid id2)
        {
            var fetched = await collection.Query.FetchObjectByID(id: id2);
            Console.WriteLine(
                "Cat retrieved via gRPC matches: " + ((fetched?.ID ?? Guid.Empty) == id2)
            );
        }

        {
            var idList = retrieved
                .Where(c => c.ID.HasValue)
                .Take(10)
                .Select(c => c.ID!.Value)
                .ToHashSet();

            var fetched = await collection.Query.FetchObjectsByIDs(idList);
            Console.WriteLine(
                $"Cats retrieved via gRPC matches:{Environment.NewLine} {JsonSerializer.Serialize(fetched.Objects, new JsonSerializerOptions { WriteIndented = true })}"
            );
        }

        Console.WriteLine("Querying Neighboring Cats: [20,21,22]");

        var queryNearVector = await collection.Query.NearVector(
            vector: new[] { 20f, 21f, 22f },
            distance: 0.5f,
            limit: 5,
            returnProperties: ["name", "breed", "color", "counter"],
            returnMetadata: MetadataOptions.Score | MetadataOptions.Distance
        );

        foreach (var cat in queryNearVector.Objects.Select(o => o.As<Cat>()))
        {
            // Console.WriteLine(
            //     JsonSerializer.Serialize(cat, new JsonSerializerOptions { WriteIndented = true })
            // );

            Console.WriteLine(cat);
        }

        Console.WriteLine();
        Console.WriteLine("Using collection iterator:");

        // Cursor API demo
        var objects = collection.Iterator().Select(o => o.As<Cat>());
        var sumWithIterator = await objects.SumAsync(c => c!.Counter);

        // Print all cats found
        foreach (var cat in await objects.OrderBy(x => x!.Counter).ToListAsync())
        {
            Console.WriteLine(cat);
        }

        Console.WriteLine($"Sum of counter on cats: {sumWithIterator}");

        var sphynxQuery = await collection.Query.BM25(
            query: "Sphynx",
            returnMetadata: MetadataOptions.Score
        );

        Console.WriteLine();
        Console.WriteLine("Querying Cat Breed: Sphynx");
        foreach (var cat in sphynxQuery)
        {
            Console.WriteLine(cat);
        }
    }
}
