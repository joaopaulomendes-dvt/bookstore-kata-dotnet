namespace Bookstore.Application.Books;

public sealed record CreateBookCommand(
    string? Title,
    string? AuthorId,
    string? Author,
    string? Category,
    int Quantity,
    string? Description,
    string? Language,
    bool Active,
    DateTime? InactiveDate,
    string? Publisher,
    DateTime? PublisherDate);
