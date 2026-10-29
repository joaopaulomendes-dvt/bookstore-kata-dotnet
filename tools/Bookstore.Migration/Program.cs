using Bookstore.Infrastructure.Configuration;
using Bookstore.Infrastructure.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddBookstoreInfrastructure();

await using var serviceProvider = services.BuildServiceProvider();
await serviceProvider
    .GetRequiredService<MongoDatabaseInitializer>()
    .InitializeAsync(CancellationToken.None);

var report = await serviceProvider
    .GetRequiredService<AuthorBackfill>()
    .RunAsync(CancellationToken.None);

Console.WriteLine(
    $"Author backfill complete. Books linked: {report.BooksLinked}; " +
    $"Authors created: {report.AuthorsCreated}; " +
    $"Books already linked: {report.BooksAlreadyLinked}; " +
    $"Books skipped: {report.BooksSkipped}.");
