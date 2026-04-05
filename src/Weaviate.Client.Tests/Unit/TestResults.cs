using Google.Protobuf;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The result tests class
/// </summary>
public class ResultTests
{
    /// <summary>
    /// Tests that search reply builds generative group by result response
    /// </summary>
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
        Grpc.Protobuf.V1.SearchReply reply = jsonParser.Parse<Grpc.Protobuf.V1.SearchReply>(json);

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

    [Fact]
    public void WeaviateResult_Has_QueryProfile_Property()
    {
        var profile = new QueryProfile { Shards = [] };
        var result = new WeaviateResult { Objects = [], QueryProfile = profile };
        Assert.Equal(profile, result.QueryProfile);
    }

    [Fact]
    public void GroupByResult_Has_QueryProfile_Property()
    {
        var profile = new QueryProfile { Shards = [] };
        var result = new GroupByResult([], new Dictionary<string, WeaviateGroup>())
        {
            QueryProfile = profile,
        };
        Assert.Equal(profile, result.QueryProfile);
    }

    [Fact]
    public void SearchReply_With_QueryProfile_Builds_WeaviateResult_With_Profile()
    {
        var json =
            @"{
            ""took"": 5.0,
            ""results"": [
                {
                    ""metadata"": { ""id"": ""00000000-0000-0000-0000-000000000001"" },
                    ""properties"": {}
                }
            ],
            ""queryProfile"": {
                ""shards"": [
                    {
                        ""name"": ""shard0"",
                        ""node"": ""node-1"",
                        ""searches"": {
                            ""vector"": {
                                ""details"": {
                                    ""total_took"": ""15.234ms"",
                                    ""vector_search_took"": ""10.1ms""
                                }
                            }
                        }
                    }
                ]
            }
        }";

        var jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
        var reply = jsonParser.Parse<Grpc.Protobuf.V1.SearchReply>(json);

        WeaviateResult result = reply;

        Assert.NotNull(result.QueryProfile);
        Assert.Single(result.QueryProfile.Shards);
        var shard = result.QueryProfile.Shards[0];
        Assert.Equal("shard0", shard.Name);
        Assert.Equal("node-1", shard.Node);
        Assert.True(shard.Searches.ContainsKey("vector"));
        Assert.Equal("15.234ms", shard.Searches["vector"].Details["total_took"]);
    }

    [Fact]
    public void SearchReply_Without_QueryProfile_Returns_Null_Profile()
    {
        var json =
            @"{
            ""took"": 3.0,
            ""results"": [
                {
                    ""metadata"": { ""id"": ""00000000-0000-0000-0000-000000000002"" },
                    ""properties"": {}
                }
            ]
        }";

        var jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
        var reply = jsonParser.Parse<Grpc.Protobuf.V1.SearchReply>(json);

        WeaviateResult result = reply;

        Assert.Null(result.QueryProfile);
    }

    [Fact]
    public void MetadataRequest_QueryProfile_Field_Is_Set_When_Requested()
    {
        var metadataReq = new Weaviate.Client.Grpc.Protobuf.V1.MetadataRequest
        {
            QueryProfile = true,
        };
        Assert.True(metadataReq.QueryProfile);
    }

    [Fact]
    public void MetadataQuery_QueryProfile_Flag_Works()
    {
        var q = new MetadataQuery(MetadataOptions.QueryProfile);
        Assert.True(q.QueryProfile);
        Assert.False(q.Score);

        var q2 = new MetadataQuery(MetadataOptions.Score | MetadataOptions.QueryProfile);
        Assert.True(q2.QueryProfile);
        Assert.True(q2.Score);
    }

    [Fact]
    public void QueryProfile_Model_Can_Be_Created()
    {
        var profile = new QueryProfile
        {
            Shards =
            [
                new ShardProfile
                {
                    Name = "shard0",
                    Node = "node1",
                    Searches = new Dictionary<string, SearchProfile>
                    {
                        ["vector"] = new SearchProfile
                        {
                            Details = new Dictionary<string, string>
                            {
                                ["total_took"] = "15.234ms",
                                ["vector_search_took"] = "10.1ms",
                            },
                        },
                    },
                },
            ],
        };

        Assert.Single(profile.Shards);
        Assert.Equal("shard0", profile.Shards[0].Name);
        Assert.Equal("15.234ms", profile.Shards[0].Searches["vector"].Details["total_took"]);
    }

    [Fact]
    public void ToTyped_WeaviateResult_Preserves_QueryProfile()
    {
        var profile = new QueryProfile { Shards = [] };
        var untyped = new WeaviateResult { Objects = [], QueryProfile = profile };

        var typed = untyped.ToTyped<object>();

        Assert.Equal(profile, typed.QueryProfile);
    }

    [Fact]
    public void ToTyped_GroupByResult_Preserves_QueryProfile()
    {
        var profile = new QueryProfile { Shards = [] };
        var untyped = new GroupByResult([], new Dictionary<string, WeaviateGroup>())
        {
            QueryProfile = profile,
        };

        var typed = untyped.ToTyped<object>();

        Assert.Equal(profile, typed.QueryProfile);
    }
}
