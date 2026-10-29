namespace Bookstore.Domain.Books;

public sealed record Book(
    string Id,
    string? Title,
    string? AuthorId,
    string? Author,
    int CategoryId,
    int Quantity,
    string? Description,
    string? Language,
    bool Active,
    DateTime? InactiveDate,
    string? Publisher,
    DateTime? PublisherDate);
