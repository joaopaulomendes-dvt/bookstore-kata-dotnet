using Bookstore.Domain.Authors;

namespace Bookstore.Application.Authors;

public interface IAuthorRepository
{
    Task<Author> CreateAsync(Author author, CancellationToken cancellationToken);

    Task<Author?> GetByIdAsync(string id, bool includeInactive, CancellationToken cancellationToken);

    Task<Author?> GetByNormalizedNameAsync(string normalizedName, bool includeInactive, CancellationToken cancellationToken);

    Task<IReadOnlyList<Author>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(string id, DateTimeOffset updatedAt, CancellationToken cancellationToken);
}
