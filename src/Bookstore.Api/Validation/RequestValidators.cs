using System.Net.Mail;
using Bookstore.Api.Endpoints;
using Bookstore.Application.Common;
using Bookstore.Domain.Books;
using FluentValidation;

namespace Bookstore.Api.Validation;

public sealed class CreateAuthorRequestValidator : AbstractValidator<CreateAuthorRequest>
{
    public CreateAuthorRequestValidator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required.")
            .Must(name => string.IsNullOrWhiteSpace(name) || name.Trim().Length <= 255)
            .WithMessage("Name must not exceed 255 characters.");

        RuleFor(request => request.Website)
            .Must(IsHttpUrl)
            .When(request => !string.IsNullOrWhiteSpace(request.Website))
            .WithMessage("Website must be an absolute HTTP or HTTPS URL.");

        RuleFor(request => request.Email)
            .Must(IsValidEmail)
            .When(request => !string.IsNullOrWhiteSpace(request.Email))
            .WithMessage("Email must be a valid email address.");
    }

    private static bool IsHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               uri.Scheme is "http" or "https";
    }

    private static bool IsValidEmail(string? value)
    {
        return MailAddress.TryCreate(value, out _);
    }
}

public sealed class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(request => request.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title is required.");

        RuleFor(request => request.AuthorId)
            .Must(ObjectIdText.IsValid)
            .When(request => !string.IsNullOrWhiteSpace(request.AuthorId))
            .WithMessage("Author ID must be a 24-character hexadecimal ObjectId.");

        RuleFor(request => request.CategoryId)
            .Must(category => Categories.FindId(category) is not null)
            .When(request => !string.IsNullOrWhiteSpace(request.CategoryId))
            .WithMessage("Category is not supported.");
    }
}

public sealed class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(request => request.Id)
            .Must(ObjectIdText.IsValid)
            .WithMessage("Book ID must be a 24-character hexadecimal ObjectId.");
    }
}

public static class RequestValidation
{
    public static async Task ValidateRequestAsync<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
