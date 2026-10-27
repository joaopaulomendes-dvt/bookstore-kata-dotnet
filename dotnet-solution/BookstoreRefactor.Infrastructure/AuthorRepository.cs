using BookstoreRefactor.Domain;
using MongoDB.Driver;

namespace BookstoreRefactor.Infrastructure;

public sealed class AuthorRepository : IAuthorRepository
{
    private readonly IMongoCollection<AuthorDocument> _authors;

    public AuthorRepository(MongoDbContext context)
    {
        _authors = context.Authors;
    }

    public async Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _authors.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : document.ToDomain();
    }

    public async Task<IReadOnlyCollection<Author>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _authors.Find(_ => true).ToListAsync(cancellationToken);
        return documents.Select(x => x.ToDomain()).ToList();
    }

    public async Task AddAsync(Author author, CancellationToken cancellationToken = default)
    {
        await _authors.InsertOneAsync(author.ToDocument(), new InsertOneOptions(), cancellationToken);
    }

    public async Task UpdateAsync(Author author, CancellationToken cancellationToken = default)
    {
        await _authors.ReplaceOneAsync(x => x.Id == author.Id, author.ToDocument(), cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _authors.CountDocumentsAsync(x => x.Id == id, cancellationToken: cancellationToken) > 0;
    }
}
