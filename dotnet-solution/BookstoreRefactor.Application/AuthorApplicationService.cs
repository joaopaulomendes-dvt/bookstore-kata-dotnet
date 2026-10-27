using BookstoreRefactor.Domain;

namespace BookstoreRefactor.Application;

public sealed class AuthorApplicationService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorApplicationService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<AuthorResponse> CreateAsync(AuthorCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Author request is required.");
        }

        try
        {
            var author = Author.Create(request.Name, request.BirthDate, request.Nationality, request.Email, request.IsActive);
            await _authorRepository.AddAsync(author, cancellationToken);
            return author.ToResponse();
        }
        catch (DomainException ex)
        {
            throw new ValidationException(ex.Message);
        }
    }

    public async Task<AuthorResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await _authorRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Author '{id}' was not found.");

        return author.ToResponse();
    }

    public async Task<IReadOnlyCollection<AuthorResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var authors = await _authorRepository.GetAllAsync(cancellationToken);
        return authors.Select(x => x.ToResponse()).ToList();
    }

    public async Task<AuthorResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await _authorRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Author '{id}' was not found.");

        author.Deactivate();
        await _authorRepository.UpdateAsync(author, cancellationToken);
        return author.ToResponse();
    }
}
