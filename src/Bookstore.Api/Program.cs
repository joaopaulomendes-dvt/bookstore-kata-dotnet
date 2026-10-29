using Bookstore.Api.Endpoints;
using Bookstore.Api.Infrastructure;
using Bookstore.Application.Authors;
using Bookstore.Application.Books;
using Bookstore.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddBookstoreInfrastructure();
builder.Services.AddSingleton<AuthorService>();
builder.Services.AddSingleton<BookService>();

var app = builder.Build();

app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseMiddleware<RequestTraceMiddleware>();
app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.MapGroup("/api")
    .MapGroup("/books")
    .MapBookEndpoints();
app.MapGroup("/api")
    .MapGroup("/authors")
    .MapAuthorEndpoints();

await app.Services
    .GetRequiredService<MongoDatabaseInitializer>()
    .InitializeAsync(app.Lifetime.ApplicationStopping);

await app.RunAsync();

public partial class Program
{
}
