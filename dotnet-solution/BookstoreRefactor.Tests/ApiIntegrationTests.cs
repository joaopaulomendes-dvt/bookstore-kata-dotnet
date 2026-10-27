using System.Net;
using System.Net.Http.Json;
using BookstoreRefactor.Application;
using BookstoreRefactor.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BookstoreRefactor.Tests;

public sealed class ApiIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;

    public ApiIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBooks_WhenCalled_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<List<BookResponse>>();
        Assert.NotNull(books);
    }

    [Fact]
    public async Task PostBook_WhenValidRequest_ReturnsCreatedAndLocationHeader()
    {
        var request = new BookCreateRequest("Clean Architecture", "Robert Martin", _factory.DefaultCategoryId, 59.90m, BookStatus.Draft);

        var response = await _client.PostAsJsonAsync("/api/books", request);
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"POST valid body: {body}");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var result = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(result);
        Assert.Equal("Clean Architecture", result!.Title);
    }

    [Fact]
    public async Task GetBook_WhenMissing_ReturnsNotFoundProblemDetails()
    {
        var response = await _client.GetAsync($"/api/books/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem!.Status);
    }

    [Fact]
    public async Task PostBook_WhenCategoryIsInvalid_ReturnsBadRequest()
    {
        var request = new BookCreateRequest(string.Empty, "Robert Martin", Guid.NewGuid(), 59.90m, BookStatus.Draft);

        var response = await _client.PostAsJsonAsync("/api/books", request);
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"POST invalid body: {body}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public Guid DefaultCategoryId { get; private set; } = Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBookRepository>();
            services.RemoveAll<ICategoryRepository>();

            var category = Category.Create("Technology", "Technology books");
            var categories = new List<Category> { category };

            DefaultCategoryId = category.Id;

            services.AddSingleton<IBookRepository>(new InMemoryBookRepository());
            services.AddSingleton<ICategoryRepository>(new InMemoryCategoryRepository(categories));
        });
    }
}

public sealed class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> _books = [];

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_books.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyCollection<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Book>>(_books.ToList());

    public Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        _books.Add(book);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var index = _books.FindIndex(x => x.Id == book.Id);
        if (index >= 0)
        {
            _books[index] = book;
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_books.Any(x => x.Id == id));
}

public sealed class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories;

    public InMemoryCategoryRepository(IEnumerable<Category>? categories = null)
    {
        _categories = categories?.ToList() ?? [];
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_categories.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Category>>(_categories.ToList());

    public Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        _categories.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        var index = _categories.FindIndex(x => x.Id == category.Id);
        if (index >= 0)
        {
            _categories[index] = category;
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_categories.Any(x => x.Id == id));
}
