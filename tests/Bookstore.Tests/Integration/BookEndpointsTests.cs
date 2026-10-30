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
    public async Task ListBooks_AuthorNameSearch_MatchesCaseInsensitiveSubstring()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Kindred",
            author = "Octavia Butler",
            categoryId = "Fiction",
            quantity = 1,
            active = true
        });
        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Small Gods",
            author = "Terry Pratchett",
            categoryId = "Fiction",
            quantity = 1,
            active = true
        });

        var response = await client.GetAsync("/api/books?authorName=butler");
        var books = await response.Content.ReadFromJsonAsync<JsonElement[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(books);
        Assert.Single(books);
        Assert.Equal("Kindred", books[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task ListBooks_AuthorNameSearch_TreatsRegexCharactersAsLiteralText()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/books", new
        {
            title = "C++ Primer",
            author = "Stanley Lippman C++",
            categoryId = "Programming",
            quantity = 1,
            active = true
        });

        var response = await client.GetAsync($"/api/books?authorName={Uri.EscapeDataString("C++")}");
        var books = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        var regexMetacharacters = await client.GetFromJsonAsync<JsonElement[]>(
            $"/api/books?authorName={Uri.EscapeDataString(".*")}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(books);
        Assert.Single(books);
        Assert.Equal("C++ Primer", books[0].GetProperty("title").GetString());
        Assert.NotNull(regexMetacharacters);
        Assert.Empty(regexMetacharacters);
    }

    [Fact]
    public async Task ListBooks_AuthorNameSearch_NoMatchReturnsEmptyCollection()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();

        var books = await client.GetFromJsonAsync<JsonElement[]>(
            "/api/books?authorName=no-such-author");

        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task ListBooks_AuthorNameSearch_TermsLongerThan255CharactersReturnBadRequest()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/books?authorName={new string('a', 256)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ListBooks_AuthorNameSearch_ComposesWithCategoryAndAuthorIdFilters()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();
        var authorResponse = await client.PostAsJsonAsync("/api/authors", new { name = "Octavia Butler" });
        using var author = JsonDocument.Parse(await authorResponse.Content.ReadAsStringAsync());
        var authorId = author.RootElement.GetProperty("id").GetString();

        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Parable of the Sower",
            authorId,
            categoryId = "Fiction",
            quantity = 1,
            active = true
        });
        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Programming with Butler",
            authorId,
            categoryId = "Programming",
            quantity = 1,
            active = true
        });

        var books = await client.GetFromJsonAsync<JsonElement[]>(
            $"/api/books?authorId={authorId}&authorName=butler&category=Fiction");

        Assert.NotNull(books);
        Assert.Single(books);
        Assert.Equal("Parable of the Sower", books[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task ListBooks_AuthorNameSearch_IncludesBooksLinkedToInactiveAuthors()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();
        var authorResponse = await client.PostAsJsonAsync("/api/authors", new { name = "Octavia Butler" });
        using var author = JsonDocument.Parse(await authorResponse.Content.ReadAsStringAsync());
        var authorId = author.RootElement.GetProperty("id").GetString();
        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Kindred",
            authorId,
            categoryId = "Fiction",
            quantity = 1,
            active = true
        });

        var deleteResponse = await client.DeleteAsync($"/api/authors/{authorId}");
        var books = await client.GetFromJsonAsync<JsonElement[]>(
            "/api/books?authorName=butler");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.NotNull(books);
        Assert.Single(books);
        Assert.Equal("Kindred", books[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task ListBooks_LegacyAuthorFilter_RemainsExactWhileAuthorNameSearchIsPartial()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Kindred",
            author = "Octavia Butler",
            categoryId = "Fiction",
            quantity = 1,
            active = true
        });
        await client.PostAsJsonAsync("/api/books", new
        {
            title = "Parable of the Sower",
            author = "Octavia L. Butler",
            categoryId = "Fiction",
            quantity = 1,
            active = true
        });

        var exact = await client.GetFromJsonAsync<JsonElement[]>(
            "/api/books?author=Octavia%20Butler");
        var partial = await client.GetFromJsonAsync<JsonElement[]>(
            "/api/books?authorName=butler");

        Assert.NotNull(exact);
        Assert.Single(exact);
        Assert.NotNull(partial);
        Assert.Equal(2, partial.Length);
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
    public async Task OpenApiDocument_ApiStarts_ListsRoutesAndErrorResponses()
    {
        using var factory = fixture.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.True(paths.TryGetProperty("/api/books", out var books));
        Assert.True(paths.TryGetProperty("/api/authors", out var authors));
        Assert.Contains(
            books.GetProperty("get").GetProperty("parameters").EnumerateArray(),
            parameter => parameter.GetProperty("name").GetString() == "authorName");
        Assert.Contains(
            "case-insensitive literal substring",
            books.GetProperty("get").GetProperty("description").GetString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            "#/components/schemas/CreateBookRequest",
            books.GetProperty("post").GetProperty("requestBody")
                .GetProperty("content").GetProperty("application/json")
                .GetProperty("schema").GetProperty("$ref").GetString());
        Assert.Equal(
            "#/components/schemas/CreateAuthorRequest",
            authors.GetProperty("post").GetProperty("requestBody")
                .GetProperty("content").GetProperty("application/json")
                .GetProperty("schema").GetProperty("$ref").GetString());
        Assert.Contains(
            "title",
            schemas.GetProperty("CreateBookRequest").GetProperty("required")
                .EnumerateArray().Select(property => property.GetString()));
        Assert.Contains(
            "name",
            schemas.GetProperty("CreateAuthorRequest").GetProperty("required")
                .EnumerateArray().Select(property => property.GetString()));
        Assert.DoesNotContain(
            "biography",
            schemas.GetProperty("CreateAuthorRequest").GetProperty("required")
                .EnumerateArray().Select(property => property.GetString()));
        Assert.Equal(
            "Use GET /api/books/{id} for ID lookups in new clients.",
            paths.GetProperty("/api/books/single").GetProperty("get")
                .GetProperty("description").GetString());
        Assert.Equal(
            255,
            schemas.GetProperty("CreateAuthorRequest").GetProperty("properties")
                .GetProperty("name").GetProperty("maxLength").GetInt32());
        Assert.Contains(
            "traceId",
            schemas.GetProperty("ApiProblemDetails").GetProperty("required")
                .EnumerateArray().Select(property => property.GetString()));

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject()
                         .Where(property => property.Name is "get" or "post" or "put" or "delete"))
            {
                Assert.True(
                    operation.Value.GetProperty("responses").TryGetProperty("500", out _),
                    $"{operation.Name.ToUpperInvariant()} {path.Name} must document HTTP 500.");
                var serverError = operation.Value.GetProperty("responses").GetProperty("500");
                Assert.Equal(
                    "#/components/schemas/ApiProblemDetails",
                    serverError.GetProperty("content").GetProperty("application/problem+json")
                        .GetProperty("schema").GetProperty("$ref").GetString());
            }
        }
    }
}
