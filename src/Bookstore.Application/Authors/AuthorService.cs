using Bookstore.Domain.Authors;
using Bookstore.Domain.Common;
using Bookstore.Application.Common;

namespace Bookstore.Application.Authors;

public sealed class AuthorService(IAuthorRepository repository, TimeProvider timeProvider)
{
    public async Task<Author> CreateAsync(
        string? name,
        string? biography,
        string? website,
        DateOnly? birthDate,
        string? nationality,
        string? email,
        CancellationToken cancellationToken)
    {
        var author = Author.Create(
            name,
            biography,
            website,
            birthDate,
            nationality,
            email,
            timeProvider.GetUtcNow());

        return await repository.CreateAsync(author, cancellationToken);
    }

    public Task<IReadOnlyList<Author>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        return repository.ListAsync(includeInactive, cancellationToken);
    }

    public async Task<Author?> GetByIdAsync(
        string id,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        if (!ObjectIdText.IsValid(id))
        {
            throw new DomainValidationException("Author ID must be a 24-character hexadecimal ObjectId.");
        }

        return await repository.GetByIdAsync(id, includeInactive, cancellationToken);
    }

    public async Task<bool> DeactivateAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectIdText.IsValid(id))
        {
            throw new DomainValidationException("Author ID must be a 24-character hexadecimal ObjectId.");
        }

        return await repository.DeactivateAsync(
            id,
            timeProvider.GetUtcNow(),
            cancellationToken);
    }
}
