using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;
using Microsoft.AspNetCore.Diagnostics;

namespace DoubleStar.Api.Common.Errors;

/// <summary>Turns any exception that escapes the pipeline into the standard API error envelope.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // The caller disconnected mid-request: nobody is listening and nothing is wrong.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path} (trace {TraceId})",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new ApiResponse<object>(
                false,
                null,
                [new ApiError(
                    "Server.Unexpected",
                    $"Something went wrong on our side. Quote this reference if you contact support: {httpContext.TraceIdentifier}",
                    "Failure")]),
            cancellationToken);

        return true;
    }
}