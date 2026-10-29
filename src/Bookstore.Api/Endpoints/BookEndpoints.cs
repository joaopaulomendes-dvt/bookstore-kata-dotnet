using Bookstore.Application.Books;
using Bookstore.Application.Common;
using Bookstore.Domain.Books;
using Bookstore.Domain.Common;
using Bookstore.Infrastructure.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Bookstore.Api.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/health", HealthAsync)
            .WithName("BookApiHealth")
            .WithSummary("Check API and MongoDB availability.");

        group.MapGet("/", ListAsync)
            .WithName("ListBooks")
            .WithSummary("List books with optional category and author filters.")
            .Produces<IReadOnlyList<BookResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/single", FindLegacyAsync)
            .WithName("GetBookLegacy")
            .WithSummary("Legacy lookup by ID, title, or author name.")
            .WithDescription("Use GET /api/books/{id} for ID lookups in new clients.")
            .Produces<BookResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id}", GetByIdAsync)
            .WithName("GetBookById")
            .WithSummary("Get a book by ID.")
            .Produces<BookResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateBook")
            .WithSummary("Create a book.")
            .Produces<BookResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/", UpdateAsync)
            .WithName("UpdateBook")
            .WithSummary("Update book quantity and active state.")
            .Produces<BookResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> HealthAsync(
        IMongoDatabase database,
        CancellationToken cancellationToken)
    {
        await database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1),
            cancellationToken: cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> ListAsync(
        string? category,
        string? authorId,
        string? author,
        BookService service,
        CancellationToken cancellationToken)
    {
        var books = await service.ListAsync(category, authorId, author, cancellationToken);
        return Results.Ok(books.Select(ToResponse).ToArray());
    }

    private static async Task<IResult> GetByIdAsync(
        string id,
        BookService service,
        CancellationToken cancellationToken)
    {
        var book = await service.GetByIdAsync(id, cancellationToken);
        return book is null
            ? throw new ResourceNotFoundException("Book", id)
            : Results.Ok(ToResponse(book));
    }

    private static async Task<IResult> FindLegacyAsync(
        string? id,
        string? name,
        string? author,
        BookService service,
        CancellationToken cancellationToken)
    {
        var book = await service.FindAsync(id, name, author, cancellationToken);
        var identifier = id ?? name ?? author ?? "query";
        return book is null
            ? throw new ResourceNotFoundException("Book", identifier)
            : Results.Ok(ToResponse(book));
    }

    private static async Task<IResult> CreateAsync(
        CreateBookRequest request,
        BookService service,
        CancellationToken cancellationToken)
    {
        var book = await service.CreateAsync(
            new CreateBookCommand(
                request.Title,
                request.AuthorId,
                request.Author,
                request.CategoryId,
                request.Quantity,
                request.Description,
                request.Language,
                request.Active,
                request.InactiveDate,
                request.Publisher,
                request.PublisherDate),
            cancellationToken);

        return Results.Created($"/api/books/{book.Id}", ToResponse(book));
    }

    private static async Task<IResult> UpdateAsync(
        UpdateBookRequest request,
        BookService service,
        CancellationToken cancellationToken)
    {
        var id = request.Id ??
                 throw new DomainValidationException("Book ID is required.");
        var book = await service.UpdateAsync(
            id,
            request.Quantity,
            request.Active,
            cancellationToken);

        return book is null
            ? throw new ResourceNotFoundException("Book", id)
            : Results.Created($"/api/books/{book.Id}", ToResponse(book));
    }

    private static BookResponse ToResponse(Book book)
    {
        return new BookResponse(
            book.Id,
            book.Title,
            book.AuthorId,
            book.Author,
            book.CategoryId,
            book.Quantity,
            book.Description,
            book.Language,
            book.Active,
            book.InactiveDate,
            book.Publisher,
            book.PublisherDate);
    }
}

public sealed record CreateBookRequest(
    string? Title = null,
    string? AuthorId = null,
    string? Author = null,
    string? CategoryId = null,
    int Quantity = 0,
    string? Description = null,
    string? Language = null,
    bool Active = true,
    DateTime? InactiveDate = null,
    string? Publisher = null,
    DateTime? PublisherDate = null);

public sealed record UpdateBookRequest(string? Id, int Quantity, bool Active);

public sealed record BookResponse(
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
