using Bookstore.Api.Endpoints;
using Bookstore.Api.Infrastructure;
using Bookstore.Api.Validation;
using Bookstore.Application.Authors;
using Bookstore.Application.Books;
using Bookstore.Infrastructure.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, loggerConfiguration) =>
    loggerConfiguration
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter()));
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddBookstoreInfrastructure();
builder.Services.AddSingleton<AuthorService>();
builder.Services.AddSingleton<BookService>();
builder.Services.AddScoped<IValidator<CreateAuthorRequest>, CreateAuthorRequestValidator>();
builder.Services.AddScoped<IValidator<CreateBookRequest>, CreateBookRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateBookRequest>, UpdateBookRequestValidator>();

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
