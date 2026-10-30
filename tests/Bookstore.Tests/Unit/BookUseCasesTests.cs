using Bookstore.Application.Authors;
using Bookstore.Application.Books;
using Bookstore.Application.Common;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Domain.Common;

namespace Bookstore.Tests.Unit;

public sealed class BookUseCasesTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);
    private static readonly string AuthorId = "507f1f77bcf86cd799439011";

    [Fact]
    public async Task ListAsync_MapsCategoryAndAuthorFilters()
    {
        var books = new InMemoryBookRepository();
        var service = CreateService(books: books);

        await service.ListAsync("programming", AuthorId, "Octavia Butler", null, CancellationToken.None);

        Assert.Equal(6, books.LastCategoryId);
        Assert.Equal(AuthorId, books.LastAuthorId);
        Assert.Equal("Octavia Butler", books.LastAuthorName);
    }

    [Fact]
    public async Task ListAsync_AuthorNameSearch_TrimsAndPassesToRepository()
    {
        var books = new InMemoryBookRepository();
        var service = CreateService(books: books);

        await service.ListAsync(null, null, null, "  butler  ", CancellationToken.None);

        Assert.Equal("butler", books.LastAuthorNameSearch);
    }

    [Fact]
    public async Task ListAsync_AuthorNameSearch_RejectsTermsLongerThan255Characters()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.ListAsync(null, null, null, new string('a', 256), CancellationToken.None));
    }

    [Fact]
    public async Task ListAsync_AuthorNameSearch_Accepts255CharacterTerm()
    {
        var books = new InMemoryBookRepository();
        var service = CreateService(books: books);
        var search = new string('a', 255);

        await service.ListAsync(null, null, null, search, CancellationToken.None);

        Assert.Equal(search, books.LastAuthorNameSearch);
    }

    [Fact]
    public async Task ListAsync_WhitespaceAuthorNameSearch_DoesNotFilter()
    {
        var books = new InMemoryBookRepository();
        var service = CreateService(books: books);

        await service.ListAsync(null, null, null, "   ", CancellationToken.None);

        Assert.Null(books.LastAuthorNameSearch);
    }

    [Fact]
    public async Task ListAsync_UnsupportedCategory_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.ListAsync("unknown", null, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task ListAsync_InvalidAuthorId_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.ListAsync(null, "invalid", null, null, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.GetByIdAsync("bad-id", CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsRepositoryResult()
    {
        var service = CreateService();

        Assert.Null(await service.GetByIdAsync(AuthorId, CancellationToken.None));
    }

    [Fact]
    public async Task FindAsync_InvalidId_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.FindAsync("bad-id", null, null, CancellationToken.None));
    }

    [Fact]
    public async Task FindAsync_ValidId_DelegatesToRepository()
    {
        var books = new InMemoryBookRepository();
        var service = CreateService(books);

        await service.FindAsync(AuthorId, null, null, CancellationToken.None);

        Assert.Equal(AuthorId, books.LastFindId);
    }

    [Fact]
    public async Task CreateAsync_WithAuthorId_LinksActiveAuthor()
    {
        var authorRepository = new InMemoryAuthorRepository();
        var author = await authorRepository.CreateAsync(
            Author.Create("Octavia Butler", null, null, null, null, null, Now),
            CancellationToken.None);
        var books = new InMemoryBookRepository();
        var service = CreateService(books, authorRepository);

        var created = await service.CreateAsync(Command(authorId: author.Id), CancellationToken.None);

        Assert.Equal(author.Id, created.AuthorId);
        Assert.Equal("Octavia Butler", created.Author);
        Assert.Equal(6, created.CategoryId);
    }

    [Fact]
    public async Task CreateAsync_WithLegacyAuthorName_CreatesMinimalAuthorAndLinksIt()
    {
        var authors = new InMemoryAuthorRepository();
        var books = new InMemoryBookRepository();
        var service = CreateService(books, authors);

        var created = await service.CreateAsync(Command(author: "Octavia Butler"), CancellationToken.None);

        Assert.Equal("Octavia Butler", created.Author);
        Assert.NotNull(created.AuthorId);
        Assert.NotNull(await authors.GetByIdAsync(created.AuthorId, false, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithLegacyNameMatchingExistingAuthor_ReusesAuthor()
    {
        var authors = new InMemoryAuthorRepository();
        var existing = await authors.CreateAsync(
            Author.Create("Octavia Butler", null, null, null, null, null, Now),
            CancellationToken.None);
        var service = CreateService(authors: authors);

        var created = await service.CreateAsync(Command(author: " octavia   butler "), CancellationToken.None);

        Assert.Equal(existing.Id, created.AuthorId);
    }

    [Fact]
    public async Task CreateAsync_WithoutAuthor_LeavesBookUnlinked()
    {
        var service = CreateService();

        var created = await service.CreateAsync(Command(), CancellationToken.None);

        Assert.Null(created.AuthorId);
        Assert.Null(created.Author);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownAuthorId_ThrowsNotFound()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            service.CreateAsync(Command(authorId: AuthorId), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAuthorId_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.CreateAsync(Command(authorId: "invalid"), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithoutTitle_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.CreateAsync(Command(title: " "), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithUnsupportedCategory_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.CreateAsync(Command(category: "unknown"), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_ValidId_PassesTimestampToRepository()
    {
        var books = new InMemoryBookRepository();
        var service = CreateService(books);

        await service.UpdateAsync(AuthorId, 5, false, CancellationToken.None);

        Assert.Equal(Now.UtcDateTime, books.LastUpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.UpdateAsync("invalid", 5, true, CancellationToken.None));
    }

    private static BookService CreateService(
        InMemoryBookRepository? books = null,
        InMemoryAuthorRepository? authors = null)
    {
        return new BookService(
            books ?? new InMemoryBookRepository(),
            authors ?? new InMemoryAuthorRepository(),
            new FixedTimeProvider(Now));
    }

    private static CreateBookCommand Command(
        string? title = "Kindred",
        string? authorId = null,
        string? author = null,
        string? category = "Programming")
    {
        return new CreateBookCommand(
            title,
            authorId,
            author,
            category,
            1,
            null,
            "English",
            true,
            null,
            "Publisher",
            null);
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

            var persisted = author.WithId(_authors.Count == 0 ? AuthorId : $"507f1f77bcf86cd7994390{_authors.Count}");
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
            return Task.FromResult(_authors.Values.FirstOrDefault(author =>
                author.NormalizedName == normalizedName && (includeInactive || author.Active)));
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

    private sealed class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books = [];

        public int? LastCategoryId { get; private set; }
        public string? LastAuthorId { get; private set; }
        public string? LastAuthorName { get; private set; }
        public string? LastAuthorNameSearch { get; private set; }
        public string? LastFindId { get; private set; }
        public DateTime? LastUpdatedAt { get; private set; }

        public Task<IReadOnlyList<Book>> ListAsync(
            int? categoryId,
            string? authorId,
            string? authorName,
            string? authorNameSearch,
            CancellationToken cancellationToken)
        {
            LastCategoryId = categoryId;
            LastAuthorId = authorId;
            LastAuthorName = authorName;
            LastAuthorNameSearch = authorNameSearch;
            return Task.FromResult<IReadOnlyList<Book>>(_books);
        }

        public Task<Book?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult(_books.FirstOrDefault(book => book.Id == id));

        public Task<Book?> FindAsync(
            string? id,
            string? title,
            string? authorName,
            CancellationToken cancellationToken)
        {
            LastFindId = id;
            return Task.FromResult(_books.FirstOrDefault(book => id is null || book.Id == id));
        }

        public Task<Book> CreateAsync(Book book, CancellationToken cancellationToken)
        {
            var persisted = book with { Id = "507f1f77bcf86cd799439012" };
            _books.Add(persisted);
            return Task.FromResult(persisted);
        }

        public Task<Book?> UpdateAsync(
            string id,
            int quantity,
            bool active,
            DateTime updatedAt,
            CancellationToken cancellationToken)
        {
            LastUpdatedAt = updatedAt;
            var book = _books.FirstOrDefault(candidate => candidate.Id == id);
            return Task.FromResult(book is null
                ? null
                : book with { Quantity = quantity, Active = active, InactiveDate = active ? null : updatedAt });
        }
    }
}
