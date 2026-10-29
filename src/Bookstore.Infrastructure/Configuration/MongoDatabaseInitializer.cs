using MongoDB.Bson;
using MongoDB.Driver;

namespace Bookstore.Infrastructure.Configuration;

public sealed class MongoDatabaseInitializer(IMongoDatabase database)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var authors = database.GetCollection<BsonDocument>("authors");
        var normalizedNameIndex = new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("normalizedName"),
            new CreateIndexOptions { Unique = true, Name = "ux_authors_normalized_name" });

        await authors.Indexes.CreateOneAsync(
            normalizedNameIndex,
            cancellationToken: cancellationToken);

        var books = database.GetCollection<BsonDocument>("books");
        var authorIdIndex = new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("authorId"),
            new CreateIndexOptions { Name = "ix_books_author_id" });
        await books.Indexes.CreateOneAsync(
            authorIdIndex,
            cancellationToken: cancellationToken);
    }
}
