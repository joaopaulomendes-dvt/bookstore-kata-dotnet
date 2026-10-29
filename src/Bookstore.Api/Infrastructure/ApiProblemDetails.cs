namespace Bookstore.Api.Infrastructure;

public sealed record ApiProblemDetails(
    string Type,
    string Title,
    int Status,
    string Detail,
    string Instance,
    string TraceId);

public static class ApiProblemMetadata
{
    public static RouteHandlerBuilder ProducesApiProblem(
        this RouteHandlerBuilder builder,
        int statusCode)
    {
        return builder.Produces<ApiProblemDetails>(statusCode, "application/problem+json");
    }
}
