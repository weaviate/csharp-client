using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
    readonly Guid TO_UUID = new Guid("8ad0d33c-8db1-4437-87f3-72161ca2a51a");
    readonly Guid TO_UUID2 = new Guid("577887c1-4c6b-5594-aa62-f0c17883d9cf");

    [Fact]
    public async Task SingleTargetReference()
    {
        // Arrange

        var cA = await CollectionFactory<TestData>("A", "Collection A");

        var uuid_A1 = await cA.Data.Insert(new TestData() { Name = "A1" });
        var uuid_A2 = await cA.Data.Insert(new TestData() { Name = "A2" });

        var cB = await CollectionFactory<TestData>(
            name: "B",
            description: "Collection B",
            references: [Property.Reference("a", cA.Name)]
        );

        var uuid_B = await cB.Data.Insert(new() { Name = "B" }, references: [("a", uuid_A1)]);

        await cB.Data.ReferenceAdd(from: uuid_B, fromProperty: "a", to: uuid_A2);

        var cC = await CollectionFactory<TestData>(
            "C",
            "Collection C",
            references: [Property.Reference("b", cB.Name)]
        );

        var uuid_C = await cC.Data.Insert(
            new TestData { Name = "find me" },
            references: [("b", uuid_B)]
        );

        // Act
        var aObjs = await cA.Query.BM25(query: "A1", ["name"]);

        var bObjs = await cB.Query.BM25(
            query: "B",
            references: [new QueryReference(linkOn: "a", fields: ["name"])]
        );

        var cObjs = await cC.Query.BM25(
            query: "find",
            fields: ["name"],
            references:
            [
                new QueryReference(
                    linkOn: "b",
                    fields: ["name"],
                    metadata: MetadataOptions.LastUpdateTime,
                    references: [new QueryReference(linkOn: "a", fields: ["name"])]
                ),
            ]
        );

        // Assert
        var aObjsObjects = aObjs.Objects.ToList();
        var bObjsObjects = bObjs.Objects.ToList();
        var cObjsObjects = cObjs.Objects.ToList();

        Assert.Equal(cA.Name, aObjsObjects[0].Collection);
        Assert.NotNull(aObjsObjects[0].As<TestData>()?.Name);
        Assert.Equal("A1", aObjsObjects[0].As<TestData>()?.Name);

        Assert.Equal(cA.Name, bObjsObjects[0].References["a"][0].Collection);
        Assert.Equal("A1", bObjsObjects[0].References["a"][0].Properties["name"]);
        Assert.Equal(uuid_A1, bObjsObjects[0].References["a"][0].ID);
        Assert.Equal(cA.Name, bObjsObjects[0].References["a"][1].Collection);
        Assert.Equal("A2", bObjsObjects[0].References["a"][1].Properties["name"]);
        Assert.Equal(uuid_A2, bObjsObjects[0].References["a"][1].ID);

        Assert.Equal(cC.Name, cObjsObjects[0].Collection);
        Assert.Equal("find me", cObjsObjects[0].Properties["name"]);
        Assert.Equal(uuid_C, cObjsObjects[0].ID);
        Assert.Equal(cB.Name, cObjsObjects[0].References["b"][0].Collection);
        Assert.Equal("B", cObjsObjects[0].References["b"][0].Properties["name"]);
        Assert.NotNull(cObjsObjects[0].References["b"][0].Metadata.LastUpdateTime);
        Assert.Equal(cA.Name, cObjsObjects[0].References["b"][0].References["a"][0].Collection);
        Assert.Equal(
            "A1",
            cObjsObjects[0].References["b"][0].References["a"][0].Properties["name"]
        );
        Assert.Equal(cA.Name, cObjsObjects[0].References["b"][0].References["a"][1].Collection);
        Assert.Equal(
            "A2",
            cObjsObjects[0].References["b"][0].References["a"][1].Properties["name"]
        );
    }

    [Fact]
    public async Task SingleTargetReference_MovieReviews()
    {
        // Arrange
        var movies = await CollectionFactory<dynamic>(
            name: "Movie",
            "Movies",
            properties:
            [
                Property.Text("title"),
                Property.Text("overview"),
                Property.Int("movie_id"),
                Property.Date("release_date"),
                Property.Int("vote_count"),
            ]
        );

        var reviews = await CollectionFactory<dynamic>(
            name: "Review",
            "Movie Reviews",
            properties:
            [
                Property.Text("author_username"),
                Property.Text("content"),
                Property.Int("rating"),
                Property.Int("review_id"),
                Property.Int("movie_id"),
            ],
            references: [Property.Reference("forMovie", targetCollection: movies.Name)],
            vectorConfig: NamedVectorConfig.New("default", new Text2VecContextionaryConfig())
        );

        var moviesData = new[]
        {
            new
            {
                movie_id = 162,
                title = "Edward Scissorhands",
                overview = "A small suburban town receives a visit from a castaway unfinished science experiment named Edward.",
                release_date = DateTime.Parse("1990-12-07").ToUniversalTime(),
                vote_count = 12308,
            },
            new
            {
                movie_id = 769,
                title = "GoodFellas",
                overview = "The true story of Henry Hill, a half-Irish, half-Sicilian Brooklyn kid who is adopted by neighbourhood gangsters at an early age and climbs the ranks of a Mafia family under the guidance of Jimmy Conway.",
                release_date = DateTime.Parse("1990-09-12").ToUniversalTime(),
                vote_count = 12109,
            },
            new
            {
                movie_id = 771,
                title = "Home Alone",
                overview = "Eight-year-old Kevin McCallister makes the most of the situation after his family unwittingly leaves him behind when they go on Christmas vacation. But when a pair of bungling burglars set their sights on Kevin's house, the plucky kid stands ready to defend his territory. By planting booby traps galore, adorably mischievous Kevin stands his ground as his frantic mother attempts to race home before Christmas Day.",
                release_date = DateTime.Parse("1990-11-16").ToUniversalTime(),
                vote_count = 10601,
            },
        };

        var movieIds = new Dictionary<int, Guid>();
        foreach (var m in moviesData)
        {
            var uuid = await movies.Data.Insert(m);
            movieIds.Add(m.movie_id, uuid);
        }

        var reviewsData = new List<dynamic>
        {
            new
            {
                author_username = "kineticandroid",
                content = @"Take the story of Frankenstein's monster, remove the hateful creator, and replace the little girl's flowers with a brightly pastel Reagan-era suburb. Though not my personal favorite Tim Burton film, I feel like this one best encapsulates his style and story interests.",
                rating = (double?)null,
                movie_id = 162,
                review_id = 162,
            },
            new
            {
                author_username = "r96sk",
                content = @"Very enjoyable.

It's funny the way we picture things in our minds. I had heard of <em>'Edward Scissorhands'</em> but actually knew very little about it, typified by the fact I was expecting this to be very dark - probably just based on the seeing the cover here and there. It's much sillier than expected, but in a positive way.

I do kinda end up wishing they went down a more dark/creative route, instead of relying on the novelty of having scissors as hands; though, to be fair, they do touch on the deeper side a bit. With that said, I did get a good amount of entertainment seeing this plot unfold. It's weird and wonderful.

Johnny Depp is a great actor and is very good here, mainly via his facial expressions and body language. It's cool to see Winona Ryder involved, someone I've thoroughly enjoyed in more recent times in <em>'Stranger Things'</em>. Alan Arkin and Anthony Michael Hall also appear.

The film looks neat, as I've come to expect from Tim Burton. It has the obvious touch of Bo Welch to it, with the neighbourhood looking not too dissimilar to what Welch would create for 2003's <em>'The Cat in the Hat'</em> - which I, truly, enjoyed.

Undoubtedly worth a watch.",
                rating = (double?)8.0,
                movie_id = 162,
                review_id = 162,
            },
            new
            {
                author_username = "SoSmooth1982",
                content = @"Love this movie. It's like a non evil Freddy Kruger. The ending could have been better though.",
                rating = (double?)8.0,
                movie_id = 162,
                review_id = 162,
            },
            new
            {
                author_username = "Geronimo1967",
                content = @"Vincent Price has spent his life working on a labour of love - a ""son"", an artificially constructed person that lacks only hands - for which he temporarily has two pairs of scissors. Sadly, the creator dies before he can rectify this and so young ""Edward"" (Johnny Depp) is left alone in his lofty castle. Alone, that is until a kindly Dianne Wiest (""Peg"") takes him under her wing, introduces him to her many friends - including an on-form Winona Ryder (""Kim"") - and they all discover he has a remarkable ability for topiary (and hairdressing!). Soon he is all the rage, the talk of the town - but always the misfit, and of course when a mishap - in this case a robbery for which he is framed - occurs, his fickle friends turn on him readily. It's a touching tale of innocence and humanity; Depp plays his role skilfully and with delicacy and humour, and the last half hour is quite a damning indictment of thoughtlessness and selfishness that still resonates today. Like many ""fairy"" tales, it has it's root in decent morals and Tim Burton is ahead of the game in delivering a nuanced and enjoyable modern day parable that makes you laugh, smile and wince with shame in equal measure.",
                rating = (double?)7.0,
                movie_id = 162,
                review_id = 162,
            },
            new
            {
                author_username = "John Chard",
                content = @"In a world that's powered by violence, on the streets where the violent have power, a new generation carries on an old tradition.

Martin Scorsese’s Goodfellas is without question one of the finest gangster movies ever made, a benchmark even. It’s that rare occasion for a genre film of this type where everything artistically comes together as one. Direction, script, editing, photography, driving soundtrack and crucially an ensemble cast firing on all cylinders. It’s grade “A” film making that marked a return to form for Scorsese whilst simultaneously showing the director at the summit of his directing abilities.

The story itself, based on Nicholas Pileggi’s non-fiction book Wiseguy, pulls absolutely no punches in its stark realisation of the Mafia lifestyle. It’s often brutal, yet funny, unflinching yet stylish, but ultimately from first frame to last it holds the attention, toying with all the human emotions during the journey, tingling the senses of those who were by 1990 fed up of popcorn movie fodder. 

It’s not romanticism here, if anything it’s a debunking of the Mafia myth, but even as the blood flows and the dialogue crackles with electricity, it always remains icy cool, brought to us by a man who had is eyes and ears open while growing up in Queens, New York in the 40s and 50s. Eccellente! 9/10",
                rating = (double?)9.0,
                movie_id = 769,
                review_id = 769,
            },
            new
            {
                author_username = "Ahmetaslan27",
                content = @"Martin Scorsese (director) always loves details in crime films, but he is not primarily interested in the crime itself. That is why his films are always produced with details that you may see as unimportant to you, especially if you want to see the movie for the purpose of seeing scenes of theft, murder, and so on, but you see the opposite. Somewhat other details are visible on the scene mostly

The film talks about liberation, stereotypes, and entering a new world for humanity. It was Ray Liotta (Henry). He wanted, as I said, to break free from stereotypes and enter the world of gangs.

Martin Scorsese (the director) filmed this unfamiliar life and directed it in the form of a film similar to documentaries because he filmed it as if it were a real, realistic life. That is why the presence of Voice Over was important in order to give you the feeling that there is a person sitting next to you telling you the story while whispering in your ear as it happens in the movies documentaries.",
                rating = (double?)7.0,
                movie_id = 769,
                review_id = 769,
            },
            new
            {
                author_username = "Geronimo1967",
                content = @"Ray Liotta is superb here as ""Henry Hill"", a man whom ever since he was young has been captivated by the mob. He starts off as a runner and before too long has ingratiated himself with the local fraternity lead by ""Paulie"" (Paul Sorvino) and is best mates with fellow hoods, the enigmatic and devious ""Jimmy"" (Robert De Niro) and the excellently vile ""Tommy"" (Joe Pesci). They put together an audacious robbery at JFK and are soon the talk of the town, but the latter in the trio is a bit of a live-wire and when he goes just a bit too far one night, the three of them find that their really quite idyllic lives of extortion and larceny start to go awry - and it's their own who are on their tracks. Scorsese takes him time with this story: the development of the characters - their personalities, trust, inter-reliance, sometimes divided, fractured, loyalties and ruthlessness and are built up in a thoroughly convincing fashion. We can, ourselves, see the obvious attractions for the young ""Henry"" of a life so very far removed from his working class Irish-Italian background - the wine, the women, the thrills; it's tantalising! If anything let's it down it's the last half hour; it's just a little too predictable and having spent so long building up the characters, we seem to be in just a bit too much of a rush; but that is a nit-pick. It's not the ""Godfather"" but it is not far short.",
                rating = (double?)7.0,
                movie_id = 769,
                review_id = 769,
            },
            new
            {
                author_username = "Ruuz",
                content = @"Doesn't really work if you actually spend the time to bother thinking about it, but so long as you don't _Home Alone_ is a pretty good time. There's really no likeable character, and it's honestly pretty mean spirited, but sometimes that's what you might need to defrag over Christmas. 

_Final rating:★★★ - I liked it. Would personally recommend you give it a go._",
                rating = (double?)6.0,
                movie_id = 771,
                review_id = 771,
            },
            new
            {
                author_username = "SoSmooth1982",
                content = @"Love this movie. I was 8 when this came out. I remember being so jealous of Kevin, because I wished I could be home alone like that to do whatever I wanted.",
                rating = (double?)10.0,
                movie_id = 771,
                review_id = 771,
            },
            new
            {
                author_username = "Geronimo1967",
                content = @"It has taken me 30 years to sit down and watch this film and I'm quite glad I finally did. I usually loathe kids movies, and the trails at the time always put me off - but Macauley Culkin is really quite a charmer in this tale of a youngster who is accidentally left at home at Christmas by his family. They have jetted off to Paris leaving him alone facing the unwanted attentions of two would-be burglars (Joe Pesci & Daniel Stern). Initially a bit unsettled, he is soon is his stride using just about every gadget (and critter) in their large family home to make sure he thwarts their thieving intentions. It's really all about the kid - and this one delivers well. The slapstick elements of the plot are designed to raise a smile, never to maim - even if having your head set on fire by a blow torch, or being walloped in the face by an hot iron might do longer term damage than happens here. That's the fun of it, for fun it is - it's a modern day Laurel & Hardy style story with an ending that's never in doubt. It does have a slightly more serious purpose, highlighting loneliness - not just for ""Kevin"" but his elderly neighbour ""Marley"" (Roberts Blossom) and it has that lovely scene on the aircraft when mother Catherine O'Hara realises that it wasn't just the garage doors that they forgot to sort out before they left! A great, and instantly recognisable score from maestro John Williams tops it all off nicely.",
                rating = (double?)7.0,
                movie_id = 771,
                review_id = 771,
            },
            new
            {
                author_username = "narrator56",
                content = @"Of course we watched this more than 20 years ago, but recently took it out of the library to watch again for a couple of reasons. One, it is ostensibly a holiday movie and we were watching a series of them. Also, a friend had just lost a loved pet and needed a silly movie to take her mind away for a couple of hours.

This movie fit the bill. It has several laugh out loud scenes, and mildly amusing material surrounding those scenes. The ensemble cast is fine. Catherine O’Hara is a believable mom and I have liked Daniel Stern ever since he couldn’t understand how a VCR works in City Slickers.

If you are one of those gentle souls like our friend who has difficulty distinguishing between cartoonish fictional violence and reality, you will need to look away a few times.

It won’t make the regular rotation of our traditional holiday movies, but I am glad we fit it in this year.",
                rating = (double?)9.0,
                movie_id = 771,
                review_id = 771,
            },
        };

        foreach (var r in reviewsData)
        {
            // Using dynamic make somo implicit conversions impossible.
            Guid movieId = movieIds[(int)r.movie_id];
            ObjectReference movieRef = ("forMovie", movieId);
            await reviews.Data.Insert(r, references: new List<ObjectReference>() { movieRef });
        }

        // Act
        var fun = await reviews.Query.NearText("Fun for the whole family", limit: 2);
        Console.WriteLine(
            JsonSerializer.Serialize(fun, new JsonSerializerOptions { WriteIndented = true })
        );

        var disappointed = await reviews.Query.NearText(
            "Disapointed by this movie",
            limit: 2,
            references: [new QueryReference("forMovie", ["title"])]
        );
        Console.WriteLine(
            JsonSerializer.Serialize(
                disappointed,
                new JsonSerializerOptions { WriteIndented = true }
            )
        );

        // Assert
        var funObjects = fun.Objects.ToList();
        var disappointedObjects = disappointed.Objects.ToList();

        Assert.NotNull(fun);
        Assert.Equal(2, funObjects.Count);
        Assert.Equal(0, funObjects[0]?.References.Count);
        Assert.Equal(0, funObjects[1]?.References.Count);

        Assert.NotNull(disappointed);
        Assert.Equal(2, disappointedObjects.Count);
        Assert.Equal(1, disappointedObjects[0]?.References.Count);
        Assert.Equal(1, disappointedObjects[1]?.References.Count);

        Assert.True(disappointedObjects[0].References.ContainsKey("forMovie"));
        Assert.True(disappointedObjects[1].References.ContainsKey("forMovie"));
        Assert.Equal(movieIds[162], disappointedObjects[0].References["forMovie"][0].ID);
        Assert.Equal(movieIds[771], disappointedObjects[1].References["forMovie"][0].ID);
    }
}
