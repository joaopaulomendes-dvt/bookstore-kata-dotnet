using Bookstore.Application.Authors;
using Bookstore.Application.Books;
using Bookstore.Infrastructure.Authors;
using Bookstore.Infrastructure.Books;
using Bookstore.Infrastructure.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Bookstore.Infrastructure.Configuration;

public static class BookstoreInfrastructureRegistration
{
    public static IServiceCollection AddBookstoreInfrastructure(
        this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString =
                configuration["MongoDb:ConnectionString"] ??
                configuration["MONGODB_URI"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "MongoDB connection is not configured. Set MongoDb:ConnectionString or MONGODB_URI.");
            }

            return new MongoClient(connectionString);
        });
        services.AddSingleton(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseName = configuration["MongoDb:DatabaseName"] ?? "bookstore";
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("MongoDb:DatabaseName must not be empty.");
            }

            return serviceProvider.GetRequiredService<IMongoClient>().GetDatabase(databaseName);
        });
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IAuthorRepository, MongoAuthorRepository>();
        services.AddSingleton<IBookRepository, MongoBookRepository>();
        services.AddSingleton<MongoDatabaseInitializer>();
        services.AddSingleton<AuthorBackfill>();

        return services;
    }
}
