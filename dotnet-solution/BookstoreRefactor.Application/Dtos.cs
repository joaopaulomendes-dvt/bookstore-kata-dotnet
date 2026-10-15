using System.ComponentModel.DataAnnotations;
using BookstoreRefactor.Domain;

namespace BookstoreRefactor.Application;

public sealed record BookCreateRequest(
    [Required, MinLength(1)] string Title,
    [Required, MinLength(1)] string Author,
    [Required] Guid CategoryId,
    [Range(0, double.MaxValue)] decimal Price,
    BookStatus Status = BookStatus.Draft);

public sealed record BookUpdateRequest(
    Guid Id,
    [MinLength(1)] string? Title,
    [MinLength(1)] string? Author,
    Guid? CategoryId,
    [Range(0, double.MaxValue)] decimal? Price,
    BookStatus? Status);

public sealed record BookResponse(
    Guid Id,
    string Title,
    string Author,
    decimal Price,
    Guid CategoryId,
    BookStatus Status,
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
    BookStatus? Status = null);

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
}
