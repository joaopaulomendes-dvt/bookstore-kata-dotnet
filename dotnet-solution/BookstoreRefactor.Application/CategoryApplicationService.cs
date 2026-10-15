using BookstoreRefactor.Domain;

namespace BookstoreRefactor.Application;

public sealed class CategoryApplicationService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryApplicationService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponse> CreateAsync(CategoryCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Category request is required.");
        }

        try
        {
            var category = Category.Create(request.Name, request.Description, request.IsActive);
            await _categoryRepository.AddAsync(category, cancellationToken);
            return category.ToResponse();
        }
        catch (DomainException ex)
        {
            throw new ValidationException(ex.Message);
        }
    }

    public async Task<CategoryResponse> UpdateAsync(CategoryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Category update request is required.");
        }

        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Category '{request.Id}' was not found.");

        try
        {
            category.Update(request.Name, request.Description);
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                {
                    category.Activate();
                }
                else
                {
                    category.Deactivate();
                }
            }

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            return category.ToResponse();
        }
        catch (DomainException ex)
        {
            throw new ValidationException(ex.Message);
        }
    }

    public async Task<CategoryResponse> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category '{id}' was not found.");

        if (category.IsActive)
        {
            category.Deactivate();
        }
        else
        {
            category.Activate();
        }

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        return category.ToResponse();
    }

    public async Task<IReadOnlyCollection<CategoryResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(x => x.ToResponse()).ToList();
    }
}
