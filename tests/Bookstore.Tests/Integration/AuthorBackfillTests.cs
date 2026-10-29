using Bookstore.Infrastructure.Migrations;
using Bookstore.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Bookstore.Tests.Integration;

[Collection(MongoDbCollection.Name)]
public sealed class AuthorBackfillTests(MongoDbFixture fixture)
{
    [Fact]
    public async Task RunAsync_LegacyBooksAndRepeatedRun_LinksOnceWithoutDuplicates()
    {
        using var factory = fixture.CreateFactory();
        _ = factory.Services;
        var database = factory.Services.GetRequiredService<IMongoDatabase>();
        var books = database.GetCollection<BsonDocument>("books");
        await books.InsertManyAsync(
        [
            new BsonDocument
            {
                ["_id"] = ObjectId.GenerateNewId(),
                ["author"] = "  Octavia Butler "
            },
            new BsonDocument
            {
                ["_id"] = ObjectId.GenerateNewId(),
                ["author"] = "octavia   butler"
            },
            new BsonDocument
            {
                ["_id"] = ObjectId.GenerateNewId(),
                ["author"] = "OCTAVIA BUTLER",
                ["authorId"] = "malformed"
            },
            new BsonDocument
            {
                ["_id"] = ObjectId.GenerateNewId(),
                ["author"] = " "
            }
        ]);

        var backfill = factory.Services.GetRequiredService<AuthorBackfill>();
        var firstRun = await backfill.RunAsync(CancellationToken.None);
        var secondRun = await backfill.RunAsync(CancellationToken.None);

        Assert.Equal(3, firstRun.BooksLinked);
        Assert.Equal(1, firstRun.AuthorsCreated);
        Assert.Equal(1, firstRun.BooksSkipped);
        Assert.Equal(0, secondRun.BooksLinked);
        Assert.Equal(0, secondRun.AuthorsCreated);
        Assert.Equal(3, secondRun.BooksAlreadyLinked);
        Assert.Equal(1, secondRun.BooksSkipped);
    }
}
