using BookstoreRefactor.Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookstoreRefactor.Infrastructure;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoOptions _options;

    public MongoDbContext(IOptions<MongoOptions> options)
    {
        _options = options.Value;
        var client = new MongoClient(_options.ConnectionString);
        _database = client.GetDatabase(_options.DatabaseName);
    }

    public IMongoCollection<BookDocument> Books => _database.GetCollection<BookDocument>(_options.BooksCollection);
    public IMongoCollection<CategoryDocument> Categories => _database.GetCollection<CategoryDocument>(_options.CategoriesCollection);
}
