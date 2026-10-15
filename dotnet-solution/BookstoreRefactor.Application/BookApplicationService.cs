using BookstoreRefactor.Domain;

namespace BookstoreRefactor.Application;

public sealed class BookApplicationService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BookApplicationService(IBookRepository bookRepository, ICategoryRepository categoryRepository)
    {
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<BookResponse> CreateAsync(BookCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Book request is required.");
        }

        if (!await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
        {
            throw new NotFoundException($"Category '{request.CategoryId}' was not found.");
        }

        try
        {
            var book = Book.Create(request.Title, request.Author, request.CategoryId, request.Price, request.Status);
            await _bookRepository.AddAsync(book, cancellationToken);
            return book.ToResponse();
        }
        catch (DomainException ex)
        {
            throw new ValidationException(ex.Message);
        }
    }

    public async Task<BookResponse> UpdateAsync(BookUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Book update request is required.");
        }

        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Book '{request.Id}' was not found.");

        try
        {
            if (request.CategoryId.HasValue)
            {
                if (!await _categoryRepository.ExistsAsync(request.CategoryId.Value, cancellationToken))
                {
                    throw new NotFoundException($"Category '{request.CategoryId.Value}' was not found.");
                }

                book.UpdateCategory(request.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                book.UpdateTitle(request.Title);
            }

            if (!string.IsNullOrWhiteSpace(request.Author))
            {
                book.UpdateAuthor(request.Author);
            }

            if (request.Price.HasValue)
            {
                book.UpdatePrice(request.Price.Value);
            }

            if (request.Status.HasValue)
            {
                book.ChangeStatus(request.Status.Value);
            }

            await _bookRepository.UpdateAsync(book, cancellationToken);
            return book.ToResponse();
        }
        catch (DomainException ex)
        {
            throw new ValidationException(ex.Message);
        }
    }

    public async Task<BookResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Book '{id}' was not found.");

        return book.ToResponse();
    }

    public async Task<IReadOnlyCollection<BookResponse>> ListAsync(BookListQuery? query = null, CancellationToken cancellationToken = default)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);
        var search = query?.SearchTerm ?? string.Empty;
        var categoryId = query?.CategoryId;
        var status = query?.Status;

        var filtered = books
            .Where(book => !categoryId.HasValue || book.CategoryId == categoryId.Value)
            .Where(book => !status.HasValue || book.Status == status.Value)
            .Where(book => string.IsNullOrWhiteSpace(search)
                || book.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                || book.Author.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.ToResponse())
            .ToList();

        return filtered;
    }
}
