using BookstoreRefactor.Domain;
using MongoDB.Driver;

namespace BookstoreRefactor.Infrastructure;

public sealed class BookRepository : IBookRepository
{
    private readonly IMongoCollection<BookDocument> _books;

    public BookRepository(MongoDbContext context)
    {
        _books = context.Books;
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _books.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : document.ToDomain();
    }

    public async Task<IReadOnlyCollection<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _books.Find(_ => true).ToListAsync(cancellationToken);
        return documents.Select(x => x.ToDomain()).ToList();
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _books.InsertOneAsync(book.ToDocument(), new InsertOneOptions(), cancellationToken);
    }

    public async Task UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _books.ReplaceOneAsync(x => x.Id == book.Id, book.ToDocument(), cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _books.CountDocumentsAsync(x => x.Id == id, cancellationToken: cancellationToken) > 0;
    }
}
