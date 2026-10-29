using System.Text.Json;
using Bookstore.Application.Authors;
using Bookstore.Application.Common;
using Bookstore.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Infrastructure;

public sealed class ProblemDetailsMiddleware(
    RequestDelegate next,
    ILogger<ProblemDetailsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request {TraceId} was canceled by the client.", context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var (status, title, detail) = exception switch
            {
                DomainValidationException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    exception.Message),
                JsonException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    "The request body contains invalid JSON."),
                BadHttpRequestException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    exception.Message),
                ResourceNotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    exception.Message),
                AuthorAlreadyExistsException => (
                    StatusCodes.Status409Conflict,
                    "Author already exists",
                    exception.Message),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred",
                    "The request could not be completed.")
            };

            if (status >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Request {TraceId} failed unexpectedly.", context.TraceIdentifier);
            }
            else
            {
                logger.LogWarning(
                    "Request {TraceId} failed with status {StatusCode}.",
                    context.TraceIdentifier,
                    status);
            }

            var problem = new ProblemDetails
            {
                Type = "about:blank",
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.TraceIdentifier
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
            context.Response.ContentType = "application/problem+json";
            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                problem,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                context.RequestAborted);
        }
    }
}
