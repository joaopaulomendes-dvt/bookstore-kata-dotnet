using System.ComponentModel.DataAnnotations;
using Bookstore.Api.Infrastructure;
using Bookstore.Application.Authors;
using Bookstore.Application.Common;
using Bookstore.Domain.Authors;

namespace Bookstore.Api.Endpoints;

public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateAsync)
            .WithName("CreateAuthor")
            .WithSummary("Create an Author profile.")
            .Produces<AuthorResponse>(StatusCodes.Status201Created)
            .ProducesApiProblem(StatusCodes.Status400BadRequest)
            .ProducesApiProblem(StatusCodes.Status409Conflict)
            .ProducesApiProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/", ListAsync)
            .WithName("ListAuthors")
            .WithSummary("List Authors, excluding inactive profiles by default.")
            .Produces<IReadOnlyList<AuthorResponse>>()
            .ProducesApiProblem(StatusCodes.Status400BadRequest)
            .ProducesApiProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id}", GetAsync)
            .WithName("GetAuthor")
            .WithSummary("Get an Author by ID.")
            .Produces<AuthorResponse>()
            .ProducesApiProblem(StatusCodes.Status400BadRequest)
            .ProducesApiProblem(StatusCodes.Status404NotFound)
            .ProducesApiProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id}", DeleteAsync)
            .WithName("SoftDeleteAuthor")
            .WithSummary("Soft-delete an Author without modifying linked Books.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesApiProblem(StatusCodes.Status400BadRequest)
            .ProducesApiProblem(StatusCodes.Status404NotFound)
            .ProducesApiProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> CreateAsync(
        CreateAuthorRequest request,
        AuthorService service,
        CancellationToken cancellationToken)
    {
        var author = await service.CreateAsync(
            request.Name,
            request.Biography,
            request.Website,
            request.BirthDate,
            request.Nationality,
            request.Email,
            cancellationToken);

        return Results.Created($"/api/authors/{author.Id}", ToResponse(author));
    }

    private static async Task<IResult> ListAsync(
        AuthorService service,
        CancellationToken cancellationToken,
        bool includeInactive = false)
    {
        var authors = await service.ListAsync(includeInactive, cancellationToken);
        return Results.Ok(authors.Select(ToResponse).ToArray());
    }

    private static async Task<IResult> GetAsync(
        string id,
        AuthorService service,
        CancellationToken cancellationToken,
        bool includeInactive = false)
    {
        var author = await service.GetByIdAsync(id, includeInactive, cancellationToken);
        return author is null
            ? throw new ResourceNotFoundException("Author", id)
            : Results.Ok(ToResponse(author));
    }

    private static async Task<IResult> DeleteAsync(
        string id,
        AuthorService service,
        CancellationToken cancellationToken)
    {
        if (!await service.DeactivateAsync(id, cancellationToken))
        {
            throw new ResourceNotFoundException("Author", id);
        }

        return Results.NoContent();
    }

    private static AuthorResponse ToResponse(Author author)
    {
        return new AuthorResponse(
            author.Id,
            author.Name,
            author.Biography,
            author.Website,
            author.BirthDate,
            author.Nationality,
            author.Email,
            author.Active,
            author.CreatedAt,
            author.UpdatedAt);
    }
}

public sealed record CreateAuthorRequest(
    [property: Required, StringLength(255, MinimumLength = 1)] string Name,
    string? Biography = null,
    [property: Url] string? Website = null,
    DateOnly? BirthDate = null,
    string? Nationality = null,
    [property: EmailAddress] string? Email = null);

public sealed record AuthorResponse(
    string Id,
    string Name,
    string? Biography,
    string? Website,
    DateOnly? BirthDate,
    string? Nationality,
    string? Email,
    bool Active,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
