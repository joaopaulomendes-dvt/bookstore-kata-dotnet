using System.ComponentModel.DataAnnotations;
using BookstoreRefactor.Domain;

namespace BookstoreRefactor.Application;

public sealed record BookCreateRequest(
    [Required, MinLength(1)] string Title,
    [Required, MinLength(1)] string Author,
    [Required] Guid CategoryId,
    [Range(0, double.MaxValue)] decimal Price,
    BookStatus Status = BookStatus.Draft,
    Guid? AuthorId = null);

public sealed record BookUpdateRequest(
    Guid Id,
    [MinLength(1)] string? Title,
    [MinLength(1)] string? Author,
    Guid? CategoryId,
    [Range(0, double.MaxValue)] decimal? Price,
    BookStatus? Status,
    Guid? AuthorId = null);

public sealed record BookResponse(
    Guid Id,
    string Title,
    string Author,
    decimal Price,
    Guid CategoryId,
    BookStatus Status,
    Guid? AuthorId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CategoryCreateRequest(
    [Required, MinLength(1)] string Name,
    string Description,
    bool IsActive = true);

public sealed record CategoryUpdateRequest(
    Guid Id,
    [Required, MinLength(1)] string Name,
    string Description,
    bool? IsActive = null);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record BookListQuery(
    Guid? CategoryId = null,
    string? SearchTerm = null,
    BookStatus? Status = null,
    Guid? AuthorId = null);

public sealed record AuthorCreateRequest(
    [Required, MinLength(1)] string Name,
    DateTime? BirthDate = null,
    string? Nationality = null,
    string? Email = null,
    bool IsActive = true);

public sealed record AuthorResponse(
    Guid Id,
    string Name,
    DateTime? BirthDate,
    string Nationality,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public static class BookMapper
{
    public static BookResponse ToResponse(this Book book)
        => new(
            book.Id,
            book.Title,
            book.Author,
            book.Price,
            book.CategoryId,
            book.Status,
            book.AuthorId,
            book.CreatedAt,
            book.UpdatedAt);

    public static CategoryResponse ToResponse(this Category category)
        => new(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAt,
            category.UpdatedAt);

    public static AuthorResponse ToResponse(this Author author)
        => new(
            author.Id,
            author.Name,
            author.BirthDate,
            author.Nationality,
            author.Email,
            author.IsActive,
            author.CreatedAt,
            author.UpdatedAt);
}
