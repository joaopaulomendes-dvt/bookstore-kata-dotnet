namespace Bookstore.Infrastructure.Migrations;

public sealed record AuthorBackfillReport(
    int BooksLinked,
    int AuthorsCreated,
    int BooksAlreadyLinked,
    int BooksSkipped);
