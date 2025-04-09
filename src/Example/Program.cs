using System.Text.Json;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace Example
{
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
                ApiUrl = new Uri(instanceUrl),
                ApiKey = apiKey
            };
        }

        static async Task<IEnumerable<CatObject>> GetCatsAsync(string filename)
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
                    return await JsonSerializer.DeserializeAsync<IList<CatObject>>(fs) ?? [];
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
            // Read 250 cats from JSON file and unmarshal into Cat class
            var cats = await GetCatsAsync("cats.json");

            //var config = GetConfiguration();

            var weaviate = new WeaviateClient();

            await foreach (var c in weaviate.Collections.List())
            {
                Console.WriteLine($"Collection: {c.Name}");
            }

            var collectionSphere = await weaviate.Collections.Get("Test_sphere");

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
                Console.WriteLine("'Cat' collection not found. Will be created.");
                Console.WriteLine(e.Message);
            }

            await weaviate.Collections.Create(c =>
            {
                c.Description = "Lots of Cats of multiple breeds";
                c.Name = "cat";
                c.Properties = new List<Property>() {
                    new Property {
                        Name = "Name",
                        DataType = { DataType.Text },
                    },
                    new Property {
                        Name = "Color",
                        DataType = { DataType.Text },
                    },
                    new Property {
                        Name = "Breed",
                        DataType = { DataType.Text },
                    },
                    new Property {
                        Name = "Name",
                        DataType = { DataType.Text },
                    },
                    new Property {
                        Name = "Counter",
                        DataType = { DataType.Int },
                    },
                };
            });

            var catCollection = await weaviate.Collections.Get("cat");

            Console.WriteLine(catCollection);

            // // Use the C# client to store all cats with a cat class
            // Console.WriteLine("Cats to store: " + cats.Count());
            // Random rnd = new Random();
            // foreach (var cat in cats)
            // {
            //     Console.WriteLine(cat);
            //     // var id = await collection.Insert<Cat>(cat, vector: cat.Vector);
            //     int randomNumber = rnd.Next(10, 30);
            //     await Task.Delay(randomNumber);
            // }

            // Get all objects and sum up the counter property

            // Low level way
            // var objects = await weaviate.GetObjects<Cat>("cat", ["name", "counter"], 10, null);
            // var sum = objects.Sum(c => c.Object.Counter);

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
}