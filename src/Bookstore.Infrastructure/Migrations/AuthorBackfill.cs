using Bookstore.Application.Authors;
using Bookstore.Domain.Authors;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Bookstore.Infrastructure.Migrations;

public sealed class AuthorBackfill(
    IMongoDatabase database,
    IAuthorRepository authors,
    TimeProvider timeProvider)
{
    public async Task<AuthorBackfillReport> RunAsync(CancellationToken cancellationToken)
    {
        var books = database.GetCollection<BsonDocument>("books");
        var documents = await books.Find(Builders<BsonDocument>.Filter.Empty)
            .ToListAsync(cancellationToken);

        var linked = 0;
        var created = 0;
        var alreadyLinked = 0;
        var skipped = 0;

        foreach (var book in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!book.TryGetValue("_id", out var bookIdValue) || !bookIdValue.IsObjectId)
            {
                skipped++;
                continue;
            }

            if (HasValidAuthorId(book))
            {
                alreadyLinked++;
                continue;
            }

            if (!book.TryGetValue("author", out var authorValue) ||
                !authorValue.IsString ||
                string.IsNullOrWhiteSpace(authorValue.AsString))
            {
                skipped++;
                continue;
            }

            var authorName = authorValue.AsString.Trim();
            var normalizedName = Author.NormalizeName(authorName);
            var author = await authors.GetByNormalizedNameAsync(
                normalizedName,
                true,
                cancellationToken);

            if (author is null)
            {
                try
                {
                    author = await authors.CreateAsync(
                        Author.Create(authorName, null, null, null, null, null, timeProvider.GetUtcNow()),
                        cancellationToken);
                    created++;
                }
                catch (AuthorAlreadyExistsException)
                {
                    author = await authors.GetByNormalizedNameAsync(
                        normalizedName,
                        true,
                        cancellationToken);
                }
            }

            if (author is null || !ObjectId.TryParse(author.Id, out var authorObjectId))
            {
                skipped++;
                continue;
            }

            var bookId = bookIdValue.AsObjectId;
            var previousAuthorId = book.TryGetValue("authorId", out var existingAuthorId)
                ? existingAuthorId
                : BsonNull.Value;
            var filter = Builders<BsonDocument>.Filter.Eq("_id", bookId) &
                         Builders<BsonDocument>.Filter.Eq("authorId", previousAuthorId);
            var update = Builders<BsonDocument>.Update
                .Set("authorId", authorObjectId)
                .Set("authorMigrationVersion", 1);
            var result = await books.UpdateOneAsync(
                filter,
                update,
                cancellationToken: cancellationToken);

            if (result.ModifiedCount == 1)
            {
                linked++;
            }
            else
            {
                var currentBook = await books.Find(
                        Builders<BsonDocument>.Filter.Eq("_id", bookId))
                    .FirstOrDefaultAsync(cancellationToken);
                if (currentBook is not null && HasValidAuthorId(currentBook))
                {
                    alreadyLinked++;
                }
                else
                {
                    skipped++;
                }
            }
        }

        return new AuthorBackfillReport(linked, created, alreadyLinked, skipped);
    }

    private static bool HasValidAuthorId(BsonDocument book)
    {
        if (!book.TryGetValue("authorId", out var value))
        {
            return false;
        }

        return value.IsObjectId ||
               (value.IsString && ObjectId.TryParse(value.AsString, out _));
    }
}
