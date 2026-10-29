using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bookstore.Tests.Infrastructure;

namespace Bookstore.Tests.Integration;

[Collection(MongoDbCollection.Name)]
public sealed class AuthorEndpointsTests(MongoDbFixture fixture)
{
    [Fact]
    public async Task CreateAuthor_ValidProfile_ReturnsCreatedAndRetrievable()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/authors", new
        {
            name = "Octavia Butler",
            biography = "American science fiction author",
            website = "https://example.test/octavia",
            birthDate = "1947-06-22",
            nationality = "American",
            email = "octavia@example.test"
        });

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        using var created = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(id));
        Assert.True(created.RootElement.GetProperty("active").GetBoolean());
        Assert.Equal("Octavia Butler", created.RootElement.GetProperty("name").GetString());

        var retrieved = await client.GetFromJsonAsync<JsonElement>($"/api/authors/{id}");
        Assert.Equal(id, retrieved.GetProperty("id").GetString());
        Assert.Equal("American", retrieved.GetProperty("nationality").GetString());
    }

    [Fact]
    public async Task CreateAuthor_InvalidName_ReturnsProblemDetails()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/authors", new { name = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var content = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, content.RootElement.GetProperty("status").GetInt32());
        var traceId = content.RootElement.GetProperty("traceId").GetString();
        Assert.Equal(traceId, content.RootElement.GetProperty("instance").GetString());
        Assert.Equal(traceId, response.Headers.GetValues("X-Trace-Id").Single());
    }

    [Fact]
    public async Task CreateAuthor_NormalizedDuplicate_ReturnsConflict()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/authors", new { name = "Octavia Butler" });

        var response = await client.PostAsJsonAsync("/api/authors", new { name = " octavia   butler " });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DeleteAuthor_InactiveByDefaultButAvailableWhenIncluded()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();
        var createdResponse = await client.PostAsJsonAsync("/api/authors", new { name = "Octavia Butler" });
        using var created = JsonDocument.Parse(await createdResponse.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("id").GetString();
        var bookResponse = await client.PostAsJsonAsync("/api/books", new
        {
            title = "Kindred",
            authorId = id,
            categoryId = "Fiction",
            quantity = 1,
            active = true
        });
        using var createdBook = JsonDocument.Parse(await bookResponse.Content.ReadAsStringAsync());
        var bookId = createdBook.RootElement.GetProperty("id").GetString();

        var deleteResponse = await client.DeleteAsync($"/api/authors/{id}");
        var defaultGet = await client.GetAsync($"/api/authors/{id}");
        var includedGet = await client.GetAsync($"/api/authors/{id}?includeInactive=true");
        var defaultList = await client.GetFromJsonAsync<JsonElement[]>("/api/authors");
        var includedList = await client.GetFromJsonAsync<JsonElement[]>("/api/authors?includeInactive=true");
        var bookAfterDelete = await client.GetFromJsonAsync<JsonElement>($"/api/books/{bookId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, defaultGet.StatusCode);
        Assert.Equal(HttpStatusCode.OK, includedGet.StatusCode);
        Assert.DoesNotContain(defaultList!, author => author.GetProperty("id").GetString() == id);
        Assert.Contains(includedList!, author => author.GetProperty("id").GetString() == id);
        Assert.Equal(id, bookAfterDelete.GetProperty("authorId").GetString());
        Assert.True(bookAfterDelete.GetProperty("active").GetBoolean());
        using var inactive = JsonDocument.Parse(await includedGet.Content.ReadAsStringAsync());
        Assert.False(inactive.RootElement.GetProperty("active").GetBoolean());
    }
}
