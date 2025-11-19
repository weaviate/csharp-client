using Google.Protobuf;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class ResultTests
{
    [Fact]
    public void SearchReply_Builds_GenerativeGroupByResult_Response()
    {
        var json =
            @"{
        ""took"": 11.081599,
        ""generativeGroupedResult"": ""Teddy apples"",
        ""groupByResults"": [
            {
                ""name"": ""apples are big"",
                ""numberOfObjects"": ""1"",
                ""objects"": [
                    {
                        ""properties"": {
                            ""targetCollection"": ""Test_BM25_Generate_And_GroupBy_With_Everything_a2a43f163495a0d5192c90f24b4ef015eb267eb5e056149c2e60d5c3aab85ae4_Object"",
                            ""nonRefProps"": {
                                ""fields"": {
                                    ""text"": { ""textValue"": ""apples are big"" },
                                    ""content"": { ""textValue"": ""Teddy is the biggest and bigger than everything else"" }
                                }
                            }
                        },
                        ""metadata"": {
                            ""id"": ""02c8d66d-d314-4b9c-ab2e-4d5daa9e55ae"",
                            ""idAsBytes"": ""AsjWbdMUS5yrLk1dqp5Vrg==""
                        }
                    }
                ],
                ""generative"": { ""result"": ""yes"" }
            }
        ]
    }";

        JsonParser jsonParser = new JsonParser(
            JsonParser.Settings.Default.WithIgnoreUnknownFields(true)
        );
        V1.SearchReply reply = jsonParser.Parse<V1.SearchReply>(json);

        Assert.NotNull(reply);

        GenerativeGroupByResult res = reply;

        Assert.Contains("Teddy apples", res.Generative[0]);
        Assert.Single(res.Groups);
        var groups = res.Groups.Values.ToList();
        // Get the first object in the first group and check its generative result
        var firstGroupObject = groups[0];
        Assert.NotNull(firstGroupObject);
        Assert.NotNull(firstGroupObject.Generative);
        Assert.Equal("yes", firstGroupObject.Generative[0]);
        // Get the first object in the result set and check its group
        var firstObject = res.Objects[0];
        Assert.Equal("apples are big", firstObject.BelongsToGroup);
    }
}
