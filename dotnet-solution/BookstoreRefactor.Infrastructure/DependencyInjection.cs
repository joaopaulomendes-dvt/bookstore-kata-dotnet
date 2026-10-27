using BookstoreRefactor.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace BookstoreRefactor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        services.Configure<GitHubMcpOptions>(configuration.GetSection(GitHubMcpOptions.SectionName));
        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddHttpClient<IGitHubIssueClient, GitHubIssueClient>();

        return services;
    }

    public static async Task InitializeMongoAsync(this IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        var context = provider.GetRequiredService<MongoDbContext>();

        await context.Books.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<BookDocument>(Builders<BookDocument>.IndexKeys.Ascending(x => x.CategoryId)),
            new CreateIndexModel<BookDocument>(Builders<BookDocument>.IndexKeys.Ascending(x => x.Status))
        ], cancellationToken);

        await context.Categories.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<CategoryDocument>(Builders<CategoryDocument>.IndexKeys.Ascending(x => x.IsActive)),
            new CreateIndexModel<CategoryDocument>(Builders<CategoryDocument>.IndexKeys.Ascending(x => x.Name))
        ], cancellationToken);

        await context.Authors.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<AuthorDocument>(Builders<AuthorDocument>.IndexKeys.Ascending(x => x.IsActive)),
            new CreateIndexModel<AuthorDocument>(Builders<AuthorDocument>.IndexKeys.Ascending(x => x.Name))
        ], cancellationToken);
    }
}
