using Testcontainers.MongoDb;

namespace Bookstore.Tests.Infrastructure;

[CollectionDefinition("MongoDB")]
public sealed class MongoDbCollection : ICollectionFixture<MongoDbFixture>
{
    public const string Name = "MongoDB";
}

public sealed class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:7.0").Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public BookstoreApiFactory CreateFactory()
    {
        return new BookstoreApiFactory(
            _container.GetConnectionString(),
            $"bookstore_tests_{Guid.NewGuid():N}");
    }
}
