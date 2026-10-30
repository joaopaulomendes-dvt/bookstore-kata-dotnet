using Bookstore.Domain.Books;

namespace Bookstore.Application.Books;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> ListAsync(
        int? categoryId,
        string? authorId,
        string? exactAuthorName,
        string? authorNameSearch,
        CancellationToken cancellationToken);

    Task<Book?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<Book?> FindAsync(
        string? id,
        string? title,
        string? authorName,
        CancellationToken cancellationToken);

    Task<Book> CreateAsync(Book book, CancellationToken cancellationToken);

    Task<Book?> UpdateAsync(
        string id,
        int quantity,
        bool active,
        DateTime updatedAt,
        CancellationToken cancellationToken);
}
