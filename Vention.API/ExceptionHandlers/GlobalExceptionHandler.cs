using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Vention.Application.Exceptions;

namespace Vention.API.ExceptionHandlers
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            int statusCode;
            string title;
            string detail;
            string type;

            switch (exception)
            {
                case ValidationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Validation failed";
                    detail = "One or more validation errors occurred.";
                    type = "urn:problem-type:validation-error";
                    break;

                case NotFoundException nf:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Resource not found";
                    detail = nf.Message;
                    type = "urn:problem-type:not-found";
                    break;

                case ArgumentException arg:
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Invalid argument";
                    detail = arg.Message;
                    type = "urn:problem-type:invalid-argument";
                    break;

                case InvalidOperationException op:
                    statusCode = StatusCodes.Status409Conflict;
                    title = "Conflict";
                    detail = op.Message;
                    type = "urn:problem-type:conflict";
                    break;

                case UnauthorizedException unauthorized:
                    statusCode = StatusCodes.Status401Unauthorized;
                    title = "Unauthorized";
                    detail = exception.Message;
                    type = "urn:problem-type:unauthorized";
                    break;

                case ForbiddenException forbidden:
                    statusCode = StatusCodes.Status403Forbidden;
                    title = "Forbidden";
                    detail = exception.Message;
                    type = "urn:problem-type:forbidden";
                    break;

                case FileStorageException storage:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "File storage error";
                    detail = storage.Message;
                    type = "urn:problem-type:file-storage-error";
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "An error occurred while processing your request.";
                    detail = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.";
                    type = "urn:problem-type:internal-server-error";
                    break;
            }

            if (statusCode >= 500)
            {
                _logger.LogError(exception, "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
                    httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);
            }
            else
            {
                _logger.LogWarning(exception, "Handled exception ({Status}) for {Method} {Path}. TraceId: {TraceId}",
                    statusCode, httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = type,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            if (exception is ValidationException ve && ve.Errors.Count > 0)
            {
                problemDetails.Extensions["errors"] = ve.Errors;
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            try
            {
                await httpContext.Response.WriteAsJsonAsync(
                      problemDetails,
                      options: null,
                      contentType: "application/problem+json",
                      cancellationToken: cancellationToken);
            }
            catch (Exception writeEx)
            {
                _logger.LogError(writeEx, "Failed to write ProblemDetails response for TraceId: {TraceId}", httpContext.TraceIdentifier);
            }

            return true;
        }
    }
}