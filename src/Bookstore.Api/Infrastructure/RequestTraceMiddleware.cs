namespace Bookstore.Api.Infrastructure;

public sealed class RequestTraceMiddleware(
    RequestDelegate next,
    ILogger<RequestTraceMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        context.Response.Headers["X-Trace-Id"] = traceId;

        using (logger.BeginScope(new Dictionary<string, object?>
               {
                   ["TraceId"] = traceId
               }))
        {
            await next(context);
        }
    }
}
