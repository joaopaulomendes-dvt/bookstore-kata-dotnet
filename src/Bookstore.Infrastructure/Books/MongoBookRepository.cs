using System.Text.RegularExpressions;
using Bookstore.Application.Books;
using Bookstore.Domain.Books;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Bookstore.Infrastructure.Books;

public sealed class MongoBookRepository(IMongoDatabase database) : IBookRepository
{
    private readonly IMongoCollection<BsonDocument> _books = database.GetCollection<BsonDocument>("books");

    public async Task<IReadOnlyList<Book>> ListAsync(
        int? categoryId,
        string? authorId,
        string? exactAuthorName,
        string? authorNameSearch,
        CancellationToken cancellationToken)
    {
        var filter = Builders<BsonDocument>.Filter.Empty;
        if (categoryId is not null)
        {
            filter &= Builders<BsonDocument>.Filter.Eq("categoryId", categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(authorId) && ObjectId.TryParse(authorId, out var objectId))
        {
            filter &= Builders<BsonDocument>.Filter.Eq("authorId", objectId);
        }

        if (!string.IsNullOrWhiteSpace(exactAuthorName))
        {
            filter &= Builders<BsonDocument>.Filter.Eq("author", exactAuthorName);
        }

        if (!string.IsNullOrWhiteSpace(authorNameSearch))
        {
            var escapedSearch = Regex.Escape(authorNameSearch);
            filter &= Builders<BsonDocument>.Filter.Regex(
                "author",
                new BsonRegularExpression(escapedSearch, "i"));
        }

        var documents = await _books.Find(filter).ToListAsync(cancellationToken);
        return documents.Select(ToBook).ToArray();
    }

    public async Task<Book?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var document = await _books.Find(
                Builders<BsonDocument>.Filter.Eq("_id", objectId))
            .FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToBook(document);
    }

    public async Task<Book?> FindAsync(
        string? id,
        string? title,
        string? authorName,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            return await GetByIdAsync(id, cancellationToken);
        }

        FilterDefinition<BsonDocument>? filter = null;
        if (!string.IsNullOrWhiteSpace(title))
        {
            filter = Builders<BsonDocument>.Filter.Eq("title", title);
        }
        else if (!string.IsNullOrWhiteSpace(authorName))
        {
            filter = Builders<BsonDocument>.Filter.Eq("author", authorName);
        }

        if (filter is null)
        {
            return null;
        }

        var document = await _books.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToBook(document);
    }

    public async Task<Book> CreateAsync(Book book, CancellationToken cancellationToken)
    {
        var objectId = ObjectId.GenerateNewId();
        var document = ToDocument(book, objectId);
        await _books.InsertOneAsync(document, cancellationToken: cancellationToken);
        return book with { Id = objectId.ToString() };
    }

    public async Task<Book?> UpdateAsync(
        string id,
        int quantity,
        bool active,
        DateTime updatedAt,
        CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
        var update = Builders<BsonDocument>.Update
            .Set("quantity", quantity)
            .Set("active", active)
            .Set(
                "inactiveDate",
                active
                    ? (BsonValue)BsonNull.Value
                    : BsonDateTime.Create(DateTime.SpecifyKind(updatedAt, DateTimeKind.Utc)));
        var updated = await _books.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<BsonDocument>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);

        return updated is null ? null : ToBook(updated);
    }

    private static BsonDocument ToDocument(Book book, ObjectId objectId)
    {
        var document = new BsonDocument
        {
            ["_id"] = objectId,
            ["categoryId"] = book.CategoryId,
            ["quantity"] = book.Quantity,
            ["active"] = book.Active
        };

        AddOptional(document, "title", book.Title);
        AddOptional(document, "author", book.Author);
        AddOptional(document, "description", book.Description);
        AddOptional(document, "language", book.Language);
        AddOptional(document, "inactiveDate", book.InactiveDate);
        AddOptional(document, "publisher", book.Publisher);
        AddOptional(document, "publisherDate", book.PublisherDate);

        if (ObjectId.TryParse(book.AuthorId, out var authorObjectId))
        {
            document["authorId"] = authorObjectId;
        }

        return document;
    }

    private static Book ToBook(BsonDocument document)
    {
        var authorId = document.TryGetValue("authorId", out var authorIdValue)
            ? authorIdValue.IsObjectId
                ? authorIdValue.AsObjectId.ToString()
                : authorIdValue.IsString
                    ? authorIdValue.AsString
                    : null
            : null;

        return new Book(
            document["_id"].AsObjectId.ToString(),
            ReadString(document, "title"),
            authorId,
            ReadString(document, "author"),
            ReadInt32(document, "categoryId"),
            ReadInt32(document, "quantity"),
            ReadString(document, "description"),
            ReadString(document, "language"),
            ReadBoolean(document, "active", false),
            ReadDateTime(document, "inactiveDate"),
            ReadString(document, "publisher"),
            ReadDateTime(document, "publisherDate"));
    }

    private static void AddOptional(BsonDocument document, string name, string? value)
    {
        if (value is not null)
        {
            document[name] = value;
        }
    }

    private static void AddOptional(BsonDocument document, string name, DateTime? value)
    {
        if (value is not null)
        {
            document[name] = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        }
    }

    private static string? ReadString(BsonDocument document, string name)
    {
        return document.TryGetValue(name, out var value) && value.IsString
            ? value.AsString
            : null;
    }

    private static int ReadInt32(BsonDocument document, string name)
    {
        return document.TryGetValue(name, out var value) && value.IsInt32
            ? value.AsInt32
            : 0;
    }

    private static bool ReadBoolean(BsonDocument document, string name, bool fallback)
    {
        return document.TryGetValue(name, out var value) && value.IsBoolean
            ? value.AsBoolean
            : fallback;
    }

    private static DateTime? ReadDateTime(BsonDocument document, string name)
    {
        return document.TryGetValue(name, out var value) && value.IsBsonDateTime
            ? value.ToUniversalTime()
            : null;
    }
}
