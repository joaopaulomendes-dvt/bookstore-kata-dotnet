using BookstoreRefactor.Domain;
using MongoDB.Driver;

namespace BookstoreRefactor.Infrastructure;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<CategoryDocument> _categories;

    public CategoryRepository(MongoDbContext context)
    {
        _categories = context.Categories;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _categories.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : document.ToDomain();
    }

    public async Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _categories.Find(_ => true).ToListAsync(cancellationToken);
        return documents.Select(x => x.ToDomain()).ToList();
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _categories.InsertOneAsync(category.ToDocument(), new InsertOneOptions(), cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _categories.ReplaceOneAsync(x => x.Id == category.Id, category.ToDocument(), cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _categories.CountDocumentsAsync(x => x.Id == id, cancellationToken: cancellationToken) > 0;
    }
}
