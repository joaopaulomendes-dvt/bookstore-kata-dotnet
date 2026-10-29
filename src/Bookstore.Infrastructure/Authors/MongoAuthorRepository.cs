using Bookstore.Application.Authors;
using Bookstore.Domain.Authors;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Bookstore.Infrastructure.Authors;

public sealed class MongoAuthorRepository(IMongoDatabase database) : IAuthorRepository
{
    private readonly IMongoCollection<BsonDocument> _authors = database.GetCollection<BsonDocument>("authors");

    public async Task<Author> CreateAsync(Author author, CancellationToken cancellationToken)
    {
        var objectId = ObjectId.GenerateNewId();
        var persistedAuthor = author.WithId(objectId.ToString());
        var document = ToDocument(persistedAuthor, objectId);

        try
        {
            await _authors.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Code == 11000)
        {
            throw new AuthorAlreadyExistsException(author.NormalizedName);
        }

        return persistedAuthor;
    }

    public async Task<Author?> GetByIdAsync(
        string id,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
        if (!includeInactive)
        {
            filter &= ActiveFilter();
        }

        var document = await _authors.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToAuthor(document);
    }

    public async Task<Author?> GetByNormalizedNameAsync(
        string normalizedName,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("normalizedName", normalizedName);
        if (!includeInactive)
        {
            filter &= ActiveFilter();
        }

        var document = await _authors.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToAuthor(document);
    }

    public async Task<IReadOnlyList<Author>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var filter = includeInactive
            ? Builders<BsonDocument>.Filter.Empty
            : ActiveFilter();
        var documents = await _authors.Find(filter)
            .Sort(Builders<BsonDocument>.Sort.Ascending("normalizedName"))
            .ToListAsync(cancellationToken);

        return documents.Select(ToAuthor).ToArray();
    }

    public async Task<bool> DeactivateAsync(
        string id,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return false;
        }

        var idFilter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
        var existing = await _authors.Find(idFilter).FirstOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            return false;
        }

        if (!ReadBoolean(existing, "active", true))
        {
            return true;
        }

        var update = Builders<BsonDocument>.Update
            .Set("active", false)
            .Set("updatedAt", updatedAt.UtcDateTime);
        var result = await _authors.UpdateOneAsync(idFilter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount == 1;
    }

    private static FilterDefinition<BsonDocument> ActiveFilter()
    {
        return Builders<BsonDocument>.Filter.Ne("active", false);
    }

    private static BsonDocument ToDocument(Author author, ObjectId objectId)
    {
        var document = new BsonDocument
        {
            ["_id"] = objectId,
            ["name"] = author.Name,
            ["normalizedName"] = author.NormalizedName,
            ["active"] = author.Active,
            ["createdAt"] = author.CreatedAt.UtcDateTime,
            ["updatedAt"] = author.UpdatedAt.UtcDateTime
        };

        AddOptional(document, "biography", author.Biography);
        AddOptional(document, "website", author.Website);
        AddOptional(document, "birthDate", author.BirthDate is null
            ? null
            : author.BirthDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        AddOptional(document, "nationality", author.Nationality);
        AddOptional(document, "email", author.Email);
        return document;
    }

    private static Author ToAuthor(BsonDocument document)
    {
        var name = ReadString(document, "name") ?? string.Empty;
        var normalizedName = ReadString(document, "normalizedName") ?? Author.NormalizeName(name);
        var createdAt = ReadDateTime(document, "createdAt") ?? DateTime.UnixEpoch;
        var updatedAt = ReadDateTime(document, "updatedAt") ?? createdAt;
        DateOnly? birthDate = ReadDateTime(document, "birthDate") is { } date
            ? DateOnly.FromDateTime(date)
            : null;

        return Author.Restore(
            document["_id"].AsObjectId.ToString(),
            name,
            normalizedName,
            ReadString(document, "biography"),
            ReadString(document, "website"),
            birthDate,
            ReadString(document, "nationality"),
            ReadString(document, "email"),
            ReadBoolean(document, "active", true),
            new DateTimeOffset(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(updatedAt, DateTimeKind.Utc)));
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
