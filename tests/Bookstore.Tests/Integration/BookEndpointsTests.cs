using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bookstore.Tests.Infrastructure;

namespace Bookstore.Tests.Integration;

[Collection(MongoDbCollection.Name)]
public sealed class BookEndpointsTests(MongoDbFixture fixture)
{
    [Fact]
    public async Task BookLifecycle_ExistingRoutesRemainAvailable()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/books/health")).StatusCode);

        var createResponse = await client.PostAsJsonAsync("/api/books", new
        {
            title = "Test Driven Development",
            author = "Kent Beck",
            categoryId = "Programming",
            quantity = 3,
            description = "A book about TDD",
            language = "English",
            active = true,
            publisher = "Addison-Wesley"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("id").GetString();
        Assert.Equal("Kent Beck", created.RootElement.GetProperty("author").GetString());
        Assert.False(string.IsNullOrWhiteSpace(created.RootElement.GetProperty("authorId").GetString()));

        var listed = await client.GetFromJsonAsync<JsonElement[]>("/api/books");
        Assert.NotNull(listed);
        Assert.Contains(listed, book => book.GetProperty("id").GetString() == id);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/books/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync("/api/books/single?name=Test%20Driven%20Development")).StatusCode);

        var updateResponse = await client.PutAsJsonAsync("/api/books", new
        {
            id,
            quantity = 7,
            active = true
        });
        Assert.Equal(HttpStatusCode.Created, updateResponse.StatusCode);
    }

    [Fact]
    public async Task ListBooks_AuthorIdFilter_ReturnsOnlyLinkedBooks()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();
        var authorResponse = await client.PostAsJsonAsync("/api/authors", new { name = "Terry Pratchett" });
        using var author = JsonDocument.Parse(await authorResponse.Content.ReadAsStringAsync());
        var authorId = author.RootElement.GetProperty("id").GetString();

        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Guards! Guards!",
            authorId,
            categoryId = "Fiction",
            quantity = 2,
            active = true
        });
        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Unrelated Book",
            categoryId = "Fiction",
            quantity = 1,
            active = true
        });

        var books = await client.GetFromJsonAsync<JsonElement[]>($"/api/books?authorId={authorId}");

        Assert.NotNull(books);
        Assert.Single(books);
        Assert.Equal("Guards! Guards!", books[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task ListBooks_MalformedAuthorId_ReturnsBadRequestProblemDetails()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/books?authorId=invalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task OpenApiDocument_IsAvailable()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");
        var document = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/books", document, StringComparison.Ordinal);
        Assert.Contains("/api/authors", document, StringComparison.Ordinal);
    }
}
