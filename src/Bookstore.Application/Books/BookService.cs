using Bookstore.Application.Authors;
using Bookstore.Application.Common;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Domain.Common;

namespace Bookstore.Application.Books;

public sealed class BookService(
    IBookRepository bookRepository,
    IAuthorRepository authorRepository,
    TimeProvider timeProvider)
{
    public Task<IReadOnlyList<Book>> ListAsync(
        string? category,
        string? authorId,
        string? exactAuthorName,
        string? authorNameSearch,
        CancellationToken cancellationToken)
    {
        var categoryId = Categories.FindId(category);
        if (!string.IsNullOrWhiteSpace(category) && categoryId is null)
        {
            throw new DomainValidationException("Category is not supported.");
        }

        if (authorId is not null && !ObjectIdText.IsValid(authorId))
        {
            throw new DomainValidationException("Author ID must be a 24-character hexadecimal ObjectId.");
        }

        authorNameSearch = authorNameSearch?.Trim();
        if (authorNameSearch?.Length > 255)
        {
            throw new DomainValidationException("Author name search must not exceed 255 characters.");
        }

        return bookRepository.ListAsync(
            categoryId,
            authorId,
            exactAuthorName,
            string.IsNullOrEmpty(authorNameSearch) ? null : authorNameSearch,
            cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectIdText.IsValid(id))
        {
            throw new DomainValidationException("Book ID must be a 24-character hexadecimal ObjectId.");
        }

        return await bookRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<Book?> FindAsync(
        string? id,
        string? title,
        string? authorName,
        CancellationToken cancellationToken)
    {
        if (id is not null && !ObjectIdText.IsValid(id))
        {
            throw new DomainValidationException("Book ID must be a 24-character hexadecimal ObjectId.");
        }

        return bookRepository.FindAsync(id, title, authorName, cancellationToken);
    }

    public async Task<Book> CreateAsync(CreateBookCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new DomainValidationException("Title is required.");
        }

        var categoryId = Categories.FindId(command.Category);
        if (!string.IsNullOrWhiteSpace(command.Category) && categoryId is null)
        {
            throw new DomainValidationException("Category is not supported.");
        }

        var author = await ResolveAuthorAsync(command.AuthorId, command.Author, cancellationToken);
        var book = new Book(
            string.Empty,
            command.Title,
            author?.Id,
            author?.Name,
            categoryId ?? 0,
            command.Quantity,
            command.Description,
            command.Language,
            command.Active,
            command.InactiveDate,
            command.Publisher,
            command.PublisherDate);

        return await bookRepository.CreateAsync(book, cancellationToken);
    }

    public async Task<Book?> UpdateAsync(
        string? id,
        int quantity,
        bool active,
        CancellationToken cancellationToken)
    {
        if (!ObjectIdText.IsValid(id))
        {
            throw new DomainValidationException("Book ID must be a 24-character hexadecimal ObjectId.");
        }

        return await bookRepository.UpdateAsync(
            id!,
            quantity,
            active,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);
    }

    private async Task<Author?> ResolveAuthorAsync(
        string? authorId,
        string? legacyAuthorName,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(authorId))
        {
            if (!ObjectIdText.IsValid(authorId))
            {
                throw new DomainValidationException("Author ID must be a 24-character hexadecimal ObjectId.");
            }

            var linkedAuthor = await authorRepository.GetByIdAsync(authorId, false, cancellationToken);
            return linkedAuthor ?? throw new ResourceNotFoundException("Active Author", authorId);
        }

        if (string.IsNullOrWhiteSpace(legacyAuthorName))
        {
            return null;
        }

        var normalizedName = Author.NormalizeName(legacyAuthorName);
        var existingAuthor = await authorRepository.GetByNormalizedNameAsync(
            normalizedName,
            false,
            cancellationToken);
        if (existingAuthor is not null)
        {
            return existingAuthor;
        }

        var newAuthor = Author.Create(legacyAuthorName, null, null, null, null, null, timeProvider.GetUtcNow());
        try
        {
            return await authorRepository.CreateAsync(newAuthor, cancellationToken);
        }
        catch (AuthorAlreadyExistsException)
        {
            return await authorRepository.GetByNormalizedNameAsync(
                       normalizedName,
                       false,
                       cancellationToken) ??
                   throw new AuthorAlreadyExistsException(normalizedName);
        }
    }
}
