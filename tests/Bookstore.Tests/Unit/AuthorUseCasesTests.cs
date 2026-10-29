using Bookstore.Application.Authors;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Common;

namespace Bookstore.Tests.Unit;

public sealed class AuthorUseCasesTests
{
    private const string AuthorId = "507f1f77bcf86cd799439011";

    [Fact]
    public async Task CreateAsync_UniqueName_ReturnsAuthorWithGeneratedId()
    {
        var repository = new InMemoryAuthorRepository();
        var service = new AuthorService(repository, new FixedTimeProvider(DateTimeOffset.UtcNow));

        var author = await service.CreateAsync(
            "Octavia Butler",
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None);

        Assert.Equal("Octavia Butler", author.Name);
        Assert.Equal(AuthorId, author.Id);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNormalizedName_ThrowsConflictException()
    {
        var repository = new InMemoryAuthorRepository();
        var service = new AuthorService(repository, new FixedTimeProvider(DateTimeOffset.UtcNow));

        await service.CreateAsync("Octavia Butler", null, null, null, null, null, CancellationToken.None);

        await Assert.ThrowsAsync<AuthorAlreadyExistsException>(() =>
            service.CreateAsync(" octavia   butler ", null, null, null, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ThrowsValidationException()
    {
        var service = new AuthorService(
            new InMemoryAuthorRepository(),
            new FixedTimeProvider(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.GetByIdAsync("invalid", false, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_InactiveAuthor_IsReturnedOnlyWhenIncluded()
    {
        var repository = new InMemoryAuthorRepository();
        var service = new AuthorService(repository, new FixedTimeProvider(DateTimeOffset.UtcNow));
        var author = await service.CreateAsync("Octavia Butler", null, null, null, null, null, CancellationToken.None);
        await service.DeactivateAsync(author.Id, CancellationToken.None);

        Assert.Null(await service.GetByIdAsync(author.Id, false, CancellationToken.None));
        Assert.False((await service.GetByIdAsync(author.Id, true, CancellationToken.None))!.Active);
    }

    [Fact]
    public async Task ListAsync_InactiveAuthor_IsReturnedOnlyWhenIncluded()
    {
        var repository = new InMemoryAuthorRepository();
        var service = new AuthorService(repository, new FixedTimeProvider(DateTimeOffset.UtcNow));
        var author = await service.CreateAsync("Octavia Butler", null, null, null, null, null, CancellationToken.None);
        await service.DeactivateAsync(author.Id, CancellationToken.None);

        Assert.Empty(await service.ListAsync(false, CancellationToken.None));
        Assert.Single(await service.ListAsync(true, CancellationToken.None));
    }

    [Fact]
    public async Task DeactivateAsync_InvalidId_ThrowsValidationException()
    {
        var service = new AuthorService(
            new InMemoryAuthorRepository(),
            new FixedTimeProvider(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.DeactivateAsync("invalid", CancellationToken.None));
    }

    [Fact]
    public async Task DeactivateAsync_ValidId_ReturnsFalseWhenAuthorDoesNotExist()
    {
        var service = new AuthorService(
            new InMemoryAuthorRepository(),
            new FixedTimeProvider(DateTimeOffset.UtcNow));

        Assert.False(await service.DeactivateAsync(AuthorId, CancellationToken.None));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InMemoryAuthorRepository : IAuthorRepository
    {
        private readonly Dictionary<string, Author> _authors = new(StringComparer.Ordinal);

        public Task<Author> CreateAsync(Author author, CancellationToken cancellationToken)
        {
            if (_authors.Values.Any(existing => existing.NormalizedName == author.NormalizedName))
            {
                throw new AuthorAlreadyExistsException(author.NormalizedName);
            }

            var persisted = author.WithId(_authors.Count == 0 ? AuthorId : $"507f1f77bcf86cd7994390{_authors.Count:D2}");
            _authors.Add(persisted.Id, persisted);
            return Task.FromResult(persisted);
        }

        public Task<Author?> GetByIdAsync(string id, bool includeInactive, CancellationToken cancellationToken)
        {
            _authors.TryGetValue(id, out var author);
            return Task.FromResult(author is not null && (includeInactive || author.Active) ? author : null);
        }

        public Task<Author?> GetByNormalizedNameAsync(
            string normalizedName,
            bool includeInactive,
            CancellationToken cancellationToken)
        {
            var author = _authors.Values.FirstOrDefault(candidate =>
                candidate.NormalizedName == normalizedName && (includeInactive || candidate.Active));
            return Task.FromResult(author);
        }

        public Task<IReadOnlyList<Author>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
        {
            IReadOnlyList<Author> authors = _authors.Values
                .Where(author => includeInactive || author.Active)
                .ToArray();
            return Task.FromResult(authors);
        }

        public Task<bool> DeactivateAsync(
            string id,
            DateTimeOffset updatedAt,
            CancellationToken cancellationToken)
        {
            if (!_authors.TryGetValue(id, out var author))
            {
                return Task.FromResult(false);
            }

            _authors[id] = author.Deactivate(updatedAt);
            return Task.FromResult(true);
        }
    }
}
