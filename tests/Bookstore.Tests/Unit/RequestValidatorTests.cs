using Bookstore.Api.Endpoints;
using Bookstore.Api.Validation;

namespace Bookstore.Tests.Unit;

public sealed class RequestValidatorTests
{
    [Fact]
    public void CreateAuthorRequest_ValidProfile_PassesValidation()
    {
        var result = new CreateAuthorRequestValidator().Validate(
            new CreateAuthorRequest(
                "Octavia Butler",
                Website: "https://example.test/author",
                Email: "author@example.test"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(" ", null, null, "Name is required.")]
    [InlineData("Valid Name", "ftp://example.test", null, "Website must be an absolute HTTP or HTTPS URL.")]
    [InlineData("Valid Name", null, "not-an-email", "Email must be a valid email address.")]
    public void CreateAuthorRequest_InvalidProfile_FailsValidation(
        string name,
        string? website,
        string? email,
        string expectedMessage)
    {
        var result = new CreateAuthorRequestValidator().Validate(
            new CreateAuthorRequest(name, Website: website, Email: email));

        Assert.Contains(result.Errors, error => error.ErrorMessage == expectedMessage);
    }

    [Fact]
    public void CreateAuthorRequest_NameExceedsMaximumLength_FailsValidation()
    {
        var result = new CreateAuthorRequestValidator().Validate(
            new CreateAuthorRequest(new string('A', 256)));

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == "Name must not exceed 255 characters.");
    }

    [Fact]
    public void CreateBookRequest_InvalidTitleAndCategory_FailsValidation()
    {
        var result = new CreateBookRequestValidator().Validate(
            new CreateBookRequest(" ", CategoryId: "Unknown"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == "Title is required.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Category is not supported.");
    }

    [Fact]
    public void UpdateBookRequest_InvalidId_FailsValidation()
    {
        var result = new UpdateBookRequestValidator().Validate(new UpdateBookRequest("bad", 1, true));

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == "Book ID must be a 24-character hexadecimal ObjectId.");
    }
}
