namespace Bookstore.Application.Authors;

public sealed class AuthorAlreadyExistsException(string normalizedName)
    : Exception($"An Author named '{normalizedName}' already exists.");
