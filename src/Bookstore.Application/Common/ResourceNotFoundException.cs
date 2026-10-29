namespace Bookstore.Application.Common;

public sealed class ResourceNotFoundException(string resourceName, string identifier)
    : Exception($"{resourceName} '{identifier}' was not found.");
